// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Advanced
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the container's behavior for compiling expressions into delegates.
    /// </summary>
    public interface IExpressionCompilationBehavior
    {
        /// <summary>
        /// Compiles the supplied <paramref name="expression"/> to a delegate.
        /// </summary>
        /// <param name="expression">The expression to compile.</param>
        /// <returns>A <see cref="Delegate"/>.</returns>
        Delegate Compile(Expression expression);
    }
}