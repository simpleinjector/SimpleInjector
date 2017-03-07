namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;

    public interface IGeneric<T>
    {
    }

    public interface IClassConstraintedGeneric<T> where T : class
    {
    }

    public interface INewConstraintedGeneric<T> where T : struct
    {
    }

    public interface IOpenGenericWithPredicate<T>
    {
    }

    public sealed class ComponentDependingOn<TDependency>
    {
        public readonly TDependency Dependency;

        public ComponentDependingOn(TDependency dependency)
        {
            this.Dependency = dependency;
        }
    }

    public class ClassConstraintedGeneric<T> : IClassConstraintedGeneric<T> where T : class
    {
    }

    public class ClassConstraintedGeneric2<T> : IClassConstraintedGeneric<T> where T : class
    {
    }

    public class NewConstraintedGeneric1<X> : INewConstraintedGeneric<X> where X : struct
    {
    }

    public class NewConstraintedGeneric2<T> : INewConstraintedGeneric<T> where T : struct
    {
    }

    public class IntGenericType : IGeneric<int>
    {
    }

    public class GenericType<T> : IGeneric<T>
    {
    }

    public class GenericTypeWithLoggerDependency<T> : IGeneric<T>
    {
        public GenericTypeWithLoggerDependency(ILogger logger)
        {
        }
    }

    public class IntAndFloatGeneric : IGeneric<int>, IGeneric<float>
    {
    }

    public class DefaultGenericType<T> : IGeneric<T>
    {
    }

    public class GenericClassType<TClass> : IGeneric<TClass> where TClass : class
    {
    }

    public class GenericClassType2<TClass> : IGeneric<TClass> where TClass : class
    {
    }

    public class GenericDisposableClassType<TDisposableClass> : IGeneric<TDisposableClass>
        where TDisposableClass : class, IDisposable
    {
    }

    public class GenericStructType<TStruct> : IGeneric<TStruct> where TStruct : struct
    {
    }

    public sealed class ValidatorWithUnusedTypeArgument<T, TUnused> : IValidate<T>
    {
        public void Validate(T instance)
        {
            // Do nothing.
        }
    }

    public class MonoDictionary<T> : Dictionary<T, T>
    {
    }

    public class SneakyMonoDictionary<T, TUnused> : Dictionary<T, T>
    {
    }

    // Note: This class deliberately implements a second IProducer. This will verify whether the code can
    // handle types with multiple versions of the same interface.
    public class NullableProducer<T> : IProducer<T?>, IProducer<IValidate<T>>, IProducer<double>
        where T : struct
    {
    }

    // The type constraint will prevent the type from being created when the arguments are ordered
    // incorrectly.
    public sealed class ServiceImplWithTypesArgsSwapped<B, A> : IService<A, B>
        where B : struct
        where A : class
    {
    }

    public class Bar
    {
    }

    public class Baz : IBar<Bar>
    {
    }

    public class Foo<T1, T2> : IFoo<T1> where T1 : IBar<T2>
    {
    }

    public class ServiceWhereTInIsTOut<TA, TB> : IService<TA, TB> where TA : TB
    {
    }

    public class Implementation<X, TUnused1, TUnused2, Y> : IInterface<X, X, Y>
    {
    }

    public class OpenGenericWithPredicate1<T> : IOpenGenericWithPredicate<T>
    {
    }

    public class OpenGenericWithPredicate2<T> : IOpenGenericWithPredicate<T>
    {
    }

    public class DisposableOpenGenericWithPredicate<T> : IOpenGenericWithPredicate<T>, IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class OpenGenericWithPredicateWithClassConstraint<T> : IOpenGenericWithPredicate<T>
        where T : class
    {
    }

    public class OpenGenericWithUnresolvableArgument<T, TUnresolved> : IOpenGenericWithPredicate<T>
    {
    }

    public class OpenGenericWithPredicateWithMultipleCtors<T> : IOpenGenericWithPredicate<T>
    {
        public OpenGenericWithPredicateWithMultipleCtors()
        {
        }

        public OpenGenericWithPredicateWithMultipleCtors(IOpenGenericWithPredicate<object> x)
        {
        }
    }
}