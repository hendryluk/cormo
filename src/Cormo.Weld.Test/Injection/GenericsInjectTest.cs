using Cormo.Impl.Weld;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using NUnit.Framework;

namespace Cormo.Weld.Test.Injection
{
    public class GenericsInjectTest
    {
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
            _manager = new WeldComponentManager("test");
            _deployer = new AttributeScanDeployer(_manager, new WeldEnvironment());
        }

        [Test]
        public void TestInjectionOfOpenGenericComponent()
        {
            _deployer.AddTypes(typeof(Repository<>), typeof(Target));
            Assert.IsInstanceOf<Repository<int>>(GetInstance<Target>()._repo);
        }

        private T GetInstance<T>()
        {
            _deployer.Deploy();
            var component = _manager.GetComponent(typeof (T));
            return (T)_manager.GetReference(null, component, _manager.CreateCreationalContext(component));
        }

        [Test]
        public void TestInjectionOfClosedGenericComponent()
        {
            _deployer.AddTypes(typeof(IntRepository), typeof(Target));
            Assert.IsInstanceOf<IntRepository>(GetInstance<Target>()._repo);
        }

        [Test]
        [ExpectedException(typeof(UnsatisfiedDependencyException))]
        public void TestMismatchWithClosedGenericComponent()
        {
            _deployer.AddTypes(typeof(StringRepository), typeof(Target));

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