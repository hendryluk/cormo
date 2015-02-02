using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
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
                config.ParameterBindingRules.Add(param => new InjectParameterBinding(param));

                //config.Services.Replace(typeof(IHttpControllerSelector), new CormoControllerSelector());
                return config;
            }
        }


    }

    public class InjectParameterBinding : HttpParameterBinding
    {
        public InjectParameterBinding(HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.ControllerContext.Controller is ICormoHttpController)
            {
                var resolved = actionContext.Request.GetDependencyScope().GetService(Descriptor.ParameterType);
                actionContext.ActionArguments[Descriptor.ParameterName] = resolved;
            }
            return Task.FromResult(0);
        }
    }
    
}