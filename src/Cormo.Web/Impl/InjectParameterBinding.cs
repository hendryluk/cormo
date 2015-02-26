using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Web.Impl
{
    public class InjectParameterBinding : HttpParameterBinding
    {
        static readonly UnwrapAttribute UnwrapAttributeInstance = new UnwrapAttribute();
        private MethodParameterInjectionPoint _injectionPoint;

        public InjectParameterBinding(IComponentManager manager, HttpParameterDescriptor descriptor)
            : base(descriptor)
        {
            var reflectedDescriptor = descriptor as ReflectedHttpParameterDescriptor;
            if (reflectedDescriptor != null)
            {
                 try
                {
                     var param = reflectedDescriptor.ParameterInfo;
                     var controllerType = param.Member.ReflectedType;
                     
                     var declaringComponent = manager.GetComponent(controllerType);
                     var binders = descriptor.GetCustomAttributes<Attribute>().GetAttributesRecursive<IBinderAttribute>()
                         .Union(new []{UnwrapAttributeInstance})
                         .ToArray();

                     var injectionPoint = new MethodParameterInjectionPoint(declaringComponent, param, new Binders(binders));
                     var component = injectionPoint.Component;
                    _injectionPoint = injectionPoint;
                }
                catch (UnsatisfiedDependencyException)
                {
                    // Not managed by cormo. Nothing here
                }
            }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (_injectionPoint != null)
            {
                var resolver = actionContext.Request.GetDependencyScope() as ICormoDependencyResolver;
                if (resolver != null)
                {
                    var resolved = resolver.GetReference(_injectionPoint);
                    actionContext.ActionArguments[Descriptor.ParameterName] = resolved;
                }
               
            }

            return Task.FromResult(0);
        }
    }
}