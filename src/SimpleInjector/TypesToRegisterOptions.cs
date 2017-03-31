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

namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Defines options to control the types returned from the
    /// <see cref="Container.GetTypesToRegister(System.Type, System.Collections.Generic.IEnumerable{System.Reflection.Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
    /// method. For a type to be returned, it should match all the conditions described by the class's
    /// properties. In other words, in case the searched assembly contains a generic type, that is both a
    /// decorator and a composite, it will only be returned by <b>GetTypesToRegister</b> in case both
    /// <see cref="IncludeGenericTypeDefinitions"/>, <see cref="IncludeDecorators"/> and 
    /// <see cref="IncludeComposites"/> are set to true.
    /// </summary>
    public class TypesToRegisterOptions
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
        /// <see cref="Container.GetTypesToRegister(System.Type, System.Collections.Generic.IEnumerable{System.Reflection.Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
        /// method, or when there is a <see cref="System.Func{T}"/> argument where <code>T</code> matches the
        /// <code>serviceType</code> argument.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeDecorators { get; set; }

#if NETSTANDARD1_0 || NETSTANDARD1_3
        /// <summary>
        /// Gets or sets a value indicating whether generic type definitions (types that have
        /// <see cref="TypeInfo.IsGenericTypeDefinition">TypeInfo.IsGenericTypeDefinition</see>
        /// set to true) 
        /// should be included in the result. The default value for this property is <b>false</b>.
        /// </summary>
        /// <value>A boolean.</value>
#else
        /// <summary>
        /// Gets or sets a value indicating whether generic type definitions (types that have
        /// <see cref="System.Type.IsGenericTypeDefinition">Type.IsGenericTypeDefinition</see>
        /// set to true) 
        /// should be included in the result. The default value for this property is <b>false</b>.
        /// </summary>
        /// <value>A boolean.</value>
#endif
        public bool IncludeGenericTypeDefinitions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether composite types should be included in the result. The default
        /// value of this property is <b>true</b>. A type is considered a composite if the type's constructor
        /// contains a parameter of <code>IEnumerable&lt;T&gt;</code>, <code>ICollection&lt;T&gt;</code>,
        /// <code>IList&lt;T&gt;</code>, <code>IReadOnlyCollection&lt;T&gt;</code>, 
        /// <code>IReadOnlyList&lt;T&gt;</code> or <code>T[]</code> (array of T), where <code>T</code>
        /// exactly matches the <code>serviceType</code> argument, supplied to the
        /// <see cref="Container.GetTypesToRegister(System.Type, System.Collections.Generic.IEnumerable{System.Reflection.Assembly}, TypesToRegisterOptions)">GetTypesToRegister</see>
        /// method.
        /// </summary>
        /// <value>A boolean.</value>
        public bool IncludeComposites { get; set; }
    }
}