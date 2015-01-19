using System.Diagnostics;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Web;
using Alpaca.Web.Weld;
using Alpaca.Weld;
using Alpaca.Weld.Context;
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