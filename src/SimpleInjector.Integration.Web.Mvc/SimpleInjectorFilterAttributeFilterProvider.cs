#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2014-2015 Simple Injector Contributors
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

namespace SimpleInjector.Integration.Web.Mvc
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    internal sealed class SimpleInjectorFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        private readonly ConcurrentDictionary<Type, Registration> registrations =
            new ConcurrentDictionary<Type, Registration>();

        private readonly Func<Type, Registration> registrationFactory;

        // Supply false for cacheAttributeInstances, because otherwise attributes become singletons and this
        // has can cause all sorts of concurrency bugs, because attributes will becomes singletons.
        internal SimpleInjectorFilterAttributeFilterProvider(Container container)
            : base(cacheAttributeInstances: false)
        {
            this.Container = container;

            this.registrationFactory =
                concreteType => Lifestyle.Transient.CreateRegistration(concreteType, container);
        }

        internal Container Container { get; }

        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext,
            ActionDescriptor actionDescriptor)
        {
            Filter[] filters = this.GetBaseFilters(controllerContext, actionDescriptor);

            foreach (var filter in filters)
            {
                this.InitializeInstance(filter.Instance);
            }

            return filters;
        }

        private Filter[] GetBaseFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            IEnumerable<Filter> filters = base.GetFilters(controllerContext, actionDescriptor);

            return (filters as Filter[]) ?? filters.ToArray();
        }

        private void InitializeInstance(object instance)
        {
            Registration registration =
                this.registrations.GetOrAdd(instance.GetType(), this.registrationFactory);

            registration.InitializeInstance(instance);
        }
    }
}