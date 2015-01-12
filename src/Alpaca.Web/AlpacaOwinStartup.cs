using System.Diagnostics;
using Alpaca.Web;
using Alpaca.Weld.Core;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AlpacaOwinStartup))]
namespace Alpaca.Web
{
    public class AlpacaOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var scanner = new Scanner();
            Debug.WriteLine("=============Scanning");
            var catalog = scanner.AutoScan();
            catalog.RegisterComponentInstance(app);
            Debug.WriteLine("============Done scanning. Running!!");
            var engine = new WeldEngine(catalog);
            engine.Run();
            Debug.WriteLine("============DONE!!");
        }
    }
}