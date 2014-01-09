namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Extensions;

    [TestClass]
    public class CuriouslyRecurringTemplatePatternTests
    {
        // These methods don't seem to really do anything but were used to 
        // identify and fix a StackOverflowException and so have been left 
        // in to capture the problem if it is erroneously reintroduced

        [TestMethod]
        public void RegisterOpenGeneric_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(RepoA<>));
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(RepoA<>), typeof(RepoB<>));
        }

        [TestMethod]
        public void GetInstance_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(RepoA<>));
            var repo = container.GetInstance<IRepo<Entity>>();
        }

        [TestMethod]
        public void GetAllInstances_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(RepoA<>), typeof(RepoB<>));
            var repo = container.GetAllInstances<IRepo<Entity>>();
            AssertThat.Equals(repo.Count(), 2);
        }

        private interface IEntity<T> where T : IEntity<T>
        {
        }

        private class Entity : IEntity<Entity>
        {
        }

        private interface IRepo<T> where T : class, IEntity<T>
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
