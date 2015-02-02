using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;
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
            [Inject, GlobalFilter] private IInstance<IFilter> _filters;
            [Inject, GlobalFilter] private IInstance<DelegatingHandler> _delegatingHandlers;
                
            [Produces, Singleton, ConditionalOnMissingComponent]
            public virtual HttpConfiguration GetHttpConfiguration()
            {
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                config.DependencyResolver = _resolver;
                foreach (var handler in _delegatingHandlers)
                    config.MessageHandlers.Add(handler);
                foreach(var filter in _filters)
                    config.Filters.Add(filter);

                //config.Services.Replace(typeof(IHttpControllerSelector), new CormoControllerSelector());
                return config;
            }
        }
    }

    
}