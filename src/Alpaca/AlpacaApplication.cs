using Alpaca.Injects;
using Alpaca.Weld;

namespace Alpaca
{
    public class AlpacaApplication
    {
        public IComponentManager ComponentManager { get; private set; }
        public WeldEnvironment Environment { get; private set; }
        public WeldComponentManager Manager { get; set; }

        private AlpacaApplication(WeldEnvironment environment, WeldComponentManager manager)
        {
            Environment = environment;
            Manager = manager;
        }

        public static AlpacaApplication Configure()
        {
            var scanner = new AttributeScannerCatalogFactory();
            var manager = new WeldComponentManager();
            var environment = scanner.AutoScan(manager);
            //var engine = new WeldDeprecatedEngine(catalog);
            return new AlpacaApplication(environment, manager);
        }

        public static void Run()
        {
            Configure().Deploy();
        }

        public void Deploy()
        {
            Manager.Deploy(Environment);
        }
    }
}