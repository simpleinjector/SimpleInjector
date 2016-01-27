﻿namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using SimpleInjector.Advanced;
    using SimpleInjector.Extensions;

    public static class ContextualDecoratorExtensions
    {
        private static readonly object PredicateCollectionKey = new object();

        public static void EnableContextualDecoratorSupport(this ContainerOptions options)
        {
            var predicates = new ContextualPredicateCollection();

            if (options.Container.GetItem(PredicateCollectionKey) != null)
            {
                throw new InvalidOperationException("EnableContextualDecoratorSupport can't be called twice.");
            }

            options.DependencyInjectionBehavior =
                new ContextualDecoratorInjectionBehavior(options.Container, predicates);

            options.Container.SetItem(PredicateCollectionKey, predicates);
        }

        public static void RegisterContextualDecorator(this Container container, Type serviceType, 
            Type decoratorType, Predicate<InjectionTargetInfo> contextualPredicate)
        {
            var predicates = GetContextualPredicates(container);

            if (predicates == null)
            {
                throw new InvalidOperationException(
                    "Please call container.Options.EnableContextualDecoratorSupport() first.");
            }

            Predicate<DecoratorPredicateContext> predicateToReplace = c =>
            {
                throw new InvalidOperationException("Conditional decorator " + decoratorType.FullName + 
                    " hasn't been applied to type " + c.ServiceType.FullName + ". Make sure that all " +
                    "registered decorators that wrap this decorator are transient and don't depend on " +
                    "Func<" + c.ServiceType.FullName + "> and that " + c.ServiceType + " is not resolved " +
                    "as root type.");
            };

            container.RegisterRuntimeDecorator(serviceType, decoratorType, predicateToReplace);

            predicates.Add(serviceType, 
                new PredicatePair(decoratorType, predicateToReplace, contextualPredicate));
        }

        private static ContextualPredicateCollection GetContextualPredicates(Container container)
        {
            return (ContextualPredicateCollection)container.GetItem(PredicateCollectionKey);
        }

        private sealed class ContextualDecoratorInjectionBehavior : IDependencyInjectionBehavior
        {
            private readonly ContextualPredicateCollection contextualPredicates;
            private readonly IDependencyInjectionBehavior defaultBehavior;

            public ContextualDecoratorInjectionBehavior(Container container, 
                ContextualPredicateCollection contextualPredicates)
            {
                this.contextualPredicates = contextualPredicates;
                this.defaultBehavior = container.Options.DependencyInjectionBehavior;
            }

            public void Verify(InjectionConsumerInfo consumer)
            {
                this.defaultBehavior.Verify(consumer);
            }

            public Expression BuildExpression(InjectionConsumerInfo consumer)
            {
                var expression = this.defaultBehavior.BuildExpression(consumer);

                List<PredicatePair> predicatePairs;

                if (this.MustApplyContextualDecorator(consumer.Target.TargetType, out predicatePairs))
                {
                    var visitor = new ContextualDecoratorExpressionVisitor(consumer.Target, predicatePairs);

                    expression = visitor.Visit(expression);

                    if (!visitor.AllContextualDecoratorsApplied)
                    {
                        throw new InvalidOperationException("Couldn't apply the contextual decorator " + 
                            visitor.UnappliedDecorators.Last().FullName + ". Make sure that all registered " +
                            "decorators that wrap this decorator are transient and don't depend on " +
                            "Func<" + consumer.Target.TargetType.FullName + ">.");
                    }
                }

                return expression;
            }

            private bool MustApplyContextualDecorator(Type serviceType, out List<PredicatePair> predicatePairs)
            {
                predicatePairs = (
                    from key in this.contextualPredicates.ServiceTypes
                    where key == serviceType || (
                        key.IsGenericTypeDefinition && serviceType.IsGenericType &&
                        key == serviceType.GetGenericTypeDefinition())
                    select this.contextualPredicates[key])
                    .SingleOrDefault();

                return predicatePairs != null;
            }
        }

        private sealed class ContextualDecoratorExpressionVisitor : ExpressionVisitor
        {
            private readonly InjectionTargetInfo target;
            private readonly List<PredicatePair> predicatePairs;
            private readonly List<PredicatePair> appliedPairs = new List<PredicatePair>();

            public ContextualDecoratorExpressionVisitor(InjectionTargetInfo target, 
                List<PredicatePair> predicatePairs)
            {
                this.target = target;
                this.predicatePairs = predicatePairs;
            }

            public bool AllContextualDecoratorsApplied 
            {
                get { return !this.UnappliedDecorators.Any(); }
            }

            public IEnumerable<Type> UnappliedDecorators
            {
                get { return this.predicatePairs.Except(this.appliedPairs).Select(p => p.DecoratorType); }
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                var predicatePair = this.GetPredicateToInsert(node);

                if (predicatePair != null)
                {
                    this.appliedPairs.Add(predicatePair);

                    return this.Visit(predicatePair.ContextualPredicate(this.target) ? node.IfTrue : node.IfFalse);
                }

                return base.VisitConditional(node);
            }

            private PredicatePair GetPredicateToInsert(ConditionalExpression node)
            {
                var predicateToReplace = GetTestPredicate(node);

                return (
                    from pair in this.predicatePairs
                    where object.ReferenceEquals(pair.PredicateToReplace, predicateToReplace)
                    select pair)
                    .SingleOrDefault();
            }

            private static Predicate<DecoratorPredicateContext> GetTestPredicate(ConditionalExpression node)
            {
                var test = node.Test as InvocationExpression;

                if (test != null)
                {
                    var constant = test.Expression as ConstantExpression;

                    if (constant != null)
                    {
                        return constant.Value as Predicate<DecoratorPredicateContext>;
                    }
                }

                return null;
            }
        }

        private sealed class ContextualPredicateCollection
        {
            private readonly Dictionary<Type, List<PredicatePair>> dictionary = 
                new Dictionary<Type, List<PredicatePair>>();

            public IEnumerable<Type> ServiceTypes
            {
                get { return this.dictionary.Keys; }
            }

            public List<PredicatePair> this[Type serviceType]
            {
                get { return this.dictionary[serviceType]; }
            }

            public void Add(Type key, PredicatePair value)
            {
                if (!this.dictionary.ContainsKey(key))
                {
                    this.dictionary[key] = new List<PredicatePair>();
                }

                this.dictionary[key].Add(value);
            }        
        }

        private sealed class PredicatePair
        {
            internal readonly Type DecoratorType;
            internal readonly Predicate<DecoratorPredicateContext> PredicateToReplace;
            internal readonly Predicate<InjectionTargetInfo> ContextualPredicate;

            public PredicatePair(Type decoratorType, Predicate<DecoratorPredicateContext> predicateToReplace,
                Predicate<InjectionTargetInfo> contextualPredicate)
            {
                this.DecoratorType = decoratorType;
                this.PredicateToReplace = predicateToReplace;
                this.ContextualPredicate = contextualPredicate;
            }
        }
    }
}