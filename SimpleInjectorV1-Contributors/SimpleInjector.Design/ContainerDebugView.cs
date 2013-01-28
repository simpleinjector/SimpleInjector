//#region Copyright (c) 2010 S. van Deursen
///* The Simple Injector is an easy-to-use Inversion of Control library for .NET
// * 
// * Copyright (C) 2010 S. van Deursen
// * 
// * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
// * cuttingedge.it.
// *
// * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// * associated documentation files (the "Software"), to deal in the Software without restriction, including 
// * without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// * copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
// * following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in all copies or substantial 
// * portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO 
// * EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
// * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
// * USE OR OTHER DEALINGS IN THE SOFTWARE.
//*/
//#endregion

//namespace SimpleInjector.Design
//{
//    using System.Collections;
//    using System.Diagnostics;
//    using System.Linq;
//    using System;

//    internal sealed class ContainerAnalysis
//    {
//        public object GetInvalidRegistrations()
//        {

//        }

//        public object GetPotentialLifetimeMismatches()
//        {

//        }

//        [DebuggerDisplay("ServiceType = {SimpleInjector.Helpers.ToFriendlyName(ServiceType),nq}, Message = {Message}")]
//        internal class ErrorInfo
//        {
//            public Type ServiceType { get; set; }

//            public string Message { get; set; }
//        }

//        internal ErrorInfo[] GetErrors()
//        {
//            return (
//                from registration in this.registrations.Values
//                where registration != null
//                where registration.Error != null
//                select new ErrorInfo
//                {
//                    ServiceType = registration.ServiceType,
//                    Message = registration.Error.Message
//                })
//                .ToArray();
//        }

//        [DebuggerDisplay("ServiceType = {SimpleInjector.Helpers.ToFriendlyName(ServiceType),nq}, Message = {Message}")]
//        public class PotentialLifetimeMismatchInfo
//        {
//            public Type ServiceType { get; set; }

//            public string Message { get; set; }

//            [DebuggerDisplay("Dependencies With A Shorter Lifetime")]
//            public Type[] ShorterDependencies { get; set; }
//        }


//        public PotentialLifetimeMismatchInfo[] GetPotentialLifetimeMismatches()
//        {
//            var validRegistrations = (
//                from registration in this.registrations.Values
//                where registration != null
//                where registration.Error == null
//                select registration)
//                .ToArray();

//            return (
//                from registration in validRegistrations
//                let shorterDependencies = (
//                    from dependency in GetDependencies(registration)
//                    where this.DetermineLifeTime(registration) > this.DetermineLifeTime(dependency)
//                    select dependency.ServiceType)
//                    .ToArray()
//                where shorterDependencies.Any()
//                select new PotentialLifetimeMismatchInfo
//                {
//                    ServiceType = registration.ServiceType,
//                    ShorterDependencies = shorterDependencies,
//                    Message = "The registration has dependencies with a lifetime that is shorter. " +
//                        "Dependencies: " + string.Join(", ",
//                            shorterDependencies.Select(t => t.ToFriendlyName()).ToArray())
//                })
//                .ToArray();
//        }

//        private IEnumerable<InstanceProducer> GetDependencies(InstanceProducer registration)
//        {
//            return GetDependenciesRecursive(registration)
//                .Where(dependency => dependency.ServiceType != registration.ServiceType)
//                .Distinct();
//        }

//        private IEnumerable<InstanceProducer> GetDependenciesRecursive(InstanceProducer registration)
//        {
//            foreach (var dependency in registration.runtimeDependencies)
//            {
//                yield return dependency;

//                foreach (var subDependency in GetDependenciesRecursive(dependency))
//                {
//                    yield return subDependency;
//                }
//            }
//        }

//        private int? DetermineLifeTime(InstanceProducer registration)
//        {
//            var expression = registration.BuildExpression();

//            if (IsTransient(expression))
//            {
//                return Lifetimes.Transient;
//            }

