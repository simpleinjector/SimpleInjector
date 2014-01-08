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
        private interface IEntity<T> where T : IEntity<T>
        {
            Guid Id { get; set; }
        }

        private class Entity : IEntity<Entity>
        {
            public Guid Id { get; set; }
        }

        private class Repo<T> : IRepo<T> where T : class, IEntity<T>
        {
            public Repo()
            {
            }
        }

        private class Repo2<T> : IRepo<T> where T : class, IEntity<T>
        {
            public Repo2()
            {
            }
        }

        private interface IRepo<T>
            where T : class, IEntity<T>
        {
        }

        [TestMethod]
        public void RegisterOpenGeneric_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(Repo<>));
        }

        [TestMethod]
        public void GetInstance_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(Repo<>));
            var repo = container.GetInstance<IRepo<Entity>>();
        }

        [TestMethod]
        public void RegisterAllOpenGeneric_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(Repo<>), typeof(Repo2<>));
        }

        [TestMethod]
        public void GetAllInstances_CuriouslyRecurringTemplatePattern_Succeeds()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(Repo<>), typeof(Repo2<>));
            var repo = container.GetAllInstances<IRepo<Entity>>();
            AssertThat.Equals(repo.Count(), 2);
        }
    }
}
