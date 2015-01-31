using Cormo.Impl.Weld;
using Cormo.Injects;
using NUnit.Framework;

namespace Cormo.Weld.Test.Injection
{
    public class ProducerMethodTest
    {
        private WeldComponentManager _manager;
        private AttributeScanDeployer _deployer;

        public interface IRepository<T>
        {
            
        }

        public class RepositoryImpl : IRepository<int>
        {
            public RepositoryImpl(int something)
            {
            }
        }

        public class RepoProducer
        {
            [Produces]
            public RepositoryImpl ProduceRepo()
            {
                return new RepositoryImpl(100);
            }
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
        public void CanInjectFromProducer()
        {
            _deployer.AddTypes(typeof(RepoProducer), typeof(Target));
            _deployer.AddProducerMethods(typeof(RepoProducer).GetMethod("ProduceRepo"));
            Assert.IsInstanceOf<RepositoryImpl>(GetInstance<Target>()._repo);
        }

        private T GetInstance<T>()
        {
            _deployer.Deploy();
            var component = _manager.GetComponent(typeof(T));
            return (T)_manager.GetReference(null, component, _manager.CreateCreationalContext(component));
        }
    }
}