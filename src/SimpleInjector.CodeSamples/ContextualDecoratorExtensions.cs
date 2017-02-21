namespace SimpleInjector.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using SimpleInjector.Advanced;

    public static class ContextualDecoratorExtensions
    {
        private static readonly object PredicateCollectionKey = new object();

        public static void EnableContextualDecoratorSupport(this ContainerOptions options)
        {
            if (GetContextualPredicates(options.Container) == null)
            {
                var predicates = new ContextualPredicateCollection();

                options.DependencyInjectionBehavior =
                    new ContextualDecoratorInjectionBehavior(options.Container, predicates);

                options.Container.SetItem(PredicateCollectionKey, predicates);
            }
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
                throw new InvalidOperationException(
                    "Conditional decorator " + decoratorType.ToFriendlyName() + " hasn't been applied to " +
                    "type " + c.ServiceType.ToFriendlyName() + ". Make sure that all registered " +
                    "decorators that wrap this decorator are transient and don't depend on " +
                    "Func<" + c.ServiceType.ToFriendlyName() + "> and that " + 
                    c.ServiceType.ToFriendlyName() + " is not resolved as root type.");
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
            private readonly Container container;
            private readonly ContextualPredicateCollection contextualPredicates;
            private readonly IDependencyInjectionBehavior defaultBehavior;

            public ContextualDecoratorInjectionBehavior(Container container,
                ContextualPredicateCollection contextualPredicates)
            {
                this.container = container;
                this.contextualPredicates = contextualPredicates;
                this.defaultBehavior = container.Options.DependencyInjectionBehavior;
            }

            public void Verify(InjectionConsumerInfo consumer)
            {
                this.defaultBehavior.Verify(consumer);
            }

            public InstanceProducer GetInstanceProducer(InjectionConsumerInfo consumer, bool throwOnFailure)
            {
                InstanceProducer producer = this.defaultBehavior.GetInstanceProducer(consumer, throwOnFailure);

                List<PredicatePair> pairs;

                if (this.MustApplyContextualDecorator(consumer.Target.TargetType, out pairs))
                {
                    return this.ApplyDecorator(consumer.Target, producer.BuildExpression(), pairs);
                }

                return producer;
            }

            private InstanceProducer ApplyDecorator(InjectionTargetInfo target, Expression expression,
                List<PredicatePair> predicatePairs)
            {
                var visitor = new ContextualDecoratorExpressionVisitor(target, predicatePairs);

                expression = visitor.Visit(expression);

                if (!visitor.AllContextualDecoratorsApplied)
                {
                    throw new InvalidOperationException("Couldn't apply the contextual decorator " +
                        visitor.UnappliedDecorators.Last().ToFriendlyName() + ". Make sure that all " +
                        "registered decorators that wrap this decorator are transient and don't depend on " +
                        "Func<" + target.TargetType.ToFriendlyName() + ">.");
                }

                return InstanceProducer.FromExpression(target.TargetType, expression, this.container);
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

            public bool AllContextualDecoratorsApplied => !this.UnappliedDecorators.Any();

            public IEnumerable<Type> UnappliedDecorators =>
                this.predicatePairs.Except(this.appliedPairs).Select(p => p.DecoratorType);

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                PredicatePair predicatePair = this.GetPredicateToInsert(node);

                if (predicatePair != null)
                {
                    this.appliedPairs.Add(predicatePair);
                    bool shouldApplyDecorator = predicatePair.ContextualPredicate(this.target);
                    var expression = shouldApplyDecorator ? node.IfTrue : node.IfFalse;
                    return this.Visit(expression);
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
                var constant = test?.Expression as ConstantExpression;
                return constant?.Value as Predicate<DecoratorPredicateContext>;
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