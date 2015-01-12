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

        public class Repository<T>
        {
        }

        public class Target
        {
            [Inject] public IRepository<int> _repo;
        }

        [Test]
        public void TestGenericInjection()
        {
            var catalog = new WeldCatalog();
            catalog.RegisterComponent(typeof(Repository<>));
            catalog.RegisterComponent(typeof(Target));
            catalog.RegisterInject(typeof(Target).GetField("_repo"));

            new WeldEngine(catalog).Run();
        }
    }
}