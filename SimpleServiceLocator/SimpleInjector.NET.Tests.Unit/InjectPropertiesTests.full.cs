namespace SimpleInjector.Tests.Unit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests for full .NET framework version.</summary>
    public partial class InjectPropertiesTests
    {
        [TestMethod]
        public void InjectProperties_OnUserControlWithMoreThanTwoInjectableDependencies_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterSingle<IPlugin>(new PluginImpl());

            var instance = new MoreThanTwoPropertiesAndWinFormsBindingContext();

            // Act
            container.InjectProperties(instance);
        }

        public class MoreThanTwoPropertiesAndWinFormsBindingContext : ClassWithBindingContext
        {
            public IPlugin Plugin01 { get; set; }

            public IPlugin Plugin02 { get; set; }

            public IPlugin Plugin03 { get; set; }
        }

        public class ClassWithBindingContext
        {
            public System.Windows.Forms.BindingContext BindingContext { get; set; }
        }
    }
}