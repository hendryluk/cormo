using System.Diagnostics;
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
            
        }
    }
}