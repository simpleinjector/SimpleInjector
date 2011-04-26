using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleInjector.CodeSamples.Tests.Unit
{
    /*
    // Example usage
    var container = new Container();

    // returns a SqlUserRepository decorated by a MonitoringInterceptor.
    container.RegisterWithInterceptor<IUserRepository, MonitoringInterceptor, SqlUserRepository>();

    // Manually: returns a SqlUserRepository decorated by a MonitoringInterceptor.
    container.Register<IUserRepository>(() =>
        Interceptor.CreateProxy<IUserRepository>(
            container.GetInstance<MonitoringInterceptor>(),
            container.GetInstance<SqlUserRepository>()
        )
    );

    // Nested decorators: SqlUserRepository gets decorated by two interceptors.
    container.RegisterWithInterceptor<IUserRepository, YetAnotherInterestingInterceptor,
    MonitoringInterceptor, SqlUserRepository>();

    // Manually: SqlUserRepository gets decorated by two interceptors.
    container.Register<IUserRepository>(() =>
        Interceptor.CreateProxy<IUserRepository>(
            container.GetInstance<YetAnotherInterestingInterceptor>(),
            Interceptor.CreateProxy<IUserRepository>(
                container.GetInstance<MonitoringInterceptor>(),
                container.GetInstance<SqlUserRepository>()
            )
        )
    );
    */

    [TestClass]
    public class InterceptorExtensionsTests
    {
        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            // Act

            // Assert
        }

        // Example interceptor
        private class MonitoringInterceptor : IInterceptor
        {
            private readonly ILogger logger;

            public MonitoringInterceptor(ILogger logger)
            {
                this.logger = logger;
            }

            public void Intercept(IInvocation invocation)
            {
                this.logger.Log("Start");

                // Calls the decorated instance.
                invocation.Proceed();

                this.logger.Log("Done");
            }
        }
    }
}
