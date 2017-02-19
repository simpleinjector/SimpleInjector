#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2016 Simple Injector Contributors
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using SimpleInjector.Diagnostics;
    using SimpleInjector.Internals;

#if !PUBLISH
    /// <summary>Methods for verifying the container.</summary>
#endif
    public partial class Container
    {
        // Flag to signal that the container's configuration is currently being verified.
        private readonly ThreadLocal<bool> isVerifying = new ThreadLocal<bool>();

        // Flag to signal that the container's configuration has been verified (at least once).
        internal bool SuccesfullyVerified { get; private set; }

        internal Scope VerificationScope { get; private set; }

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

#if !NET40
        // NOTE: IsVerifying is thread-specific. We return null is the container is verifying on a
        // different thread.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal Scope GetVerificationScopeForCurrentThread() => 
            this.VerificationScope != null && this.IsVerifying
                ? this.VerificationScope
                : null;

        private void VerifyInternal(bool suppressLifestyleMismatchVerification)
        {
            // Prevent multiple threads from starting verification at the same time. This could crash, because
            // the first thread could dispose the verification scope, while the other thread is still using it.
            lock (this.isVerifying)
            {
                this.LockContainer();
                bool original = this.Options.SuppressLifestyleMismatchVerification;
                this.IsVerifying = true;
                this.VerificationScope = new ContainerVerificationScope(this);

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
                    this.VerifyThatAllRootObjectsCanBeCreated();
                    this.SuccesfullyVerified = true;
                }
                finally
                {
                    this.Options.SuppressLifestyleMismatchVerification = original;
                    this.IsVerifying = false;
                    var scopeToDispose = this.VerificationScope;
                    this.VerificationScope = null;
                    scopeToDispose.Dispose();
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

        private void VerifyThatAllRootObjectsCanBeCreated()
        {
            var rootProducers = this.GetRootRegistrations(includeInvalidContainerRegisteredTypes: true);

            var producersThatMustBeExplicitlyVerified = this.GetProducersThatNeedExplicitVerification();

            var producersToVerify =
                from producer in rootProducers.Concat(producersThatMustBeExplicitlyVerified).Distinct()
                where !producer.InstanceSuccessfullyCreated || !producer.VerifiersAreSuccessfullyCalled
                select producer;

            VerifyInstanceCreation(producersToVerify.ToArray());
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
            ConstantExpression constant = expression as ConstantExpression;

            if (constant != null && constant.Value is IContainerControlledCollection)
            {
                ((IContainerControlledCollection)constant.Value).VerifyCreatingProducers();
            }
        }

        private static void VerifyInstanceCreation(InstanceProducer[] producersToVerify)
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
                    producer.DoExtraVerfication();
                }
            }
        }

        private static void VerifyContainerUncontrolledCollection(object instance, InstanceProducer producer)
        {
            bool isContainerUncontrolledCollection =
                producer.Registration.IsCollection && !(instance is IContainerControlledCollection);

            if (isContainerUncontrolledCollection)
            {
                Type collectionType = producer.ServiceType;
                Type serviceType = collectionType.GetGenericArguments()[0];

                Helpers.VerifyCollection((IEnumerable)instance, serviceType);
            }
        }

        private void ThrowOnDiagnosticWarnings()
        {
            var errors = (
                from result in Analyzer.Analyze(this)
                where result.Severity > DiagnosticSeverity.Information
                select result)
                .ToArray();

            if (errors.Any())
            {
                throw new DiagnosticVerificationException(errors);
            }
        }
    }
}