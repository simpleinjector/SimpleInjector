namespace SimpleInjector.Tests.Unit
{
    using System.Collections.Generic;

    public interface IOpenGenericWithPredicate<T>
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
}