//            if (IsPerLifetimeScope(expression))
//            {
//                return Lifetimes.LifetimeScope;
//            }

//            if (IsPerWebRequest(expression))
//            {
//                return Lifetimes.WebRequest;
//            }

//            if (IsSingleton(expression))
//            {
//                return Lifetimes.Singleton;
//            }

//            return Lifetimes.Unknown;
//        }

//        private static bool IsTransient(Expression expression)
//        {
//            if (expression is NewExpression)
//            {
//                return true;
//            }

//            var invocation = expression as InvocationExpression;

//            if (invocation != null && invocation.Expression is ConstantExpression &&
//                invocation.Arguments.Count == 1 && invocation.Arguments[0] is NewExpression)
//            {
//                return true;
//            }

//            return false;
//        }

//        private bool IsPerLifetimeScope(Expression expression)
//        {
//            var invocation = expression as InvocationExpression;

//            if (invocation != null && !invocation.Arguments.Any())
//            {
//                var constant = invocation.Expression as ConstantExpression;

//                object value = constant != null ? constant.Value : null;

//                if (value != null && value.GetType().IsGenericType &&
//                    value.GetType().GetGenericTypeDefinition() == typeof(Func<>))
//                {
//                    var method = value.GetType().GetProperty("Method").GetValue(value, null) as MethodInfo;

//                    if (method != null && method.Name == "CreateScopedInstance" &&
//                        method.Module.Name == "SimpleInjector.Extensions.LifetimeScoping.dll")
//                    {
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        private bool IsPerWebRequest(Expression expression)
//        {
//            var call = expression as MethodCallExpression;

//            return call != null && !call.Arguments.Any() && call.Method != null &&
//                call.Method.Name == "GetInstance" &&
//                call.Method.ReflectedType.Name.StartsWith("PerWebRequest") &&
//                call.Method.Module.Name == "SimpleInjector.Integration.Web.dll";
//        }

//        private static bool IsSingleton(Expression expression)
//        {
//            return expression is ConstantExpression;
//        }

//        private static class Lifetimes
//        {
//            internal static readonly int? Unknown = null;
//            internal static readonly int? Transient = 0;
//            internal static readonly int? LifetimeScope = 1;
//            internal static readonly int? WebRequest = 2;
//            internal static readonly int? Singleton = 3;
//        }
//    }

//    internal sealed class ContainerDebugView
//    {
//        private readonly Container container;

//        public ContainerDebugView(Container container)
//        {
//            this.container = container;

//            this.Items = new DebuggerViewItem[]
//            {
//                new DebuggerViewItem("Registration", this.container.GetCurrentRegistrations()),
//                new DebuggerViewItem("Invalid Registrations", this.container.GetErrors()),
//                new DebuggerViewItem("Potential Lifetime Mismatches", this.container.GetPotentialLifetimeMismatches()),
//            };
//        }

//        public ContainerOptions Options
//        {
//            get { return this.container.Options; }
//        }

//        [DebuggerDisplay("")]
//        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
//        public DebuggerViewItem[] Items { get; private set; }

//    }

//    [DebuggerDisplay("{Description,nq}", Name = "{Name,nq}")]
//    public class DebuggerViewItem
//    {
//        public DebuggerViewItem(string name, IEnumerable value)
//        {
//            this.Name = name;
//            this.Description = "Count = " + value.Cast<object>().Count();
//            this.Value = value;
//        }

//        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//        public object Description { get; private set; }

//        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//        public string Name { get; private set; }

//        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
//        public object Value { get; private set; }
//    }
//}



//namespace SimpleInjector.InstanceProducers
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Diagnostics;
//    using System.Diagnostics.CodeAnalysis;
//    using System.Linq;
//    using System.Linq.Expressions;

//    /// <summary>Base class for producing instances.</summary>
//    [DebuggerDisplay(Helpers.InstanceProviderDebuggerDisplayString)]
//    internal abstract class InstanceProducer : IInstanceProducer
//    {
//        [ThreadStatic]
//        private static Stack<InstanceProducer> stack;

