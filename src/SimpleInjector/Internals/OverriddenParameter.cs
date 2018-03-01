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

namespace SimpleInjector.Internals
{
    using System.Linq.Expressions;
    using System.Reflection;

    // An Overridden parameter prevents the Registration class from calling back into the container to build
    // an expression for the given constructor parameter. Instead the Registration will 
    internal struct OverriddenParameter
    {
        // The parameter to ignore.
        internal readonly ParameterInfo Parameter;

        // The place holder to temporarily inject into the constructor instead so the complete expression can
        // go through the interception pipeline.
        internal readonly ConstantExpression PlaceHolder;
        
        // The final expression that will replace the place holder after the expression went through the
        // interception pipeline.
        internal readonly Expression Expression;

        // The producer of the dependency. This is used to build up the relationships collection and is used
        // for diagnostics.
        internal readonly InstanceProducer Producer;

        internal OverriddenParameter(ParameterInfo parameter, Expression expression, InstanceProducer producer)
        {
            this.Parameter = parameter;
            this.Expression = expression;
            this.Producer = producer;

            // A placeholder is a fake expression that we inject into the NewExpression. After the 
            // NewExpression is created, it is ran through the ExpressionBuilding interception. By using
            // placeholders instead of the real overridden expressions we prevent those expressions from
            // being processed twice by the ExpressionBuilding event (since we expect the supplied expressions
            // to already be processed). After the event has ran we replace the placeholders with the real
            // expressions again (using an ExpressionVisitor).
            this.PlaceHolder = Expression.Constant(null, parameter.ParameterType);
        }
    }
}