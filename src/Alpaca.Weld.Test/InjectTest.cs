using System;
using Alpaca.Weld.Attributes;
using Alpaca.Weld.Core;
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
            var catalog = new WeldCatalog();
            catalog.RegisterComponent(typeof(Repository<>));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            Assert.IsInstanceOf<Repository<int>>(GetInstance<Target>(catalog, reg)._repo);
        }

        private static T GetInstance<T>(WeldCatalog catalog, ComponentRegistration reg)
        {
            var weldEngine = new WeldEngine(catalog);
            weldEngine.Run();
            return (T)weldEngine.GetInstance(reg);
        }

        [Test]
        public void TestInjectionOfClosedGenericComponent()
        {
            var catalog = new WeldCatalog();
            catalog.RegisterComponent(typeof(IntRepository));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            Assert.IsInstanceOf<IntRepository>(GetInstance<Target>(catalog, reg)._repo);
        }

        [Test]
        [ExpectedException(typeof(UnsatisfiedDependencyException))]
        public void TestMismatchWithClosedGenericComponent()
        {
            var catalog = new WeldCatalog();
            catalog.RegisterComponent(typeof(StringRepository));
            var reg = catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            try
            {
                new WeldEngine(catalog).Run();
            }
            catch (UnsatisfiedDependencyException e)
            {
                Assert.AreEqual(typeof(IRepository<int>), e.InjectRegistration.RequestedType);
                throw;
            }
        }
    }
}