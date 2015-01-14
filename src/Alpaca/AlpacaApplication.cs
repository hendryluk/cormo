using Alpaca.Weld;

namespace Alpaca
{
    public class AlpacaApplication
    {
        public WeldEngine Engine { get; private set; }

        public AlpacaApplication(WeldEngine engine)
        {
            Engine = engine;
        }

        public static AlpacaApplication Configure()
        {
            var scanner = new AttributeScannerCatalogFactory();
            var catalog = scanner.AutoScan();
            var engine = new WeldEngine(catalog);
            return new AlpacaApplication(engine);
        }

        public static void Run()
        {
            Run(Configure());
        }

        public static void Run(AlpacaApplication application)
        {
            application.Engine.Run();
        }
    }
}