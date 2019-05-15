// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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