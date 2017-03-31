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

namespace SimpleInjector.Packaging
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Contract for types allow registering a set of services.
    /// </summary>
    /// <example>
    /// The following example shows an implementation of an <see cref="IPackage"/>.
    /// <code lang="cs"><![CDATA[
    /// public class BusinessLayerPackage : IPackage
    /// {
    ///     public void RegisterServices(Container container)
    ///     {
    ///         container.Register<IUserRepository, DatabaseUserRepository>();
    ///         container.Register<ICustomerRepository, DatabaseCustomerRepository>();
    ///     }
    /// }
    /// ]]></code>
    /// The following example shows how to load all defined packages, using the 
    /// <see cref="PackageExtensions.RegisterPackages(Container, IEnumerable{Assembly})">RegisterPackages</see> method. 
    /// <code lang="cs"><![CDATA[
    /// container.RegisterPackages();
    /// ]]></code>
    /// </example>
    public interface IPackage
    {
        /// <summary>Registers the set of services in the specified <paramref name="container"/>.</summary>
        /// <param name="container">The container the set of services is registered into.</param>
        void RegisterServices(Container container);
    }
}