namespace SimpleInjector.CodeSamples
{
    // http://simpleinjector.codeplex.com/wikipage?title=PerWebRequestExtensionMethod
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Web;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for registering types on a per web request basis.
    /// </summary>
    public static partial class SimpleInjectorPerWebRequestExtensions
    {
        [DebuggerStepThrough]
        public static void RegisterPerWebRequest<TService, TImplementation>(
            this Container container)
            where TService : class
            where TImplementation : class, TService
        {
            Func<TService> instanceCreator =
                () => container.GetInstance<TImplementation>();

            container.RegisterPerWebRequest<TService>(instanceCreator);
        }

        [DebuggerStepThrough]
        public static void RegisterPerWebRequest<TService>(this Container container,
            Func<TService> instanceCreator) where TService : class
        {
            var creator =
                new PerWebRequestInstanceCreator<TService>(instanceCreator);

            container.Register<TService>(creator.GetInstance);
        }

        [DebuggerStepThrough]
        public static void RegisterPerWebRequest<TConcrete>(this Container container)
            where TConcrete : class
        {
            // Register the type as transient. This prevents it from being registered 
            // twice and allows us to hook onto the ExpressionBuilt event.
            container.Register<TConcrete>();

            container.ExpressionBuilt += (sender, e) =>
            {
                // e.Expression contains an new TConcrete(...) call where that type is
                // auto-wired by the container. There is no other way than using
                // ExpressionBuilt to get this auto-wired instance.
                if (e.RegisteredServiceType == typeof(TConcrete))
                {
                    // Extract a Func<T> delegate for creating the transient TConcrete.
                    var transientInstanceCreator = Expression.Lambda<Func<TConcrete>>(
                        e.Expression, new ParameterExpression[0]).Compile();

                    var creator = new PerWebRequestInstanceCreator<TConcrete>(
                        transientInstanceCreator);

                    // Swap the original expression so that the lifetime becomes per
                    // web request.
                    e.Expression = Expression.Call(Expression.Constant(creator),
                        creator.GetType().GetMethod("GetInstance"));
                }
            };
        }

        [DebuggerStepThrough]
        public static void DisposeInstance<TService>() where TService : class
        {
            object key = typeof(PerWebRequestInstanceCreator<TService>);

            var instance = HttpContext.Current.Items[key] as IDisposable;

            if (instance != null)
            {
                instance.Dispose();
            }
        }

        private sealed class PerWebRequestInstanceCreator<T> where T : class
        {
            private readonly Func<T> instanceCreator;

            internal PerWebRequestInstanceCreator(Func<T> instanceCreator)
            {
                this.instanceCreator = instanceCreator;
            }

            [DebuggerStepThrough]
            public T GetInstance()
            {
                var context = HttpContext.Current;

                if (context == null)
                {
                    // No HttpContext: Let's create a transient object.
                    return this.instanceCreator();
                }

                object key = this.GetType();

                T instance = (T)context.Items[key];

                if (instance == null)
                {
                    context.Items[key] = instance = this.instanceCreator();
                }

                return instance;
            }
        }
    }
}