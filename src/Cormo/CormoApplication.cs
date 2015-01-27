using Cormo.Weld;

namespace Cormo
{
    public class CormoApplication
    {
        public AttributeScanDeployer Deployer { get; private set; }
        private WeldEnvironment Environment { get; set; }
        private WeldComponentManager Manager { get; set; }

        private CormoApplication()
        {
            Environment = new WeldEnvironment();
            Manager = new WeldComponentManager("deployment");
            Deployer = new AttributeScanDeployer(Manager, Environment);
        }

        public static CormoApplication AutoScan()
        {
            var app = new CormoApplication();
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