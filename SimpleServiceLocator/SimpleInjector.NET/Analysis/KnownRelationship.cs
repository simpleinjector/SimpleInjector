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

namespace SimpleInjector.Analysis
{
    using System;
    using System.Diagnostics;
    using SimpleInjector.Lifestyles;

    [DebuggerDisplay(
        "ImplementationType = {SimpleInjector.Helpers.ToFriendlyName(ImplementationType),nq}, " +
        "Lifestyle = {Lifestyle.Name,nq}, " +
        "Dependency = \\{ServiceType = {SimpleInjector.Helpers.ToFriendlyName(Dependency.ServiceType),nq}, " +
            "Lifestyle = {Dependency.Lifestyle.Name,nq}\\}")]
    public sealed class KnownRelationship : IEquatable<KnownRelationship>
    {
        public KnownRelationship(Type implementationType, Lifestyle lifestyle, 
            InstanceProducer dependency)
        {
            Requires.IsNotNull(implementationType, "implementationType");
            Requires.IsNotNull(lifestyle, "lifestyle");
            Requires.IsNotNull(dependency, "dependency");

            this.ImplementationType = implementationType;
            this.Lifestyle = lifestyle;
            this.Dependency = dependency;
        }

        public Type ImplementationType { get; private set; }

        public Lifestyle Lifestyle { get; private set; }

        public InstanceProducer Dependency { get; private set; }

        public override int GetHashCode()
        {
            return
                this.ImplementationType.GetHashCode() ^
                this.Lifestyle.GetHashCode() ^
                this.Dependency.GetHashCode();
        }

        public bool Equals(KnownRelationship other)
        {
            if (other == null)
            {
                return false;
            }

            return
                this.ImplementationType == other.ImplementationType &&
                this.Lifestyle == other.Lifestyle &&
                this.Dependency == other.Dependency;
        }
    }
}