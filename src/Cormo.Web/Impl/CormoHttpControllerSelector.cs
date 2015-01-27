using System.Collections.Generic;
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

        [Inject]
        public CormoHttpControllerSelector(HttpConfiguration httpConfiguration)
            : base(httpConfiguration)
        {
            _configuration = httpConfiguration;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var mapping = base.GetControllerMapping();
            var controllers = _manager.GetComponents(typeof(object), new RestControllerAttribute());

            foreach (var controller in controllers)
            {
                var descriptor = new HttpControllerDescriptor(_configuration, controller.Type.Name, controller.Type);
                mapping.Add(descriptor.ControllerName, descriptor);
            }
            return mapping;
        }
    }
}