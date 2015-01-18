using System.Web.Http;
using Alpaca.Injects;
using Owin;

namespace Alpaca.Web.Weld
{
    public class WebApiRegistrar
    {
        [Inject]
        public virtual void Setup(IAppBuilder appBuilder, HttpConfiguration httpConfiguration)
        {
            appBuilder.UseWebApi(httpConfiguration);
        }

        public class Defaults
        {
            [Produces]
            [ConditionalOnMissingBean]
            public virtual HttpConfiguration GetHttpConfiguration()
            {
                return new HttpConfiguration();
            }
        }
    }
}