namespace SimpleInjector.Advanced
{
    using System.Linq.Expressions;
    using System.Reflection;

    internal struct OverriddenParameter
    {
        internal readonly ParameterInfo Parameter;
        internal readonly Expression Expression;
        internal readonly InstanceProducer Producer;
        internal readonly ConstantExpression PlaceHolder;

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
            this.PlaceHolder = System.Linq.Expressions.Expression.Constant(null, parameter.ParameterType);
        }
    }
}