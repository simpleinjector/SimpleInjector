#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2015 Simple Injector Contributors
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

namespace SimpleInjector.Decorators
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class CompositeHelpers
    {
        internal static bool ComposesServiceType(Type serviceType, ConstructorInfo constructor) =>
            GetNumberOfCompositeServiceTypeDependencies(serviceType, constructor) > 0;
        
        private static int GetNumberOfCompositeServiceTypeDependencies(Type serviceType,
            ConstructorInfo compositeConstructor)
        {
            Type compositeServiceType = GetCompositeBaseType(serviceType, compositeConstructor);

            if (compositeServiceType == null)
            {
                return 0;
            }

            var validServiceTypeParameters =
                from parameter in compositeConstructor.GetParameters()
                where IsCompositeParameter(parameter, compositeServiceType)
                select parameter;

            return validServiceTypeParameters.Count();
        }

        // Returns the base type of the composite that can be used for decoration (because serviceType might
        // be open generic, while the base type might not be).
        private static Type GetCompositeBaseType(Type serviceType, ConstructorInfo compositeConstructor)
        {
            // This list can only contain serviceType and closed and partially closed versions of serviceType.
            var baseTypeCandidates = 
                Types.GetBaseTypeCandidates(serviceType, compositeConstructor.DeclaringType);

            var compositeInterfaces =
                from baseTypeCandidate in baseTypeCandidates
                where ContainsCompositeParameters(compositeConstructor, baseTypeCandidate)
                select baseTypeCandidate;

            return compositeInterfaces.FirstOrDefault();
        }

        private static bool ContainsCompositeParameters(ConstructorInfo compositeConstructor, Type serviceType) =>
            compositeConstructor.GetParameters()
                .Any(parameter => IsCompositeParameter(parameter, serviceType));

        private static bool IsCompositeParameter(ParameterInfo parameter, Type serviceType) =>
            Types.IsGenericCollectionType(parameter.ParameterType) &&
                parameter.ParameterType.GetGenericArguments()[0] == serviceType;
    }
}
