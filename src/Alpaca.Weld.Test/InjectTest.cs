using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using NUnit.Framework;

namespace Alpaca.Weld.Test
{
    public class InjectTest
    {
        public interface IRepository<T>
        {
        }

        public class Repository<T>: IRepository<T>
        {
        }

        public class IntRepository : IRepository<int>
        {
        }

        public class StringRepository : IRepository<string>
        {
        }

        public class Target
        {
            [Inject] public IRepository<int> _repo;
        }

        [Test]
        public void TestInjectionOfOpenGenericComponent()
        {
            var catalog = new WeldEnvironment();
            catalog.RegisterComponent(typeof(Repository<>));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            Assert.IsInstanceOf<Repository<int>>(GetInstance<Target>(catalog, reg)._repo);
        }

        private static T GetInstance<T>(WeldEnvironment environment, AbstractComponent reg)
        {
            var manager = new WeldDeprecatedEngine(environment);
            manager.Run();
            return (T)manager.GetInstance(reg);
        }

        [Test]
        public void TestInjectionOfClosedGenericComponent()
        {
            var catalog = new WeldEnvironment();
            catalog.RegisterComponent(typeof(IntRepository));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            Assert.IsInstanceOf<IntRepository>(GetInstance<Target>(catalog, reg)._repo);
        }

        [Test]
        [ExpectedException(typeof(UnsatisfiedDependencyException))]
        public void TestMismatchWithClosedGenericComponent()
        {
            var catalog = new WeldEnvironment();
            catalog.RegisterComponent(typeof(StringRepository));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            try
            {
                new WeldDeprecatedEngine(catalog).Run();
            }
            catch (UnsatisfiedDependencyException e)
            {
                Assert.AreEqual(typeof(IRepository<int>), e.InjectRegistration.RequestedType);
                throw;
            }
        }
    }
}