using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Utils;

namespace Cormo.Web.Impl
{
    public class InjectParameterBinding : HttpParameterBinding
    {
        private readonly IComponentManager _manager;
        private readonly IComponent _component = null;

        public InjectParameterBinding(IComponentManager manager, HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
            _manager = manager;

            try
            {
                var qualifiers = descriptor.GetCustomAttributes<Attribute>().GetAttributesRecursive<IQualifier>().ToArray();
                _component = _manager.GetComponent(descriptor.ParameterType, qualifiers);
            }
            catch (UnsatisfiedDependencyException)
            {
                // Not managed by cormo. Nothing here
            }
           
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (_component != null)
            {
                var resolver = actionContext.Request.GetDependencyScope() as CormoDependencyResolver;
                if (resolver != null)
                {
                    var resolved = resolver.GetReference(_component, Descriptor.ParameterType);
                    actionContext.ActionArguments[Descriptor.ParameterName] = resolved;
                }
               
            }
            return Task.FromResult(0);
        }
    }
}