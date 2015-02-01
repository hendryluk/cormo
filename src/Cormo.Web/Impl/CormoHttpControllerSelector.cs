using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Cormo.Injects;
using Cormo.Web.Api;

namespace Cormo.Web.Impl
{
    [ConditionalOnMissingComponent]
    public class CormoHttpControllerSelector : DefaultHttpControllerSelector
    {
        [Inject] IComponentManager _manager;
        private readonly HttpConfiguration _configuration;
        private Dictionary<string, HttpControllerDescriptor> _restDescriptors;

        [Inject]
        public CormoHttpControllerSelector(HttpConfiguration httpConfiguration)
            : base(httpConfiguration)
        {
            _configuration = httpConfiguration;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var name = GetControllerName(request);
            if (name != null)
            {
                HttpControllerDescriptor descriptor;
                if (_restDescriptors.TryGetValue(name.ToLower(), out descriptor))
                    return descriptor;
            }
            return base.SelectController(request);
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var mapping = base.GetControllerMapping();
            var restDescriptors = 
                _manager.GetComponents(typeof(object), new RestControllerAttribute())
                .Select(controller =>
                {
                    var name = controller.Type.Name;
                    if (name.EndsWith(ControllerSuffix))
                        name = name.Substring(0, name.Length - ControllerSuffix.Length);

                    return new HttpControllerDescriptor(_configuration, name, controller.Type);
                })
                .Where(x=> !mapping.ContainsKey(x.ControllerName))
                .ToArray();

            foreach (var rest in restDescriptors)
                mapping.Add(rest.ControllerName, rest);

            _restDescriptors = restDescriptors.ToDictionary(x => x.ControllerName.ToLower(), x=> x);
            return mapping;
        }
    }
}