namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CompilationTests
    {
        [TestMethod]
        public void GetInstance_OnAPublicNestedClassOfAnInternalTopClassRegisteredWithDelegate_Succeeds()
        {
            // Arrange
            var instance = new InternalClass.PublicNestedClass();

            var container = ContainerFactory.New();

            container.Register<InternalClass.PublicNestedClass>(() => instance);

            // Act
            container.GetInstance<InternalClass.PublicNestedClass>();
        }
        
        [TestMethod]
        public void GetInstance_ResolvingPrivateTypeRegisteredAsDelegate_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<InternalClass>(() => new InternalClass());

            // Act
            container.GetInstance<InternalClass>();
        }
        
        [TestMethod]
        public void GetInstance_ResolvingPublicClassNestedInPublicClassNestedInPrivateClass_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<PrivateNested.NestedNested.DeeplyNestedClass>(
                () => new PrivateNested.NestedNested.DeeplyNestedClass());

            // Act
            container.GetInstance<PrivateNested.NestedNested.DeeplyNestedClass>();
        }

        [TestMethod]
        public void GetInstance_ObjectGraphBiggerThanMaximumNumberOfNodesPerDelegate_ReducesTheGraphSizeCorrectly()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.MaximumNumberOfNodesPerDelegate = 10;

            // Act
            // This object has a total graph size of about 100, which means it has to be broken up multiple
            // times, testing effectively the whole mechanism to reduce the object graph size.
            var instance = container.GetInstance<T<T<T<T<A, B>, T<B, A>>, T<T<A, A>, T<B, B>>>, T<B, B>>>();

            // Assert
            Assert.IsNotNull(instance);
        }

        public class A
        {
            public A(B b, C c)
            {
                Requires.IsNotNull(b, nameof(b));
                Requires.IsNotNull(c, nameof(c));
            }
        }

        public class B
        {
            public B(C c)
            {
                Requires.IsNotNull(c, nameof(c));
            }
        }

        public class C
        {
        }

        public class T<T1, T2>
        {
            public T(T1 t1, T2 t2, A a, B b, C c)
            {
                Requires.IsNotNull(t1, nameof(t1));
                Requires.IsNotNull(t2, nameof(t2));
                Requires.IsNotNull(a, nameof(a));
                Requires.IsNotNull(b, nameof(b));
                Requires.IsNotNull(c, nameof(c));
            }
        }

        private class PrivateNested
        {
            public class NestedNested
            {
                public class DeeplyNestedClass 
                { 
                }
            }
        }
    }

    internal class InternalClass
    {
        public class PublicNestedClass 
        { 
        }
    }
}