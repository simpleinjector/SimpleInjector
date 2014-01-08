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
        interface IEntity<T> where T : IEntity<T>
        {
            Guid Id { get; set; }
        }

        class Entity : IEntity<Entity>
        {
            public Guid Id { get; set; }
        }

        class Repo<T> : IRepo<T> where T : class, IEntity<T>
        {
            public Repo()
            {

            }
        }

        class Repo2<T> : IRepo<T> where T : class, IEntity<T>
        {
            public Repo2()
            {

            }
        }

        interface IRepo<T>
            where T : class, IEntity<T>
        {
        }

        // from this link
        // https://simpleinjector.codeplex.com/workitem/20602

        [TestMethod]
        public void CuriouslyRecurringTemplatePatternTestsCanRegister()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(Repo<>));
        }

        [TestMethod]
        public void CuriouslyRecurringTemplatePatternTestsCanResolve()
        {
            var container = new Container();
            container.RegisterOpenGeneric(typeof(IRepo<>), typeof(Repo<>));
            var repo = container.GetInstance<IRepo<Entity>>();
        }

        [TestMethod]
        public void CuriouslyRecurringTemplatePatternTestsCanRegisterAll()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(Repo<>), typeof(Repo2<>));
        }

        [TestMethod]
        public void CuriouslyRecurringTemplatePatternTestsCanResolveAll()
        {
            var container = new Container();
            container.RegisterAllOpenGeneric(typeof(IRepo<>), typeof(Repo<>), typeof(Repo2<>));
            var repo = container.GetAllInstances<IRepo<Entity>>();
            AssertThat.Equals(repo.Count(), 2);
        }
    }
}
