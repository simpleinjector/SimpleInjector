// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Internals;

#if !PUBLISH
    /// <summary>Methods for verifying the container.</summary>
#endif
    public partial class Container
    {
        // Flag to signal that the container's configuration is currently being verified.
        private readonly ThreadLocal<bool> isVerifying = new();

        private readonly ThreadLocal<Scope?> resolveScope = new();

        private bool usingCurrentThreadResolveScope;

        // Flag to signal that the container's configuration has been verified (at least once).
        internal bool SuccesfullyVerified { get; private set; }

        internal Scope? VerificationScope { get; private set; }

        // Allows to resolve directly from a scope instead of relying on an ambient context.
        internal Scope? CurrentThreadResolveScope
        {
            get
            {
                return this.usingCurrentThreadResolveScope ? this.resolveScope.Value : null;
            }

            set
            {
                // PERF: We flag the use of the current-thread-resolve-scope to optimize getting the right
                // scope. Most application's won't resolve directly from the scope, but from the container.
                if (!this.usingCurrentThreadResolveScope)
                {
                    this.usingCurrentThreadResolveScope = true;
                }

                this.resolveScope.Value = value;
            }
        }

        /// <summary>
        /// Verifies and diagnoses this <b>Container</b> instance. This method will call all registered
        /// delegates, iterate registered collections and throws an exception if there was an error.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        public void Verify()
        {
            this.Verify(VerificationOption.VerifyAndDiagnose);
        }

        /// <summary>
        /// Verifies the <b>Container</b>. This method will call all registered delegates,
        /// iterate registered collections and throws an exception if there was an error.
        /// </summary>
        /// <param name="option">Specifies how the container should verify its configuration.</param>
        /// <exception cref="InvalidOperationException">Thrown when the registration of instances was
        /// invalid.</exception>
        /// <exception cref="DiagnosticVerificationException">Thrown in case there are diagnostic errors and
        /// the <see cref="VerificationOption.VerifyAndDiagnose"/> option is supplied.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="option"/> has an invalid value.</exception>
        public void Verify(VerificationOption option)
        {
            Requires.IsValidEnum(option, nameof(option));

            this.ThrowWhenDisposed();

            bool diagnose = option == VerificationOption.VerifyAndDiagnose;

            this.VerifyInternal(suppressLifestyleMismatchVerification: diagnose);

            if (diagnose)
            {
                this.ThrowOnDiagnosticWarnings();
            }
        }

        // NOTE: IsVerifying is thread-specific. We return null when the container is verifying on a
        // different thread.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal Scope? GetVerificationOrResolveScopeForCurrentThread()
        {
            if (this.VerificationScope != null && this.IsVerifying)
            {
                return this.GetScopeWhileVerifying();
            }
            else
            {
                return this.usingCurrentThreadResolveScope ? this.resolveScope.Value : null;
            }
        }

        private Scope? GetScopeWhileVerifying()
        {
            if (this.usingCurrentThreadResolveScope)
            {
                // resolveScope.Value will be not null when someone called Scope.GetInstance. In that case we
                // prefer using this scope, rather than the verification scope.
                return this.resolveScope.Value ?? this.VerificationScope;
            }
            else
            {
                return this.VerificationScope;
            }
        }

        internal void UseCurrentThreadResolveScope() => this.usingCurrentThreadResolveScope = true;

        private void VerifyInternal(bool suppressLifestyleMismatchVerification)
        {
            // Prevent multiple threads from starting verification at the same time. This could crash, because
            // the first thread could dispose the verification scope, while the other thread is still using it.
            lock (this.isVerifying)
            {
                // Flag verifying before locking the container, because LockContainer otherwise triggers
                // verification again causing a stack overflow.
                this.IsVerifying = true;

                this.LockContainer();

                bool original = this.Options.SuppressLifestyleMismatchVerification;

                var scope = this.Options.DefaultScopedLifestyle?.GetCurrentScope(this);

                // If there is a current active scope, prefer using that scope for verification. This prevents
                // this method from having to manage a verification scope, which means calling Dispose() which
                // is problematic when there are IAsyncDisposable registrations. Downside of using the active
                // scope is that those components could be kept alive much longer (depending on when the
                // active scope is disposed of).
                this.VerificationScope = scope ?? new ContainerVerificationScope(this);

                try
                {
                    // Temporarily suppress lifestyle mismatch verification, because that would cause a single
                    // diagnostic warning to be displayed instead of the complete list of found warnings.
                    if (suppressLifestyleMismatchVerification)
                    {
                        this.Options.SuppressLifestyleMismatchVerification = true;
                    }

                    this.Verifying();
                    this.VerifyThatAllExpressionsCanBeBuilt();
                    this.VerifyThatAllRootObjectsCanBeCreated(this.VerificationScope);
                    this.SuccesfullyVerified = true;
                }
                finally
                {
                    this.Options.SuppressLifestyleMismatchVerification = original;
                    
                    var scopeToDispose = this.VerificationScope;
                    this.VerificationScope = null;

                    try
                    {
                        // Only when Verify created the scope itself, should it dispose it. This will prevent
                        // us from having to call Dispose, which will throw when there are IAsyncDisposable
                        // registrations.
                        if (scopeToDispose is ContainerVerificationScope)
                        {
                            scopeToDispose.Dispose();
                        }
                    }
                    finally
                    {
                        // Must be reset after calling Dispose(), because Scope.Dispose() ignores
                        // IAsyncDisposables during verification.
                        this.IsVerifying = false;
                    }
                }
            }
        }

        private void VerifyThatAllExpressionsCanBeBuilt()
        {
            int maximumNumberOfIterations = 10;

            InstanceProducer[] producersToVerify;

            // The process of building expressions can trigger the creation/registration of new instance
            // producers. Those new producers need to be checked as well. That's why we have a loop here. But
            // since a user could accidentally trigger the creation of new registrations during verify, we
            // must set a sensible limit to the number of iterations, to prevent the process from never
            // stopping.
            do
            {
                maximumNumberOfIterations--;

                producersToVerify = this.GetCurrentRegistrations(includeInvalidContainerRegisteredTypes: true);

                producersToVerify = (
                    from producer in producersToVerify
                    where !producer.IsExpressionCreated
                    select producer)
                    .ToArray();

                VerifyThatAllExpressionsCanBeBuilt(producersToVerify);
            }
            while (maximumNumberOfIterations > 0 && producersToVerify.Any());
        }

        private void VerifyThatAllRootObjectsCanBeCreated(Scope verificationScope)
        {
            var rootProducers = this.GetRootRegistrations(includeInvalidContainerRegisteredTypes: true);

            var producersThatMustBeExplicitlyVerified = this.GetProducersThatNeedExplicitVerification();

            var producersToVerify =
                from producer in rootProducers.Concat(producersThatMustBeExplicitlyVerified).Distinct()
                where !producer.InstanceSuccessfullyCreated || !producer.VerifiersAreSuccessfullyCalled
                select producer;

            this.VerifyInstanceCreation(producersToVerify.ToArray(), verificationScope);
        }

        private IEnumerable<InstanceProducer> GetProducersThatNeedExplicitVerification()
        {
            var currentRegistrations = this.GetCurrentRegistrations(includeInvalidContainerRegisteredTypes: true);

            return
                from registration in currentRegistrations
                where registration.MustBeExplicitlyVerified
                select registration;
        }

        private static void VerifyThatAllExpressionsCanBeBuilt(InstanceProducer[] producersToVerify)
        {
            foreach (var producer in producersToVerify)
            {
                var expression = producer.VerifyExpressionBuilding();

                VerifyInstanceProducersOfContainerControlledCollection(expression);
            }
        }

        private static void VerifyInstanceProducersOfContainerControlledCollection(Expression expression)
        {
            var constant = expression as ConstantExpression;

            if (constant?.Value is IContainerControlledCollection collection)
            {
                collection.VerifyCreatingProducers();
            }
        }

        private void VerifyInstanceCreation(InstanceProducer[] producersToVerify, Scope verificationScope)
        {
            foreach (var producer in producersToVerify)
            {
                if (!producer.InstanceSuccessfullyCreated)
                {
                    var instance = producer.VerifyInstanceCreation();

                    VerifyContainerUncontrolledCollection(instance, producer);
                }

                if (!producer.VerifiersAreSuccessfullyCalled)
                {
                    producer.DoExtraVerfication(verificationScope);
                }
            }
        }

        private static void VerifyContainerUncontrolledCollection(object instance, InstanceProducer producer)
        {
            bool isContainerUncontrolledCollection =
                producer.Registration.IsCollection && instance is not IContainerControlledCollection;

            if (isContainerUncontrolledCollection)
            {
                Type collectionType = producer.ServiceType;
                Type serviceType = collectionType.GetGenericArguments()[0];

                Helpers.VerifyCollection((IEnumerable)instance, serviceType);
            }
        }

        private void ThrowOnDiagnosticWarnings()
        {
            DiagnosticResult[] errors = this.GetDiagnosticWarnings().ToArray();

            if (errors.Length > 0)
            {
                throw new DiagnosticVerificationException(errors);
            }
        }

        private IEnumerable<DiagnosticResult> GetDiagnosticWarnings() =>
            from result in Analyzer.Analyze(this)
            where result.Severity > DiagnosticSeverity.Information
            select result;
    }
}