using System.Web.Http;
using Alpaca.Inject;
using Owin;

namespace Alpaca.Web.Attributes
{
    public class WebApiRegistrar
    {
        [Inject] IAppBuilder _appBuilder;
        //[Inject] HttpConfiguration _httpConfiguration;

        [Produces]
        [ConditionalOnMissingBean]
        public virtual HttpConfiguration GetHttpConfiguration()
        {
            return new HttpConfiguration();
        }

        [PostConstruct]
        public virtual void Init()
        {
            //_appBuilder.UseWebApi(_httpConfiguration);
        }
    }
}