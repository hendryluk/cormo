using Alpaca.Injects;
using Alpaca.Weld;

namespace Alpaca
{
    public class AlpacaApplication
    {
        private AttributeScanDeployer _scanner;
        public IComponentManager ComponentManager { get; private set; }
        public WeldEnvironment Environment { get; private set; }
        public WeldComponentManager Manager { get; set; }

        private AlpacaApplication()
        {
            Environment = new WeldEnvironment();
            Manager = new WeldComponentManager();
            _scanner = new AttributeScanDeployer(Manager, Environment);
        }

        public static AlpacaApplication AutoScan()
        {
            var app = new AlpacaApplication();
            app.Scan();
            return app;
        }

        private void Scan()
        {
            _scanner.AutoScan();
        }

        public static void Run()
        {
            AutoScan().Deploy();
        }

        public void Deploy()
        {
            _scanner.Deploy();
        }
    }
}