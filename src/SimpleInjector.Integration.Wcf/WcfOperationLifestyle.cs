// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Integration.Wcf
{
    using System;
    using System.ServiceModel;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Defines a lifestyle that caches instances for the lifetime of a WCF service class. WCF allows service
    /// classes to be (both implicitly and explicitly) configured to have a lifetime of <b>PerCall</b>,
    /// <b>PerSession</b> or <b>Single</b> using the <see cref="InstanceContextMode"/> enumeration. The
    /// lifetime of WCF service classes is controlled by WCF and this lifestyle allows registrations to be
    /// scoped according to the containing WCF service class.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>WcfOperationLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new WcfOperationLifestyle();
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
    /// ]]></code>
    /// </example>
    [Obsolete("Please use SimpleInjector.Lifestyles.AsyncScopedLifestyle instead. " +
        "Will be removed in version 5.0.",
        error: true)]
    public class WcfOperationLifestyle : AsyncScopedLifestyle
    {
        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class. The instance
        /// will ensure that created and cached instance will be disposed after the execution of the web
        /// request ended and when the created object implements <see cref="IDisposable"/>.</summary>
        public WcfOperationLifestyle()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WcfOperationLifestyle"/> class.</summary>
        /// <param name="disposeInstanceWhenOperationEnds">
        /// Specifies whether the created and cached instance will be disposed after the execution of the WCF
        /// operation ended and when the created object implements <see cref="IDisposable"/>.
        /// </param>
        [Obsolete("Please use WcfOperationLifestyle() instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public WcfOperationLifestyle(bool disposeInstanceWhenOperationEnds) : this()
        {
            throw new NotSupportedException(
                "This constructor has been deprecated. Please use WcfOperationLifestyle() instead.");
        }

        /// <summary>
        /// Allows registering an <paramref name="action"/> delegate that will be called when the current
        /// WCF operation ends, but before the scope disposes any instances.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        /// <param name="action">The delegate to run when the WCF operation ends.</param>
        /// <exception cref="ArgumentNullException">Thrown when one of the arguments is a null reference
        /// (Nothing in VB).</exception>
        /// <exception cref="InvalidOperationException">Will be thrown when there is currently no active
        /// WCF operation in the supplied <paramref name="container"/> instance.</exception>
        [Obsolete("Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead. " +
            "Will be removed in version 5.0.",
            error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void WhenWcfOperationEnds(Container container, Action action)
        {
            throw new NotSupportedException(
                "WhenWcfOperationEnds has been deprecated. " +
                "Please use Lifestyle.Scoped.WhenScopeEnds(Container, Action) instead.");
        }
    }
}