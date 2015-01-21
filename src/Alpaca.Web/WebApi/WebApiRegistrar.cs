using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Alpaca.Injects;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
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
            [ConditionalOnMissingComponent]
            public virtual HttpConfiguration GetHttpConfiguration()
            {
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                config.DependencyResolver = new AlpacaDependencyResolver(_manager);
                //config.Services.Replace(typeof(IHttpControllerSelector), new AlpacaControllerSelector());
                return config;
            }
        }
    }

    
}