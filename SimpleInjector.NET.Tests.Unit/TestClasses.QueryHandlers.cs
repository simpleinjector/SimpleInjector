namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.ObjectModel;

    public interface IQuery<TResult>
    {
    }

    public interface ICacheableQuery<TResult> : IQuery<ReadOnlyCollection<TResult>>
    {
    }

    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
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

    public class CacheableQuery : ICacheableQuery<DayOfWeek>
    {
    }

    public class NonCacheableQuery : IQuery<DayOfWeek[]>
    {
    }

    public class CacheableQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, ReadOnlyCollection<TResult>>
        where TQuery : ICacheableQuery<TResult>
    {
        public CacheableQueryHandlerDecorator(IQueryHandler<TQuery, ReadOnlyCollection<TResult>> handler)
        {
        }
    }

    public class CacheableQueryHandler : IQueryHandler<CacheableQuery, ReadOnlyCollection<DayOfWeek>>
    {
    }

    public class NonCacheableQueryHandler : IQueryHandler<NonCacheableQuery, DayOfWeek[]>
    {
    }
}