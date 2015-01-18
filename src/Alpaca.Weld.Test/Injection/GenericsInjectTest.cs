using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using NUnit.Framework;

namespace Alpaca.Weld.Test.Injection
{
    public class GenericsInjectTest
    {
        private WeldEnvironment _env;
        private WeldComponentManager _manager;
        private AttributeScanDeployer _deployer;

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

        [SetUp]
        public void Setup()
        {
            _manager = new WeldComponentManager();
            _deployer = new AttributeScanDeployer(_manager, new WeldEnvironment());
        }

        [Test]
        public void TestInjectionOfOpenGenericComponent()
        {
            _deployer.AddType(typeof(IRepository<>));
            var target = _deployer.AddType(typeof(Target));
            
            Assert.IsInstanceOf<Repository<int>>(GetInstance<Target>(target)._repo);
        }

        private T GetInstance<T>(IComponent component)
        {
            _deployer.Deploy();
            return (T)_manager.GetReference(component);
        }

        [Test]
        public void TestInjectionOfClosedGenericComponent()
        {
            _deployer.AddType(typeof(IntRepository));
            var target = _deployer.AddType(typeof(Target));

            Assert.IsInstanceOf<IntRepository>(GetInstance<Target>(target)._repo);
        }

        [Test]
        [ExpectedException(typeof(UnsatisfiedDependencyException))]
        public void TestMismatchWithClosedGenericComponent()
        {
            _deployer.AddType(typeof(StringRepository));
            _deployer.AddType(typeof(Target));

            try
            {
                _deployer.Deploy();
            }
            catch (UnsatisfiedDependencyException e)
            {
                Assert.AreEqual(typeof(IRepository<int>), e.InjectionPoint.ComponentType);
                throw;
            }
        }
    }
}