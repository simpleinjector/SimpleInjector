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