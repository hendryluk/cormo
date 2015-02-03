using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;
using Newtonsoft.Json.Serialization;

namespace Cormo.Web.Impl
{
    [Configuration]
    public class WebApiRegistrar
    {
        [Inject] private IDependencyResolver _resolver;
        [Inject, GlobalFilter] private IInstance<IFilter> _filters;
        [Inject, GlobalFilter] private IInstance<DelegatingHandler> _delegatingHandlers;
        [Inject] private IContractResolver _contractResolver;

        [Inject]
        public virtual void Setup(IComponentManager manager, HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = _contractResolver;
            config.MapHttpAttributeRoutes();
            config.DependencyResolver = _resolver;
            foreach (var handler in _delegatingHandlers)
                config.MessageHandlers.Add(handler);
            foreach (var filter in _filters)
                config.Filters.Add(filter);
            config.ParameterBindingRules.Add(param => new InjectParameterBinding(manager, param));
            config.EnsureInitialized();
        }

        [Singleton]
        public class Defaults
        {
            [Produces, Singleton, ConditionalOnMissingComponent]
            private readonly HttpConfiguration _configuration = GlobalConfiguration.Configuration;

            [Produces, Singleton, ConditionalOnMissingComponent]
            private readonly IContractResolver _contractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }

    public class CormoControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            var cormoResolver = request.GetDependencyScope() as ICormoDependencyResolver;
            if(cormoResolver == null)
                return null;

            // No proxy
            return (IHttpController) cormoResolver.GetReference(controllerType);
        }
    }
}