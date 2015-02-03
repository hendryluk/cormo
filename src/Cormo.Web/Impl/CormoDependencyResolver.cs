using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Web.Impl
{
    public interface ICormoDependencyResolver : IDependencyResolver
    {
        object GetReference(IComponent component, params Type[] proxyTypes);
        object GetReference(Type serviceType, params Type[] proxyType);
        object GetReference(IInjectionPoint injectionPoint);
    }

    public class CormoDependencyResolver : ICormoDependencyResolver
    {
        private readonly IComponentManager _componentManager;
        private readonly ICreationalContext _creationalContext;

        [Inject]
        public CormoDependencyResolver(IComponentManager componentManager):
            this(componentManager, componentManager.CreateCreationalContext(null))
        {
        }

        public CormoDependencyResolver(IComponentManager componentManager, ICreationalContext creationalContext)
        {
            _componentManager = componentManager;
            _creationalContext = creationalContext;
        }

        public void Dispose()
        {
            _creationalContext.Release();
        }

        public object GetService(Type serviceType)
        {
            return GetReference(serviceType, serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _componentManager.GetComponents(serviceType)
                    .Select(x => GetReference(x, serviceType));
        }

        public IDependencyScope BeginScope()
        {
            return new CormoDependencyResolver(_componentManager, _creationalContext.GetCreationalContext(null));
        }

        public object GetReference(Type serviceType, params Type[] proxyTypes)
        {
            try
            {
                var component = _componentManager.GetComponent(serviceType);
                return GetReference(component, proxyTypes);
            }
            catch (UnsatisfiedDependencyException)
            {
                return null;
            }
        }

        public object GetReference(IComponent component, params Type[] proxyTypes)
        {
            return _componentManager.GetReference(component, _creationalContext, proxyTypes);
        }

        public object GetReference(IInjectionPoint injectionPoint)
        {
            return _componentManager.GetInjectableReference(injectionPoint, injectionPoint.Component, _creationalContext);
        }
    }
}