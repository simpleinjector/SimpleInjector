namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector;
    using SimpleInjector.Tests.Unit;

    using ArgEx = System.ArgumentException;
    using ArgNull = System.ArgumentNullException;

    public abstract class TypesExtensionsTests
    {
        public interface IX<T> { }
        public class IntX : IX<int> { }
        public class IntFloatX : IX<int>, IX<float> { }
        public class GenX<T> : IX<T> { }
        public class NoX { }
    }

    [TestClass]
    public class TypeExtensions_IsClosedType_Tests : TypesExtensionsTests
    {
        [TestMethod] public void ReturnsTrue_1() => IsClosedTypeOf(typeof(IX<int>), typeof(IX<>));
        [TestMethod] public void ReturnsTrue_2() => IsClosedTypeOf(typeof(IntX), typeof(IX<>));
        [TestMethod] public void ReturnsTrue_3() => IsClosedTypeOf(typeof(GenX<int>), typeof(IX<>));
        [TestMethod] public void ReturnsTrue_4() => IsClosedTypeOf(typeof(GenX<>), typeof(IX<>));
        [TestMethod] public void ReturnsTrue_5() => IsClosedTypeOf(typeof(IntFloatX), typeof(IX<>));

        [TestMethod] public void ReturnsFalse_1() => IsNotClosedTypeOf(typeof(NoX), typeof(IX<>));
        [TestMethod] public void ReturnsFalse_2() => IsNotClosedTypeOf(typeof(IX<int>), typeof(GenX<>));

        [TestMethod] public void Throws_1() => IsClosedTypeOf_Throws<ArgNull>(null, typeof(IFoo<>));
        [TestMethod] public void Throws_2() => IsClosedTypeOf_Throws<ArgNull>(typeof(NullLogger), null);
        [TestMethod] public void Throws_3() => IsClosedTypeOf_Throws<ArgEx>(typeof(IX<int>), typeof(IX<int>));
        [TestMethod] public void Throws_4() => IsClosedTypeOf_Throws<ArgEx>(typeof(NullLogger), typeof(ILogger));

        private static void IsClosedTypeOf(Type type, Type genericTypeDefinition) =>
            IsClosedTypeOf(type, genericTypeDefinition, expected: true);

        private static void IsNotClosedTypeOf(Type type, Type genericTypeDefinition) =>
            IsClosedTypeOf(type, genericTypeDefinition, expected: false);

        private static void IsClosedTypeOf(Type type, Type genericTypeDefinition, bool expected)
        {
            bool actual = type.IsClosedTypeOf(genericTypeDefinition);

            Assert.AreEqual(expected, actual, 
                message: 
                    "type: " + type.ToFriendlyName() + ", " +
                    "genericTypeDefinition: " + genericTypeDefinition.ToFriendlyName());
        }

        private static void IsClosedTypeOf_Throws<T>(Type type, Type genericType) where T : Exception =>
            AssertThat.Throws<T>(() => type.IsClosedTypeOf(genericType));
    }

    [TestClass]
    public class TypeExtensions_GetClosedTypesOf_Tests : TypesExtensionsTests
    {
        [TestMethod] public void Returns_1() => _(typeof(IX<int>), typeof(IX<>), expected: new[] { typeof(IX<int>) });
        [TestMethod] public void Returns_2() => _(typeof(IntX), typeof(IX<>), expected: new[] { typeof(IX<int>) });
        [TestMethod] public void Returns_3() => _(typeof(GenX<int>), typeof(IX<>), expected: new[] { typeof(IX<int>) });
        [TestMethod] public void Returns_4() => _(typeof(IntFloatX), typeof(IX<>), expected: new[] { typeof(IX<float>), typeof(IX<int>) });
        [TestMethod] public void Returns_5() => _(typeof(NoX), typeof(IX<>), expected: Type.EmptyTypes);
        [TestMethod] public void Returns_6() => _(typeof(IX<int>), typeof(GenX<>), expected: Type.EmptyTypes);

        [TestMethod] public void Throws_1() => _<ArgNull>(null, typeof(IFoo<>));
        [TestMethod] public void Throws_2() => _<ArgNull>(typeof(NullLogger), null);
        [TestMethod] public void Throws_3() => _<ArgEx>(typeof(IX<int>), typeof(IX<int>));
        [TestMethod] public void Throws_4() => _<ArgEx>(typeof(NullLogger), typeof(ILogger));

        private static void _(Type type, Type genericTypeDefinition, params Type[] expected)
        {
            Type[] actual = type.GetClosedTypesOf(genericTypeDefinition);

            AssertThat.SequenceEquals(
                expectedTypes: expected.OrderBy(HashCode),
                actualTypes: actual.OrderBy(HashCode));
        }

        private static void _<T>(Type type, Type genericType) where T : Exception =>
            AssertThat.Throws<T>(() => type.GetClosedTypesOf(genericType));

        private static int HashCode(Type type) => type.GetHashCode();
    }

    [TestClass]
    public class TypeExtensions_GetClosedTypeOf_Tests : TypesExtensionsTests
    {
        [TestMethod] public void Returns_1() => _(typeof(IX<int>), typeof(IX<>), expected: typeof(IX<int>));
        [TestMethod] public void Returns_2() => _(typeof(IntX), typeof(IX<>), expected: typeof(IX<int>));
        [TestMethod] public void Returns_3() => _(typeof(GenX<int>), typeof(IX<>), expected: typeof(IX<int>));

        [TestMethod] public void Throws_1() => _<InvalidOperationException>(typeof(IntFloatX), typeof(IX<>));
        [TestMethod] public void Throws_2() => _<ArgEx>(typeof(NoX), typeof(IX<>));
        [TestMethod] public void Throws_3() => _<ArgEx>(typeof(IX<int>), typeof(GenX<>));

        [TestMethod]
        public void GetClosedTypeOf_AmbiquousRequest_ThrowsExceptionWithExpectedMessage()
        {
            // Act
            Action action = () => typeof(IntFloatX).GetClosedTypeOf(typeof(IX<>));

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                Your request is ambiguous. There are multiple closed version of TypesExtensionsTests.IX<T> 
                that are assignable from TypesExtensionsTests.IntFloatX, namely: 
                TypesExtensionsTests.IX<Int32> and TypesExtensionsTests.IX<Single>. Use GetClosedTypesOf 
                instead to get this list of closed types to select the proper type."
                .TrimInside(),
                action);
        }

        private static void _(Type type, Type genericTypeDefinition, Type expected)
        {
            Type actual = type.GetClosedTypeOf(genericTypeDefinition);

            AssertThat.AreEqual(expected, actual);
        }

        private static void _<T>(Type type, Type genericType) where T : Exception =>
            AssertThat.Throws<T>(() => type.GetClosedTypeOf(genericType));
    }
}