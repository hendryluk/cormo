using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cormo.Web.Impl
{
    [Configuration]
    public class WebApiRegistrar
    {
        [Inject, WebProvider] private IDependencyResolver _resolver;
        [Inject, WebProvider] private IInstance<IFilter> _filters;
        [Inject, WebProvider] private IInstance<DelegatingHandler> _delegatingHandlers;
        [Inject, WebProvider] private IInstance<JsonConverter> _converters;
        [Inject, WebProvider] private IContractResolver _contractResolver;
        [Inject, WebProvider] private JsonSerializerSettings _serializerSettings;

        [Inject]
        public virtual void Setup(IComponentManager manager, HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings = _serializerSettings;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = _contractResolver;
            foreach(var converter in _converters)
                config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(converter);

            config.MapHttpAttributeRoutes(new DefaultInlineConstraintResolver());
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
            [Produces, Default, WebProvider, Singleton]
            private readonly HttpConfiguration _configuration = GlobalConfiguration.Configuration;

            [Produces, WebProvider, ConditionalOnMissingComponent]
            private readonly JsonSerializerSettings _serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;


            [Produces, Singleton, WebProvider, ConditionalOnMissingComponent]
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