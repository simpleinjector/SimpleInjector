#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
 * associated documentation files (the "Software"), to deal in the Software without restriction, including 
 * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
 * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace SimpleInjector.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using SimpleInjector.Advanced.Internal;
    using SimpleInjector.Lifestyles;

    internal static partial class CompilationHelpers
    {
        private static readonly ConstructorInfo LazyScopeConstructor =
            Helpers.GetConstructor(() => new LazyScope(null, null));

        private static readonly MethodInfo CreateConstantValueDelegateMethod = 
            Helpers.GetGenericMethodDefinition(() => CreateConstantValueDelegate<object>(null));

        // NOTE: This method should be public. It is called using reflection.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Func<TResult> CreateConstantValueDelegate<TResult>(Expression expression)
        {
            object value = ((ConstantExpression)expression).Value;

            var singleton = (TResult)value;

            // This lambda will be a tiny little bit faster than a compiled delegate and 
            return () => singleton;
        }

        // Compile the expression. If the expression is compiled in a dynamic assembly, the compiled delegate
        // is called (to ensure that it will run, because it tends to fail now and then) and the created
        // instance is returned through the out parameter. Note that NO created instance will be returned when
        // the expression is compiled using Expression.Compile)(.
        internal static Func<TResult> CompileExpression<TResult>(Container container, Expression expression)
        {
            // Skip compiling if all we need to do is return a singleton.
            if (expression is ConstantExpression)
            {
                return CreateConstantValueDelegate<TResult>(expression);
            }

            return (Func<TResult>)CompileExpression(typeof(TResult), container, expression, null);
        }

        internal static Delegate CompileExpression(Type resultType, Container container, Expression expression,
            Dictionary<Expression, InvocationExpression> reducedNodes = null)
        {
            if (expression is ConstantExpression)
            {
                return (Delegate)CreateConstantValueDelegateMethod.MakeGenericMethod(resultType)
                    .Invoke(null, new[] { expression });
            }

            // Reduce the size of the object graph to prevent the CLR from throwing stack overflow exceptions.
            expression = ReduceObjectGraphSize(expression, container, reducedNodes);

            expression = OptimizeScopedRegistrationsInObjectGraph(container, expression);

            container.Options.ExpressionCompiling(expression);

            Delegate compiledLambda = null;

            // In the common case, the developer will/should only create a single container during the 
            // lifetime of the application (this is the recommended approach). In this case, we can optimize
            // the performance by compiling delegates in an dynamic assembly. We can't do this when the 
            // developer creates many containers, because this will create a memory leak (dynamic assemblies
            // are never unloaded).
            if (container.Options.EnableDynamicAssemblyCompilation)
            {
                TryCompileInDynamicAssembly(resultType, expression, ref compiledLambda);
            }

            return compiledLambda ?? CompileLambda(resultType, expression);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Expression OptimizeScopedRegistrationsInObjectGraph(Container container,
            Expression expression)
        {
            var lifestyleInfos = PerObjectGraphOptimizableRegistrationFinder.Find(expression, container);

            if (lifestyleInfos.Any())
            {
                return OptimizeExpression(container, expression, lifestyleInfos);
            }

            return expression;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Func<TService> CompileLambda<TService>(Expression expression) =>
            Expression.Lambda<Func<TService>>(expression).Compile();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Delegate CompileLambda(Type resultType, Expression expression)
        {
            Type delegateType = typeof(Func<>).MakeGenericType(resultType);
            return Expression.Lambda(delegateType, expression).Compile();
        }

        static partial void TryCompileInDynamicAssembly(Type resultType, Expression expression,
            ref Delegate compiledLambda);

        // OptimizeExpression will implement caching of the scopes of ScopedLifestyles which will optimize
        // performance in case multiple scoped registrations are used within a single delegate. Here's an
        // example of how the expression gets optimized:
        // Before:
        // Func<HomeController> factory = () =>
        // {
        //     return new HomeController(
        //         reg1.GetInstance(), // Hits ThreadLocal, hits dictionary
        //         new SomeQueryHandler(reg1.GetInstance()), // Hits ThreadLocal, hits dictionary
        //         new SomeCommandHandler(reg2.GetInstance())); // Hits ThreadLocal, hits dictionary
        // };
        // After: 
        // Func<HomeController> factory = () =>
        // {
        //     var scope1 = new LazyScope(scopeFactory1, container);
        //     var value1 = new LazyScopedRegistration<IRepository, RepositoryImpl>(reg1);
        //     var value2 = new LazyScopedRegistration<IService, ServiceImpl>(reg2);
        //
        //     return new HomeController(
        //         value1.GetInstance(scope1.Value), // Hits ThreadLocal, hits dictionary
        //         new SomeQueryHandler(value1.GetInstance(scope1.Value)), // hits local cache
        //         new SomeCommandHandler(value2.GetInstance(scope1.Value))); // Hits dictionary
        // };
        private static Expression OptimizeExpression(Container container, Expression expression,
            OptimizableLifestyleInfo[] lifestyleInfos)
        {
            var lifestyleAssigmentExpressions = (
                from lifestyleInfo in lifestyleInfos
                let scopeFactory = lifestyleInfo.Lifestyle.CreateCurrentScopeProvider(container)
                let newExpression = CreateNewLazyScopeExpression(scopeFactory, container)
                select Expression.Assign(lifestyleInfo.Variable, newExpression))
                .ToArray();

            var registrationInfos = (
                from lifestyleInfo in lifestyleInfos
                from registrationInfo in lifestyleInfo.Registrations
                select registrationInfo)
                .ToArray();

            var registrationAssignmentExpressions = (
                from registrationInfo in registrationInfos
                let newExpression = CreateNewLazyScopedRegistration(registrationInfo.Registration)
                select Expression.Assign(registrationInfo.Variable, newExpression))
                .ToArray();

            var optimizedExpression = ObjectGraphOptimizerExpressionVisitor.Optimize(expression, registrationInfos);

            return Expression.Block(
                variables: lifestyleInfos.Select(l => l.Variable)
                    .Concat(registrationInfos.Select(r => r.Variable)),
                expressions: lifestyleAssigmentExpressions
                    .Concat(registrationAssignmentExpressions
                        .Concat(new[] { optimizedExpression })));
        }

        private static NewExpression CreateNewLazyScopeExpression(Func<Scope> scopeFactory, Container container) =>
            Expression.New(
                LazyScopeConstructor,
                Expression.Constant(scopeFactory, typeof(Func<Scope>)),
                Expression.Constant(container, typeof(Container)));

        private static NewExpression CreateNewLazyScopedRegistration(Registration registration)
        {
            Type type = typeof(LazyScopedRegistration<>)
                .MakeGenericType(registration.GetType().GetGenericArguments());

            return Expression.New(
                type.GetConstructor(new[] { typeof(Registration) }),
                Expression.Constant(registration, typeof(Registration)));
        }

        // Reduces the size of a supplied expression tree by compiling parts of the tree into individual
        // delegates. Besides preventing the CLR from throwing stack overflow exceptions (which will happen
        // when the tree gets somewhere between 20,000 and 50,000 nodes), this can reduce the amount of code 
        // that needs to be JITted and can therefore reduce the memory footprint of the application.
        private static Expression ReduceObjectGraphSize(Expression expression, Container container,
            Dictionary<Expression, InvocationExpression> reducedNodes = null)
        {
            var results = NodeSizeCalculator.Calculate(expression);

            while (results.TotalSize > container.Options.MaximumNumberOfNodesPerDelegate)
            {
                reducedNodes = reducedNodes ?? new Dictionary<Expression, InvocationExpression>(16);

                Expression mostReductiveNode = FindMostReductiveNodeOrNull(results,
                    container.Options.MaximumNumberOfNodesPerDelegate);

                if (mostReductiveNode == null)
                {
                    // In case mostReductiveNode is null, there's no good candidate to reduce the object
                    // graph. In that case we break out.
                    break;
                }

                // If the found mostReductiveNode has been reduced before, we use the previously compiled
                // value (instead of doing the compilation all over again). Otherwise we compile that node.
                InvocationExpression replacementNode = reducedNodes.ContainsKey(mostReductiveNode)
                    ? reducedNodes[mostReductiveNode]
                    : CompileToInvocation(mostReductiveNode, container, reducedNodes);

                reducedNodes[mostReductiveNode] = replacementNode;

                expression = NodeReplacer.Replace(expression, oldNode: mostReductiveNode, newNode: replacementNode);

                results = NodeSizeCalculator.Calculate(expression);
            }

            return expression;
        }

        private static InvocationExpression CompileToInvocation(Expression expression, Container container,
            Dictionary<Expression, InvocationExpression> reducedNodes)
        {
            // Here we compile the expression. This will recursively reduce this sub graph again. The already
            // reduced nodes are passed on; hopefully they can be reused while reducing this sub graph.
            Delegate compiledDelegate = CompileExpression(expression.Type, container, expression, reducedNodes);
            return Expression.Invoke(Expression.Constant(compiledDelegate));
        }

        private static Expression FindMostReductiveNodeOrNull(NodeSizes results,
            int maximumNumberOfNodesPerDelegate)
        {
            // By setting a maximum size, we prevent that the one of the root nodes will be selected as most
            // reductive node. Although this would reduce the expression to a few nodes, this will leave us
            // with a new expression that is as big or almost as big, and might even cause a stack overflow.
            int maximumSizeOfReducibleNode = maximumNumberOfNodesPerDelegate * 9 / 10; // 90%
            int maximumSizeOfNode = results.TotalSize - maximumSizeOfReducibleNode;

            // We must set a minimum size for the nodes to prevent selecting nodes with just a few sub nodes
            // (such as CallExpression and InvocationExpression nodes that capture a ConstantExpression),
            // because that would reduce them with new expressions of the same size, and would cause us to
            // keep looping infinitely without reducing the size of the expression.
            // Although we could lower the size to 4 or 5, this could cause those small trees (that are
            // always leaf structures) to be selected, while we prefer reducing big object graphs first,
            // because replacing a leaf node will cause all its parents to be replaced, making it impossible
            // to detect duplicate reduced expressions (see the ReduceObjectGraphSize method). This makes it
            // much less likely to be able to reuse a compiled delegate.
            int minimumSizeOfNode = 10;

            // Here we sort not only by total cost, but in case there are multiple nodes with the same total
            // cost, we prefer the node with the biggest size. This has the same reason as explained above.
            var nodesByTotalCost =
                from info in results.Nodes
                where info.TreeSize <= maximumSizeOfNode && info.TreeSize >= minimumSizeOfNode
                orderby info.TotalCost descending, info.TreeSize descending
                select info;

            var nodeWithLargestCost = nodesByTotalCost.FirstOrDefault();

            return nodeWithLargestCost != null ? nodeWithLargestCost.Node : null;
        }

        private sealed class PerObjectGraphOptimizableRegistrationFinder : ExpressionVisitor
        {
            private readonly List<OptimizableRegistrationInfo> perObjectGraphRegistrations =
                new List<OptimizableRegistrationInfo>();

            private Container container;

            public static OptimizableLifestyleInfo[] Find(Expression expression, Container container)
            {
                var finder = new PerObjectGraphOptimizableRegistrationFinder();

                finder.container = container;

                finder.Visit(expression);

                var lifestyleGroups = finder.perObjectGraphRegistrations
                    .GroupBy(r => r.Registration.Lifestyle, ReferenceEqualityComparer<Lifestyle>.Instance);

                return (
                    from lifestyleGroup in lifestyleGroups
                    select new OptimizableLifestyleInfo(
                        (ScopedLifestyle)lifestyleGroup.Key,
                        lifestyleGroup.GroupBy(info => info.Registration,
                            ReferenceEqualityComparer<Registration>.Instance)
                            .Select(g => g.First())))
                    .ToArray();
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                Registration registration = this.GetScopedRegistration(node);

                if (registration != null)
                {
                    this.perObjectGraphRegistrations.Add(new OptimizableRegistrationInfo(registration, node));
                }

                return base.VisitMethodCall(node);
            }

            private Registration GetScopedRegistration(MethodCallExpression node)
            {
                var instance = node.Object as ConstantExpression;

                var registration = instance != null ? instance.Value as Registration : null;

                if (registration != null && object.ReferenceEquals(registration.Container, this.container))
                {
                    Type type = registration.GetType();

                    if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(ScopedRegistration<>))
                    {
                        return registration;
                    }
                }

                return null;
            }
        }

        private sealed class ObjectGraphOptimizerExpressionVisitor : ExpressionVisitor
        {
            private OptimizableRegistrationInfo[] registrationsToOptimize;

            public static Expression Optimize(Expression expression,
                OptimizableRegistrationInfo[] registrationsToOptimize)
            {
                var optimizer = new ObjectGraphOptimizerExpressionVisitor();

                optimizer.registrationsToOptimize = registrationsToOptimize;

                return optimizer.Visit(expression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var registration = this.registrationsToOptimize.FirstOrDefault(r => r.OriginalExpression == node);

                if (registration != null)
                {
                    return registration.LazyScopeRegistrationGetInstanceExpression;
                }

                return base.VisitMethodCall(node);
            }
        }

        private sealed class OptimizableLifestyleInfo
        {
            internal readonly ScopedLifestyle Lifestyle;
            internal readonly ParameterExpression Variable;
            internal readonly IEnumerable<OptimizableRegistrationInfo> Registrations;

            public OptimizableLifestyleInfo(ScopedLifestyle lifestyle,
                IEnumerable<OptimizableRegistrationInfo> registrations)
            {
                this.Lifestyle = lifestyle;
                this.Registrations = registrations.ToArray();
                this.Variable = Expression.Variable(typeof(LazyScope));

                this.InitializeRegistrations();
            }

            private void InitializeRegistrations()
            {
                foreach (var registration in this.Registrations)
                {
                    registration.LifestyleInfo = this;
                }
            }
        }

        private sealed class OptimizableRegistrationInfo
        {
            internal readonly Registration Registration;
            internal readonly MethodCallExpression OriginalExpression;

            private readonly Type lazyScopeRegistrationType;

            private ParameterExpression variable;

            internal OptimizableRegistrationInfo(Registration registration,
                MethodCallExpression originalExpression)
            {
                this.Registration = registration;
                this.OriginalExpression = originalExpression;

                this.lazyScopeRegistrationType = typeof(LazyScopedRegistration<>)
                    .MakeGenericType(this.Registration.GetType().GetGenericArguments());
            }

            internal ParameterExpression Variable
            {
                get
                {
                    if (this.variable == null)
                    {
                        this.variable = Expression.Variable(this.lazyScopeRegistrationType);
                    }

                    return this.variable;
                }
            }

            internal OptimizableLifestyleInfo LifestyleInfo { get; set; }

            internal Expression LazyScopeRegistrationGetInstanceExpression =>
                Expression.Call(
                    this.Variable,
                    this.lazyScopeRegistrationType.GetMethod("GetInstance"),
                    Expression.Property(this.LifestyleInfo.Variable, "Value"));
        }

        [DebuggerDisplay("TotalCost: {TotalCost}, Count: {Count}, NumberOfNodes: {NumberOfNodes}, Node: {Node}")]
        private sealed class ExpressionInfo
        {
            public Expression Node { get; set; }
            public int Count { get; set; }
            public int TreeSize { get; set; }
            public int TotalCost => this.Count * this.TreeSize;
        }

        private sealed class NodeSizes
        {
            public int TotalSize { get; set; }
            public ICollection<ExpressionInfo> Nodes { get; set; }
        }

        private sealed class NodeSizeCalculator : ExpressionVisitor
        {
            private readonly Dictionary<Expression, ExpressionInfo> nodes =
                new Dictionary<Expression, ExpressionInfo>(ReferenceEqualityComparer<Expression>.Instance);

            private int size;

            public static NodeSizes Calculate(Expression node)
            {
                var calculator = new NodeSizeCalculator();
                calculator.Visit(node);
                return new NodeSizes { TotalSize = calculator.size, Nodes = calculator.nodes.Values };
            }

            public override Expression Visit(Expression node)
            {
                // Weird: node can be null: CallExpression.Object can be null.
                if (node != null)
                {
                    var info = this.GetInfo(node);
                    info.Count++;

                    int old = this.size;

                    if (info.TreeSize == 0)
                    {
                        this.size++;
                        base.Visit(node);
                        info.TreeSize = this.size - old;
                    }
                    else
                    {
                        // We already visited this part of the tree. Just increase the total size with the
                        // already calculated size of this part of the tree.
                        this.size += info.TreeSize;
                    }
                }

                return node;
            }

            private ExpressionInfo GetInfo(Expression node)
            {
                ExpressionInfo info;

                return this.nodes.TryGetValue(node, out info)
                    ? info
                    : this.nodes[node] = new ExpressionInfo { Node = node };
            }
        }

        private sealed class NodeReplacer : ExpressionVisitor
        {
            private Expression oldNode;
            private Expression newNode;

            public static Expression Replace(Expression expression, Expression oldNode, Expression newNode)
            {
                var replacer = new NodeReplacer { oldNode = oldNode, newNode = newNode };
                return replacer.Visit(expression);
            }

            public override Expression Visit(Expression node)
            {
                return object.ReferenceEquals(node, this.oldNode) ? this.newNode : base.Visit(node);
            }
        }
    }
}