//        internal List<InstanceProducer> runtimeDependencies = new List<InstanceProducer>(0);

//        private readonly object instanceCreationLock = new object();

//        private CyclicDependencyValidator validator;
//        private Func<object> instanceCreator;
//        private Expression expression;
//        private bool? isValid = true;

//        /// <summary>Initializes a new instance of the <see cref="InstanceProducer"/> class.</summary>
//        /// <param name="serviceType">The type of the service this instance will produce.</param>
//        protected InstanceProducer(Type serviceType)
//        {
//            this.ServiceType = serviceType;
//            this.validator = new CyclicDependencyValidator(serviceType);
//        }

//        /// <summary>Gets the service type for which this producer produces instances.</summary>
//        /// <value>A <see cref="Type"/> instance.</value>
//        public Type ServiceType { get; private set; }

//        internal Container Container { get; set; }

//        internal bool IsResolvedThroughUnregisteredTypeResolution
//        {
//            set { this.isValid = value ? null : (bool?)true; }
//        }

//        // Will only return false when the type is a concrete unregistered type that was automatically added
//        // by the container, while the expression can not be generated.
//        // Types that are registered upfront are always considered to be valid, while unregistered types must
//        // be validated. The reason for this is that we must prevent the container to throw an exception when
//        // GetRegistration() is called for an unregistered (concrete) type that can not be resolved.
//        internal bool IsValid
//        {
//            get
//            {
//                if (this.isValid == null)
//                {
//                    this.isValid = this.CanBuildExpression();
//                }

//                return this.isValid.Value;
//            }
//        }

//        internal Exception Error
//        {
//            get
//            {
//                try
//                {
//                    this.GetInstance();

//                    return null;
//                }
//                catch (Exception ex)
//                {
//                    return ex;
//                }
//            }
//        }

//        private void RegisterAsDependencyOfParent()
//        {
//            var stack = InstanceProducer.stack;

//            var current = stack != null && stack.Any() ? stack.Peek() : null;

//            if (current != null)
//            {
//                lock (current.runtimeDependencies)
//                {
//                    if (!current.runtimeDependencies.Contains(this))
//                    {
//                        current.runtimeDependencies.Add(this);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Builds an expression that expresses the intent to get an instance by the current producer.
//        /// </summary>
//        /// <returns>An Expression.</returns>
//        public Expression BuildExpression()
//        {
//            this.validator.CheckForRecursiveCalls();

//            try
//            {
//                this.expression = this.GetExpression();

//                this.RemoveValidator();

//                return this.expression;
//            }
//            catch (Exception ex)
//            {
//                this.validator.Reset();

//                this.ThrowErrorWhileTryingToGetInstanceOfType(ex);

//                throw;
//            }
//        }

//        /// <summary>Produces an instance.</summary>
//        /// <returns>An instance. Will never return null.</returns>
//        /// <exception cref="ActivationException">When the instance could not be retrieved or is null.</exception>
//        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification =
//            "A property is not appropriate, because get instance could possibly be a heavy ")]
//        public object GetInstance()
//        {
//            this.validator.CheckForRecursiveCalls();

//            object instance;

//            try
//            {
//                if (this.instanceCreator == null)
//                {
//                    this.instanceCreator = this.BuildInstanceCreator();
//                }

//                instance = this.instanceCreator();

//                this.RemoveValidator();
//            }
//            catch (Exception ex)
//            {
//                this.validator.Reset();

//                this.ThrowErrorWhileTryingToGetInstanceOfType(ex);

//                throw;
//            }

//            if (instance == null)
//            {
//                throw new ActivationException(this.BuildRegisteredDelegateForTypeReturnedNullExceptionMessage());
//            }

//            return instance;
//        }

