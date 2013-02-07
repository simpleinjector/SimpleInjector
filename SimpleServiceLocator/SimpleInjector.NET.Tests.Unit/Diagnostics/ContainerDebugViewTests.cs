namespace SimpleInjector.Tests.Unit.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Diagnostics;
    
#if !SILVERLIGHT
    
    public interface ILogger
    {
    }

    public interface ISomeGeneric<T>
    {
    }

    public interface IDoThings<T>
    {
    }

    public class ConcreteShizzle
    {
    }

    public class FakeLogger : ILogger
    {
        public FakeLogger(ConcreteShizzle shizzle, ConcreteThing thing)
        {
        }
    }

    public interface IConcreteThing 
    {
    }

    public class ConcreteThing : IConcreteThing 
    { 
    }
    
    public class SomeGeneric<T> : ISomeGeneric<T>
    {
        public SomeGeneric(ILogger logger, IComparable bla, ConcreteThing thing, IDoThings<T> x)
        {
        }
    }

    public class ThingDoer<T> : IDoThings<T>
    {
        public ThingDoer(ILogger logger, IComparable foo)
        {
        }
    }
#endif

#if DEBUG
    [TestClass]
    public class ContainerDebugViewTests
    {
        [TestMethod]
        public void Ctor_WithValidArgument_Succeeds()
        {
            // Arrange
            var container = new Container();

            // Act
            new ContainerDebugView(container);
        }

        [TestMethod]
        public void Options_Always_ReturnsSameInstanceAsThatOfContainer()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(object.ReferenceEquals(container.Options, debugView.Options));
        }

        [TestMethod]
        public void Ctor_WithUnlockedContainer_LeavesContainerUnlocked()
        {
            var container = new Container();

            // Act
            new ContainerDebugView(container);

            // Assert
            Assert.IsFalse(container.IsLocked);
        }

        [TestMethod]
        public void Ctor_WithLockedContainer_LeavesContainerLocked()
        {
            var container = new Container();

            // This locks the container
            container.Verify();

            Assert.IsTrue(container.IsLocked, "Test setup failed.");

            // Act
            new ContainerDebugView(container);

            // Assert
            Assert.IsTrue(container.IsLocked);
        }

        [TestMethod]
        public void Ctor_WithLockedContainer_ReturnsAnItemWithTheRegistrations()
        {
            var container = new Container();

            // This locks the container
            container.Verify();

            var debugView = new ContainerDebugView(container);

            // Act
            var registrationsItem = debugView.Items.Single(item => item.Name == "Registrations");

            // Assert
            Assert.IsInstanceOfType(registrationsItem.Value, typeof(InstanceProducer[]));
        }

        [TestMethod]
        public void Ctor_UnverifiedContainer_ReturnsOneItemWithInfoAboutHowToGetAnalysisInformation()
        {
            // Arrange
            var container = new Container();

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.AreEqual(1, debugView.Items.Length);
            Assert.AreEqual("How To View Diagnostic Info", debugView.Items.Single().Name);
            Assert.AreEqual(
                "Analysis info is available in this debug view after Verify() is " +
                "called on this container instance.", 
                debugView.Items.Single().Description);
        }

        [TestMethod]
        public void Ctor_UnsuccesfullyVerifiedContainer_ReturnsOneItemWithInfoAboutHowToGetAnalysisInformation()
        {
            // Arrange
            var container = new Container();

            // Invalid registration
            container.Register<ILogger>(() => null);

            try
            {
                container.Verify();
            }
            catch (InvalidOperationException) 
            {
            }

            // Act
            var debugView = new ContainerDebugView(container);

            // Assert
            Assert.AreEqual(1, debugView.Items.Length);
            Assert.AreEqual("How To View Diagnostic Info", debugView.Items.Single().Name);
        }

        [TestMethod]
        public void Ctor_VerifiedContainerWithoutConfigurationErrors_ContainsAPotentialLifestyleMismatchesSection()
        {
            // Arrange
            var container = new Container();

            // Forces a lifestyle mismatch
            container.RegisterSingle<ILogger, FakeLogger>();

            container.Verify();

            // Act
            var debugView = new ContainerDebugView(container);

            var warningsItem = debugView.Items.Single(item => item.Name == "Configuration Warnings");

            var items = warningsItem.Value as DebuggerViewItem[];

            // Assert
            Assert.IsTrue(items.Any(item => item.Name == "Potential Lifestyle Mismatches"));
        }

#if !SILVERLIGHT

        [TestMethod]
        public void MethodUnderTest_Scenario_Behavior()
        {
            // Arrange
            var container = new Container();

            container.Register<IConcreteThing, ConcreteThing>(Lifestyle.Singleton);

            container.Register<IComparable>(() => 4);

            container.Register<ILogger, FakeLogger>(Lifestyle.Transient);

            container.Register<ISomeGeneric<IEnumerable<int>>, SomeGeneric<IEnumerable<int>>>(Lifestyle.Transient);
            container.Register<ISomeGeneric<IEnumerable<double>>, SomeGeneric<IEnumerable<double>>>(Lifestyle.Transient);
            container.Register<ISomeGeneric<IEnumerable<long>>, SomeGeneric<IEnumerable<long>>>(Lifestyle.Transient);

            container.Register<IDoThings<IEnumerable<int>>, ThingDoer<IEnumerable<int>>>(Lifestyle.Transient);
            container.Register<IDoThings<IEnumerable<double>>, ThingDoer<IEnumerable<double>>>(Lifestyle.Transient);
            container.Register<IDoThings<IEnumerable<long>>, ThingDoer<IEnumerable<long>>>(Lifestyle.Transient);

            var allTypes =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetExportedTypes()
                where !type.IsGenericTypeDefinition
                select type;

            this.RegisterAll(container, allTypes.Take(1000));
                        
            var watch = Stopwatch.StartNew();

            var ms1 = watch.ElapsedMilliseconds;
            container.Verify();

            // new ContainerDebugView(container);
            var ms2 = watch.ElapsedMilliseconds;

            container.GetInstance<ISomeGeneric<int>>();

            Console.WriteLine();

            Assert.Fail("This is a test.");
        }

        [DebuggerStepThrough]
        public void RegisterAll(Container container, IEnumerable<Type> types)
        {
            var register = this.GetType().GetMethod("Register");

            foreach (var type in types)
            {
                try
                {
                    register.MakeGenericMethod(type).Invoke(null, new object[] { container });
                }
                catch 
                { 
                }
            }
        }

        [DebuggerStepThrough]
        public static void Register<T>(Container container)
        {
            container.Register<ISomeGeneric<T>, SomeGeneric<T>>(Lifestyle.Transient);

            container.Register<IDoThings<T>, ThingDoer<T>>(Lifestyle.Transient);
        }       

#endif

        public class UserRepositoryDecorator : IUserRepository
        {
            public UserRepositoryDecorator(IUserRepository repository)
            {
            }

            public void Delete(int userId)
            {
            }
        }
    }
#endif
}