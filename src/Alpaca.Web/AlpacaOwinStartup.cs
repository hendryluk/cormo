using System.Diagnostics;
using Alpaca.Web;
using Alpaca.Web.Weld;
using Alpaca.Weld;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(AlpacaOwinStartup))]
namespace Alpaca.Web
{
    public class AlpacaOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var alpaca = AlpacaApplication.Configure();
            alpaca.Engine.AddContext(new RequestScopeContext());
        }
    }
}