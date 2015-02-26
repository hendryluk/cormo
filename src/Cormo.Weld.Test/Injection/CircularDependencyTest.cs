using Cormo.Impl.Weld;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using NUnit.Framework;

namespace Cormo.Weld.Test.Injection
{
    public class CircularDependencyTest
    {
        private WeldComponentManager _manager;
        private AttributeScanDeployer _deployer;

        public class One
        {
            [Inject] public Two two;
        }

        public class Two
        {
            [Inject] public One one;
        }

        public class WithCtorOne
        {
            [Inject]
            public WithCtorOne(WithCtorTwo one)
            {
            }
        }

        public class WithCtorTwo
        {
            [Inject]
            public WithCtorTwo(WithCtorOne one)
            {
            }
        }

        [SetUp]
        public void Setup()
        {
            _manager = new WeldComponentManager("test");
            _deployer = new AttributeScanDeployer(_manager, new WeldEnvironment());
        }

        [Test]
        public void FieldCircularDependencyShouldBeAllowed()
        {
            _deployer.AddTypes(typeof(One), typeof(Two));
            _deployer.Deploy();

            var component = _manager.GetComponent(typeof (One));
            var instance = (One)_manager.GetReference(component, _manager.CreateCreationalContext(component));

            Assert.AreEqual(instance, instance.two.one);    
        }

        [Test]
        [ExpectedException(typeof(CircularDependenciesException))]
        public void ConstructorCircularDependencyOnDependentScopesShouldBeDetectedAsError()
        {
            _deployer.AddTypes(typeof(WithCtorOne), typeof(WithCtorTwo));
            _deployer.Deploy();
        }
    }
}