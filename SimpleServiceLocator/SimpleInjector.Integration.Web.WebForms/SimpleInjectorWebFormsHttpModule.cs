#region Copyright (c) 2013 S. van Deursen
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (C) 2013 S. van Deursen
 * 
 * To contact me, please visit my blog at http://www.cuttingedge.it/blogs/steven/ or mail to steven at 
 * cuttingedge.it.
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
                throw new ArgumentNullException("container");
            }

            if (SimpleInjectorWebFormsHttpModule.container != null)
            {
                throw new InvalidOperationException("SetContainer already called.");
            }

            SimpleInjectorWebFormsHttpModule.container = container;
        }

        void IHttpModule.Dispose()
        {
        }

        void IHttpModule.Init(HttpApplication context)
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
        protected virtual Container GetContainer()
        {
            if (SimpleInjectorWebFormsHttpModule.container == null)
            {
                throw new InvalidOperationException(
                    "Make sure WebFormsInitializerHttpModule.SetContainer is called during application startup.");
            }

            return SimpleInjectorWebFormsHttpModule.container;
        }

        private void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var handler = this.application.Context.CurrentHandler;

            if (handler != null)
            {
                this.InitializeHttpHandler(handler);

                var page = handler as Page;

                if (page != null)
                {
                    this.InitializeControls(page);
                }
            }
        }

        private void InitializeHttpHandler(IHttpHandler instance)
        {
            Type type = instance.GetType();

            // ASP.NET creates a sub type for Pages with all the mark up, but this is not the type that a user 
            // can register and not always a type that can be initialized.
            type = typeof(Page).IsAssignableFrom(type) ? type.BaseType : type;

            this.GetRegistration(type).InitializeInstance(instance);
        }

        private Registration GetRegistration(Type type)
        {
            return this.GetContainer().GetRegistration(type, throwOnFailure: true).Registration;
        }

        private void InitializeControls(Page page)
        {
            PageInitializer.HookEventsForControlInitialization(this, page);
        }

        private sealed class PageInitializer
        {
            private readonly HashSet<Control> alreadyInitializedControls = new HashSet<Control>();
            private readonly SimpleInjectorWebFormsHttpModule module;
            private readonly Page page;

            private PageInitializer(SimpleInjectorWebFormsHttpModule module, Page page)
            {
                this.module = module;
                this.page = page;
            }

            internal static void HookEventsForControlInitialization(SimpleInjectorWebFormsHttpModule module,
                Page page)
            {
                var x = new PageInitializer(module, page);

                page.PreInit += x.PreInit;
                page.PreLoad += x.PreLoad;
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
                MasterPage masterPage = this.page.Master;

                while (masterPage != null)
                {
                    yield return masterPage;

                    masterPage = masterPage.Master;
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

                    this.module.GetRegistration(type).InitializeInstance(instance);

                    // Ensure every user control is only initialized once.
                    this.alreadyInitializedControls.Add(instance);
                }
            }
        }
    }
}