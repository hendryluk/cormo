using Alpaca.Web;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AlpacaOwinStartup))]
namespace Alpaca.Web
{
    public class AlpacaOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var alpaca = AlpacaApplication.AutoScan();
            alpaca.Deployer.AddValue(app);
            //alpaca.Engine.AddContext(new RequestContext());
            alpaca.Deploy();
        }
    }
}