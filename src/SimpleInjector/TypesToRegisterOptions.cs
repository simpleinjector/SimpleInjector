// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using SimpleInjector.Advanced;

    /// <summary>
    /// Defines options to control the types returned from the
    /// <see cref="Container.GetTypesToRegister(Type, IEnumerable{Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
    /// method. For a type to be returned, it should match all the conditions described by the class's
    /// properties. In other words, in case the searched assembly contains a generic type, that is both a
    /// decorator and a composite, it will only be returned by <b>GetTypesToRegister</b> in case both
    /// <see cref="IncludeGenericTypeDefinitions"/>, <see cref="IncludeDecorators"/> and
    /// <see cref="IncludeComposites"/> are set to true.
    /// </summary>
    public class TypesToRegisterOptions : ApiObject
    {
        /// <summary>Initializes a new instance of the <see cref="TypesToRegisterOptions"/> class.</summary>
        public TypesToRegisterOptions()
        {
            this.IncludeDecorators = false;
            this.IncludeGenericTypeDefinitions = false;
            this.IncludeComposites = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether decorator types should be included in the result. The default
        /// value of this property is <b>false</b>. A type is considered a decorator if the type's constructor
        /// contains a parameter of the type that exactly matches the <code>serviceType</code> argument,
        /// supplied to the
        /// <see cref="Container.GetTypesToRegister(Type, IEnumerable{Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
        /// method, or when there is a <see cref="Func{T}"/> argument where <code>T</code> matches the
        /// <code>serviceType</code> argument.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeDecorators { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generic type definitions (types that have
        /// TypeInfo.IsGenericTypeDefinition or Type.IsGenericTypeDefinition set to true)
        /// should be included in the result. The default value for this property is <b>false</b>.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeGenericTypeDefinitions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether composite types should be included in the result. The default
        /// value of this property is <b>true</b>. A type is considered a composite if the type's constructor
        /// contains a parameter of <code>IEnumerable&lt;T&gt;</code>, <code>ICollection&lt;T&gt;</code>,
        /// <code>IList&lt;T&gt;</code>, <code>IReadOnlyCollection&lt;T&gt;</code>,
        /// <code>IReadOnlyList&lt;T&gt;</code> or <code>T[]</code> (array of T), where <code>T</code>
        /// exactly matches the <code>serviceType</code> argument, supplied to the
        /// <see cref="Container.GetTypesToRegister(Type, IEnumerable{Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
        /// method.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeComposites { get; set; }
    }
}