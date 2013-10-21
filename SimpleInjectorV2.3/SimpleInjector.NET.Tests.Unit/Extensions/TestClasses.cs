namespace SimpleInjector.Tests.Unit.Extensions
{
    public interface IQuery<TResult>
    {
    }

    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
    }

    public interface ICommand
    {
        void Execute();
    }

    public class ConcreteCommand : ICommand
    {
        public void Execute()
        {
        }
    }

    public class ConcreteTypeWithMultiplePublicConstructors
    {
        public ConcreteTypeWithMultiplePublicConstructors(ICommand command)
        {
        }

        public ConcreteTypeWithMultiplePublicConstructors(ICommand command1, ICommand command2)
        {
        }
    }

    public class GenericQuery1<TModel> : IQuery<TModel>
    {
    }

    public class GenericQuery2<TModel> : IQuery<TModel>
    {
    }

    public class QueryHandlerWithNestedType1<TModel> : IQueryHandler<GenericQuery1<TModel>, TModel>
    {
    }

    public class QueryHandlerWithNestedType2<TModel> : IQueryHandler<GenericQuery2<TModel>, TModel>
    {
    }
}