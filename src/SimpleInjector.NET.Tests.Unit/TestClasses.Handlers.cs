namespace SimpleInjector.Tests.Unit
{
    public interface IRequest<TResponse>
    {
    }

    public interface IHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
    }

    public class RequestGroup :
        IHandler<Query1, int>,
        IHandler<Query2, double>,
        IHandler<Query3, double>
    {
    }

    public class Query1 : IRequest<int>
    {
    }

    public class Query2 : IRequest<double>
    {
    }

    public class Query3 : IRequest<double>
    {
    }

    public class RequestDecorator<TRequest, TResponse> : IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public RequestDecorator(IHandler<TRequest, TResponse> decoratee)
        {
            this.Decoratee = decoratee;
        }

        public IHandler<TRequest, TResponse> Decoratee { get; }
    }
}
