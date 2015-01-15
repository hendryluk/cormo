using System;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using NUnit.Framework;

namespace Alpaca.Weld.Test
{
    public class InjectTest
    {
        private WeldEnvironment _env;
        private WeldComponentManager _manager;

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
            _env = new WeldEnvironment();
            _manager = new WeldComponentManager();
            
        }

        private ClassComponent CreateComponent(Type type)
        {
            return new ClassComponent(type, new Attribute[0], _manager, new MethodInfo[0],
                new MethodInfo[0]);
        }

        [Test]
        public void TestInjectionOfOpenGenericComponent()
        {
            _env.AddComponent(CreateComponent(typeof(IRepository<>)));
            var target = CreateComponent(typeof (IRepository<>));
            _env.AddComponent(target);
            target.AddInjectionPoints(new FieldInjectionPoint(target, typeof(Target).GetField("_repo"), new Attribute[0]));

            Assert.IsInstanceOf<Repository<int>>(GetInstance<Target>(target)._repo);
        }

        private T GetInstance<T>(IComponent component)
        {
            _manager.Deploy(_env);
            return (T)_manager.GetReference(component);
        }

        [Test]
        public void TestInjectionOfClosedGenericComponent()
        {
            _env.AddComponent(CreateComponent(typeof(IntRepository)));
            var target = CreateComponent(typeof(IRepository<>));
            _env.AddComponent(target);
            target.AddInjectionPoints(new FieldInjectionPoint(target, typeof(Target).GetField("_repo"), new Attribute[0]));

            Assert.IsInstanceOf<IntRepository>(GetInstance<Target>(target)._repo);
        }

        [Test]
        [ExpectedException(typeof(UnsatisfiedDependencyException))]
        public void TestMismatchWithClosedGenericComponent()
        {
            _env.AddComponent(CreateComponent(typeof(StringRepository)));
            var target = CreateComponent(typeof(IRepository<>));
            _env.AddComponent(target);
            target.AddInjectionPoints(new FieldInjectionPoint(target, typeof(Target).GetField("_repo"), new Attribute[0]));
        
            try
            {
                _manager.Deploy(_env);
            }
            catch (UnsatisfiedDependencyException e)
            {
                Assert.AreEqual(typeof(IRepository<int>), e.InjectionPoint.ComponentType);
                throw;
            }
        }
    }
}