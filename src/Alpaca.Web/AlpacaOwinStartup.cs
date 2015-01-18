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
            
            var appComponent = new InstanceComponent(app, app.GetType(), 
                    new QualifierAttribute[0], new DependentAttribute(), 
                    alpaca.ComponentManager);

            alpaca.Environment.AddComponent(appComponent);
            //alpaca.Engine.AddContext(new RequestContext());
            alpaca.Deploy();
        }
    }
}