using Cormo.Web;
using Cormo.Web.Impl.Contexts;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CormoOwinStartup))]
namespace Cormo.Web
{
    public class CormoOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var cormo = CormoApplication.AutoScan();
            cormo.Deployer.AddValue(app);
            cormo.Manager.AddContext(new HttpRequestContext());
            cormo.Manager.AddContext(new HttpSessionContext());
            cormo.Deploy();
        }
    }
}