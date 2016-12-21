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

    // Searches an expression for a specific sub expression and replaces that sub expression with a
    // different supplied expression.
    internal sealed class SubExpressionReplacer : ExpressionVisitor
    {
        private readonly ConstantExpression subExpressionToFind;
        private readonly Expression replacementExpression;

        private SubExpressionReplacer(ConstantExpression subExpressionToFind,
            Expression replacementExpression)
        {
            this.subExpressionToFind = subExpressionToFind;
            this.replacementExpression = replacementExpression;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        internal static Expression Replace(Expression expressionToAlter,
            ConstantExpression nodeToFind, Expression replacementNode)
        {
            var visitor = new SubExpressionReplacer(nodeToFind, replacementNode);

            return visitor.Visit(expressionToAlter);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node == this.subExpressionToFind ? this.replacementExpression : base.VisitConstant(node);
        }
    }
}