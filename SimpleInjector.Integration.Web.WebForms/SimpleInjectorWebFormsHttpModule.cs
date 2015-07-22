#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013 Simple Injector Contributors
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

namespace SimpleInjector.Integration.Web.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    /// Simple Injector ASP.NET Web Forms integration HTTP Module. This module can be registered in an ASP.NET
    /// Web Forms application and allows automatic initialization of 
    /// the assembly of this class is included in the application's bin folder. The module will trigger the
    /// disposing of created instances that are flagged as needing to be disposed at the end of the web 
    /// request.
    /// </summary>
    public class SimpleInjectorWebFormsHttpModule : IHttpModule
    {
        private static Container container;

        private HttpApplication application;

        /// <summary>Sets the <see cref="container"/> instance for this module to use.</summary>
        /// <param name="container">The container instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is a null
        /// reference (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Thrown when this operation is called twice.</exception>
        public static void SetContainer(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (SimpleInjectorWebFormsHttpModule.container != null)
            {
                throw new InvalidOperationException("SetContainer has already been called.");
            }

            SimpleInjectorWebFormsHttpModule.container = container;
        }

        /// <summary>Disposes of the resources (other than memory) used by this module.</summary>
        public virtual void Dispose()
        {
        }

        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        /// <param name="context">An <see cref="HttpApplication"/> that provides access to the methods, 
        /// properties, and events common to all application objects within an ASP.NET application.</param>
        public virtual void Init(HttpApplication context)
        {
            this.application = context;

            context.PreRequestHandlerExecute += this.PreRequestHandlerExecute;
        }

        /// <summary>
        /// Returns the <see cref="container"/> instance that is registered using <see cref="SetContainer"/>.
        /// Inheritors can override this property to allow returning different container instances based on
        /// some condition (request information for instance). This is useful for multi-tenant applications.
        /// </summary>
        /// <returns>The container instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="SetContainer"/> hasn't been
        /// called.</exception>
        /// <value>The method should never return null.</value>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "The execution of this method could potentially be performance heavy.")]
        protected virtual Container GetContainer()
        {
            if (SimpleInjectorWebFormsHttpModule.container == null)
            {
                throw new InvalidOperationException(
                    "Make sure WebFormsInitializerHttpModule.SetContainer is called during application " + 
                    "startup.");
            }
                
            return SimpleInjectorWebFormsHttpModule.container;
        }

        private void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var handler = this.application.Context.CurrentHandler;

            if (handler != null)
            {
                HandlerInitializer.Initialize(handler, this.GetContainer());
            }
        }

        private struct HandlerInitializer
        {
            private readonly IHttpHandler handler;
            private readonly Container container;

            private HandlerInitializer(IHttpHandler handler, Container container)
            {
                this.handler = handler;
                this.container = container;
            }

            internal static void Initialize(IHttpHandler handler, Container container)
            {
                var initializer = new HandlerInitializer(handler, container);

                initializer.InitializeHttpHandler();
            }

            private void InitializeHttpHandler()
            {
                bool handlerIsPage = typeof(Page).IsAssignableFrom(this.handler.GetType());

                if (handlerIsPage)
                {
                    this.InitializePage();
                }
                else
                {                
                    this.InitializeHandler();
                }
            }

            private void InitializePage()
            {
                // ASP.NET creates a sub type for Pages with all the mark up, but this is not the type that a 
                // user can register and not always a type that can be initialized.
                Type pageType = this.handler.GetType().BaseType;

                var page = (Page)this.handler;

                this.InitializeInstance(pageType, page);
                
                this.InitializePageUserControls(page);
            }
            
            private void InitializeHandler()
            {
                Type handlerType = this.handler.GetType();

                this.InitializeInstance(handlerType, this.handler);
            }

            private void InitializePageUserControls(Page page)
            {
                PageInitializer.HookEventsForUserControlInitialization(page, this.container);
            }

            private void InitializeInstance(Type type, object instance)
            {
                var producer = this.container.GetRegistration(type, throwOnFailure: true);

                producer.Registration.InitializeInstance(instance);
            }
        }

        private sealed class PageInitializer
        {
            private readonly HashSet<Control> alreadyInitializedControls = new HashSet<Control>();
            private readonly Page page;
            private readonly Container container;

            private PageInitializer(Page page, Container container)
            {
                this.page = page;
                this.container = container;
            }

            internal static void HookEventsForUserControlInitialization(Page page, Container container)
            {
                var initializer = new PageInitializer(page, container);

                page.PreInit += initializer.PreInit;
                page.PreLoad += initializer.PreLoad;
            }

            private void PreInit(object sender, EventArgs e)
            {
                this.RecursivelyInitializeMasterPages();
            }

            private void RecursivelyInitializeMasterPages()
            {
                foreach (var masterPage in this.GetMasterPages())
                {
                    this.InitializeUserControl(masterPage);
                }
            }

            private IEnumerable<MasterPage> GetMasterPages()
            {
                MasterPage master = this.page.Master;

                while (master != null)
                {
                    yield return master;

                    master = master.Master;
                }
            }

            private void PreLoad(object sender, EventArgs e)
            {
                this.InitializeControlHierarchy(this.page);
            }

            private void InitializeControlHierarchy(Control control)
            {
                var dataBoundControl = control as DataBoundControl;

                if (dataBoundControl != null)
                {
                    dataBoundControl.DataBound += this.InitializeDataBoundControl;
                }
                else
                {
                    var userControl = control as UserControl;

                    if (userControl != null)
                    {
                        this.InitializeUserControl(userControl);
                    }

                    foreach (var childControl in control.Controls.Cast<Control>())
                    {
                        this.InitializeControlHierarchy(childControl);
                    }
                }
            }

            private void InitializeDataBoundControl(object sender, EventArgs e)
            {
                var control = (DataBoundControl)sender;

                if (control != null)
                {
                    control.DataBound -= this.InitializeDataBoundControl;

                    this.InitializeControlHierarchy(control);
                }
            }

            private void InitializeUserControl(UserControl instance)
            {
                if (!this.alreadyInitializedControls.Contains(instance))
                {
                    // ASP.NET creates a sub type for UserControls with all the mark up, but this is not the  
                    // type that a user can register and not always a type that can be initialized.
                    Type type = instance.GetType().BaseType;

                    this.InitializeInstance(type, instance);

                    // Ensure every user control is only initialized once.
                    this.alreadyInitializedControls.Add(instance);
                }
            }

            private void InitializeInstance(Type type, object instance)
            {
                var producer = this.container.GetRegistration(type, throwOnFailure: true);

                producer.Registration.InitializeInstance(instance);
            }
        }
    }
}