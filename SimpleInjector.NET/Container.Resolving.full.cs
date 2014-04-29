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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

#if !PUBLISH
    /// <summary>Methods for resolving instances.</summary>
#endif
    public partial class Container
    {
#if NET45
        partial void TryBuildCollectionInstanceProducerForReadOnly(Type serviceType, 
            ref InstanceProducer producer)
        {
            Type serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            if (serviceTypeDefinition == typeof(IReadOnlyList<>) ||
                serviceTypeDefinition == typeof(IReadOnlyCollection<>))
            {
                Type elementType = serviceType.GetGenericArguments()[0];

                var collection = this.GetAllInstances(elementType) as IContainerControlledCollection;

                if (collection != null)
                {
                    var registration = SingletonLifestyle.CreateSingleRegistration(serviceType, collection, this);

                    producer = new InstanceProducer(serviceType, registration);

                    if (!((IEnumerable<object>)collection).Any())
                    {
                        producer.IsContainerAutoRegistered = true;
                    }
                }
            }
        }
#endif
    }
}