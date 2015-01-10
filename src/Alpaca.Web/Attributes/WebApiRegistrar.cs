using System.Web.Http;
using Alpaca.Weld;
using Alpaca.Weld.Attributes;
using Owin;

namespace Alpaca.Web.Attributes
{
    public class WebApiRegistrar
    {
        [Inject] IAppBuilder _appBuilder;

        [Produces]
        [ConditionalOnMissingBean]
        public virtual HttpConfiguration GetHttpConfiguration()
        {
            return new HttpConfiguration();
        }

        [PostConstruct]
        public virtual void Init(HttpConfiguration httpConfiguration)
        {
            _appBuilder.UseWebApi(httpConfiguration);
        }
    }
}