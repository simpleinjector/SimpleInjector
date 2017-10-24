
namespace SimpleInjector.CodeSamples.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.Localization;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using SimpleInjector.Lifestyles;
    using SimpleInjector.Advanced;

    public class Program
    {
        public static int Main(string[] args)
        {
            var mode = args.FirstOrDefault() ?? AskForMode();
            args = args.Skip(1).ToArray();
            switch (mode)
            {
                case "classic-basic": return Classic_Basic(args);
                case "classic-mvc": return Classic_MVC(args);
                case "ioc-basic": return IoC_Basic(args);
                case "ioc-mvc": return IoC_MVC(args);
                case "ioc-mvc-proposed": return IoC_MVC_Proposed(args);
                default:
                    Console.Error.WriteLine($"Invalid mode: {mode}");
                    return 1;
            }
        }

        static string AskForMode()
        {
            Console.WriteLine("Available modes: classic-basic, classic-mvc, ioc-basic, ioc-mvc, ioc-mvc-proposed");
            return Console.ReadLine();
        }

        public static int Classic_Basic(string[] args)
        {
            var host = new WebHostBuilder()
            .UseKestrel()
            .Configure(a => a.Run(c => c.Response.WriteAsync(new HiThere(new HttpContextAccessor() { HttpContext = c }).SayHi())))
            .Build();

            host.Run();
            return 0;
        }
        public static int Classic_MVC(string[] args)
        {
            var host = new WebHostBuilder()
            .ConfigureLogging(factory => factory.AddConsole().AddDebug())
            .ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddScoped<IHiThere, HiThere>();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            })
            .Configure(app =>
            {
                app.UseMvcWithDefaultRoute();
            })
            .UseKestrel()
            .Build();

            host.Run();
            return 0;
        }

        public static int IoC_Basic(string[] args)
        {
            var container = new Container();

            container.Register<IWebHostBuilder, WebHostBuilder>();
            container.RegisterInitializer<IWebHostBuilder>(web => web.UseKestrel());
            container.RegisterInitializer<IWebHostBuilder>(web => web.Configure(a => a.Run(c => c.Response.WriteAsync(new HiThere(new HttpContextAccessor() { HttpContext = c }).SayHi()))));
            container.Register<IWebHost>(() => container.GetInstance<IWebHostBuilder>().Build());

            var host = container.GetInstance<IWebHost>();
            host.Run();
            return 0;
        }

        public static int IoC_MVC(string[] args)
        {
            var container = new Container();

            // application registrations:

            // fails verification:
            container.Register<IHiThere, HiThere>();

            // chicken or egg:
            //container.Register<IHiThere>(()=>new HiThereAspNet(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>()));

            // webhost registrations:

            container.Register<IWebHostBuilder, WebHostBuilder>();
            container.RegisterInitializer<IWebHostBuilder>(web => web.UseKestrel());
            container.RegisterInitializer<IWebHostBuilder>(web => web.ConfigureLogging(factory => factory.AddConsole().AddDebug()));
            container.RegisterInitializer<IWebHostBuilder>(web => web.ConfigureServices(services =>
            {
                services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));
                services.AddMvc();
            }));
            container.RegisterInitializer<IWebHostBuilder>(web => web.Configure(a => a.UseMvcWithDefaultRoute()));
            container.Register<IWebHost>(() => container.GetInstance<IWebHostBuilder>().Build());

            // resolve and run:

            container.Verify();

            var host = container.GetInstance<IWebHost>();
            host.Run();
            return 0;
        }


        public static int IoC_MVC_Proposed(string[] args)
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.RegisterWebHost(
                web => web
                    .ConfigureLogging(factory => factory.AddConsole().AddDebug())
                    .ConfigureServices(services => services.AddLocalization().AddMvc())
                    .UseKestrel()
                , (appc, services) =>
                {
                    appc.Register<IControllerActivator,SimpleInjectorControllerActivator>();
                    appc.Register<HiThereController>();
                    appc.Register<IHiThere, HiThere>();
                    appc.RegisterSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    appc.RegisterSingleton<IActionContextAccessor, ActionContextAccessor>();
                    //appc.Verify();
                    return appc;
                }, app => app.UseMvcWithDefaultRoute());

            container.Verify();

            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var host = container.GetInstance<IWebHost>();
                host.Run();
                return 0;
            }
        }
    }

    public sealed class SimpleInjectorControllerActivator : IControllerActivator
    {
        private readonly Container container;
        public SimpleInjectorControllerActivator(Container c) { container = c; }

        public object Create(ControllerContext c) =>
           container.GetInstance(c.ActionDescriptor.ControllerTypeInfo.AsType());

        public void Release(ControllerContext c, object controller) { }
    }
}
