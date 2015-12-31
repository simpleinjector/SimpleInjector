namespace SimpleInjector.Integration.Web.Tests.Unit
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Web;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public sealed class HttpContextScope : IDisposable
    {
        public HttpContextScope()
        {
            Assert.IsNull(HttpContext.Current);

            var request = new HttpRequest(null, "//someUri", null);
            var response = new HttpResponse(null);
            var context = new HttpContext(request, response);

            HttpContext.Current = context;

            Assert.IsNotNull(HttpContext.Current);
        }

        public void Dispose()
        {
            Assert.IsNotNull(HttpContext.Current);

            WebApplication.Instance.EndRequest();

            HttpContext.Current = null;
        }

        private sealed class WebApplication : HttpApplication
        {
            public static readonly WebApplication Instance;

            private static readonly object EventEndRequest = typeof(HttpApplication)
                .GetField("EventEndRequest", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            static WebApplication()
            {
                var application = new WebApplication();

                IHttpModule module = new SimpleInjectorHttpModule();

                // This registers the EndRequest event in the HttpApplication
                module.Init(application);

                Instance = application;
            }

            public new void EndRequest()
            {
                this.Events[EventEndRequest].DynamicInvoke(null, null);
            }
        }
    }

    [TestClass]
    public class HttpContextScopeTests
    {
        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            Assert.IsNull(HttpContext.Current, "Test setup failed");

            // Act
            using (new HttpContextScope())
            {
                // Assert
                Assert.IsNotNull(HttpContext.Current, "HttpContext.Current should be set.");
            }

            Assert.IsNull(HttpContext.Current, "HttpContext.Current should be null after disposing.");
        }

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior2()
        {
            // Arrange
            Assert.IsNull(HttpContext.Current, "Test setup failed");

            // Act
            using (new HttpContextScope())
            {
                var otherThread = new Thread(() =>
                {
                    // Assert
                    Assert.IsNull(HttpContext.Current, "HttpContext.Current should still be null in other thread.");
                });

                otherThread.Start();

                otherThread.Join();
            }
        }
    }
}