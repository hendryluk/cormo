using System.Web.Http;
using System.Web.Http.Dependencies;
using Cormo.Injects;
using Owin;

namespace Cormo.Web.Impl
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
            [Inject] private IDependencyResolver _resolver;

            [Produces]
            [ConditionalOnMissingComponent]
            public virtual HttpConfiguration GetHttpConfiguration()
            {
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                config.DependencyResolver = _resolver;
                //config.Services.Replace(typeof(IHttpControllerSelector), new CormoControllerSelector());
                return config;
            }
        }
    }

    
}