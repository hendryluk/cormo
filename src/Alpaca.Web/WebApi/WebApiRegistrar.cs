using System.Web.Http;
using Alpaca.Injects;
using Alpaca.Injects;
using Owin;

namespace Alpaca.Web.WebApi
{
    [Configuration]
    public class WebApiRegistrar
    {
        [Inject]
        public virtual void Setup(IAppBuilder appBuilder, HttpConfiguration httpConfiguration)
        {
            appBuilder.UseWebApi(httpConfiguration);
        }

        public class Defaults
        {
            [Inject] private IComponentManager _manager;

            [Produces]
            [ConditionalOnMissingBean]
            public virtual HttpConfiguration GetHttpConfiguration()
            {
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                config.DependencyResolver = new AlpacaDependencyResolver(_manager);
                return config;
            }
        }
    }
}