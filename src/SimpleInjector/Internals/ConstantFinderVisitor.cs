#region Copyright Simple Injector Contributors
/* The Simple Injector is an easy-to-use Inversion of Control library for .NET
 * 
 * Copyright (c) 2013-2014 Simple Injector Contributors
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
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed class ConstantFinderVisitor : ExpressionVisitor
    {
        private readonly List<ConstantExpression> constants = new List<ConstantExpression>();

        private ConstantFinderVisitor()
        {
        }

        public static List<ConstantExpression> FindConstants(Expression node)
        {
            var visitor = new ConstantFinderVisitor();
            visitor.Visit(node);
            return visitor.constants;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (!node.Type.IsPrimitive())
            {
                this.constants.Add(node);
            }

            return base.VisitConstant(node);
        }
    }
}