//        /// <summary>
//        /// Builds an expression that expresses the intent to get an instance by the current producer.
//        /// </summary>
//        /// <returns>An Expression.</returns>
//        protected abstract Expression BuildExpressionCore();

//        protected virtual string BuildErrorWhileTryingToGetInstanceOfTypeExceptionMessage()
//        {
//            return StringResources.DelegateForTypeThrewAnException(this.ServiceType);
//        }

//        protected virtual string BuildRegisteredDelegateForTypeReturnedNullExceptionMessage()
//        {
//            return StringResources.DelegateForTypeReturnedNull(this.ServiceType);
//        }

//        protected virtual string BuildErrorWhileBuildingDelegateFromExpressionExceptionMessage(
//            Expression expression, Exception exception)
//        {
//            return StringResources.ErrorWhileBuildingDelegateFromExpression(this.ServiceType, expression,
//                exception);
//        }

//        private Func<object> BuildInstanceCreator()
//        {
//            // Don't do recursive checks. The GetInstance() already does that.
//            var expression = this.GetExpression();

//            try
//            {
//                var newInstanceMethod = Expression.Lambda<Func<object>>(expression, new ParameterExpression[0]);

//                return newInstanceMethod.Compile();
//            }
//            catch (Exception ex)
//            {
//                string message = this.BuildErrorWhileBuildingDelegateFromExpressionExceptionMessage(
//                    expression, ex);

//                throw new ActivationException(message, ex);
//            }
//        }

//        private Expression GetExpression()
//        {
//            // Prevent the Expression from being built more than once on this InstanceProducer. Note that this
//            // still means that the expression can be created multiple times for a single service type, because
//            // the container does not guarantee that a single InstanceProducer is created, just as the
//            // ResolveUnregisteredType event can be called multiple times for a single service type.
//            if (this.expression == null)
//            {
//                lock (this.instanceCreationLock)
//                {
//                    if (this.expression == null)
//                    {
//                        this.expression = this.BuildExpressionWithBuildingDependencyGraph();
//                    }
//                }
//            }

//            return this.expression;
//        }

//        private Expression BuildExpressionWithBuildingDependencyGraph()
//        {
//            this.RegisterAsDependencyOfParent();

//            try
//            {
//                var stack = InstanceProducer.stack ?? (InstanceProducer.stack = new Stack<InstanceProducer>());

//                stack.Push(this);

//                return this.BuildExpressionWithInterception();
//            }
//            finally
//            {
//                stack.Pop();
//            }
//        }

//        private Expression BuildExpressionWithInterception()
//        {
//            var expression = this.BuildExpressionCore();

//            var e = new ExpressionBuiltEventArgs(this.ServiceType, expression);

//            this.Container.OnExpressionBuilt(e);

//            return e.Expression;
//        }

//        private void ThrowErrorWhileTryingToGetInstanceOfType(Exception innerException)
//        {
//            string exceptionMessage = this.BuildErrorWhileTryingToGetInstanceOfTypeExceptionMessage();

//            // Prevent wrapping duplicate exceptions.
//            if (!innerException.Message.StartsWith(exceptionMessage, StringComparison.OrdinalIgnoreCase))
//            {
//                throw new ActivationException(exceptionMessage + " " + innerException.Message, innerException);
//            }
//        }

//        // This method will be inlined by the JIT.
//        private void RemoveValidator()
//        {
//            // No recursive calls detected, we can remove the validator to increase performance.
//            // We first check for null, because this is faster. Every time we write, the CPU has to send
//            // the new value to all the other CPUs. We only nullify the validator while using the GetInstance
//            // method, because the BuildExpression will only be called a limited amount of time.
//            if (this.validator != null)
//            {
//                this.validator = null;
//            }
//        }

//        private bool CanBuildExpression()
//        {
//            try
//            {
//                // Test if the instance can be made.
//                this.BuildExpression();

//                return true;
//            }
//            catch (ActivationException)
//            {
//                return false;
//            }
//        }
//    }
//}