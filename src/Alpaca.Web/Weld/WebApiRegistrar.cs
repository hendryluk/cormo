using System.Web.Http;
using Alpaca.Injects;
using Owin;

namespace Alpaca.Web.Weld
{
    public class WebApiRegistrar
    {
        [Inject] IAppBuilder _appBuilder;
        [Inject] HttpConfiguration _httpConfiguration;

        [PostConstruct]
        public virtual void Init()
        {
            _appBuilder.UseWebApi(_httpConfiguration);
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