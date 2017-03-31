namespace SimpleInjector.Tests.Unit
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CuriouslyRecurringTemplatePatternTests
    {
        private interface IEntity<T> where T : IEntity<T>
        {
        }

        private interface IRepo<T> where T : class, IEntity<T>
        {
        }

        // These methods don't seem to really do anything but were used to 
        // identify and fix a StackOverflowException and so have been left 
        // in to capture the problem if it is erroneously reintroduced
        [TestMethod]
        public void RegisterOpenGeneric_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.Register(typeof(IRepo<>), typeof(RepoA<>));
        }

        [TestMethod]
        public void RegisterCollection_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterCollection(typeof(IRepo<>), new[] { typeof(RepoA<>), typeof(RepoB<>) });
        }

        [TestMethod]
        public void GetInstance_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.Register(typeof(IRepo<>), typeof(RepoA<>));
            var repo = container.GetInstance<IRepo<Entity>>();
        }

        [TestMethod]
        public void GetAllInstances_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterCollection(typeof(IRepo<>), new[] { typeof(RepoA<>), typeof(RepoB<>) });
            var repo = container.GetAllInstances<IRepo<Entity>>();
            AssertThat.Equals(repo.Count(), 2);
        }

        private class Entity : IEntity<Entity>
        {
        }

        private class RepoA<T> : IRepo<T> where T : class, IEntity<T>
        {
        }

        private class RepoB<T> : IRepo<T> where T : class, IEntity<T>
        {
        }
    }
}