using Alpaca.Weld;

namespace Alpaca
{
    public class AlpacaApplication
    {
        public AttributeScanDeployer Deployer { get; private set; }
        private WeldEnvironment Environment { get; set; }
        private WeldComponentManager Manager { get; set; }

        private AlpacaApplication()
        {
            Environment = new WeldEnvironment();
            Manager = new WeldComponentManager();
            Deployer = new AttributeScanDeployer(Manager, Environment);
        }

        public static AlpacaApplication AutoScan()
        {
            var app = new AlpacaApplication();
            app.Scan();
            return app;
        }

        private void Scan()
        {
            Deployer.AutoScan();
        }

        public static void Run()
        {
            AutoScan().Deploy();
        }

        public void Deploy()
        {
            Deployer.Deploy();
        }
    }
}