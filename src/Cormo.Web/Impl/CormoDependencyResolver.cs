using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Injects.Exceptions;

namespace Cormo.Web.Impl
{
    public class CormoDependencyResolver: IDependencyResolver
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
            try
            {
                var component = _componentManager.GetComponent(serviceType);
                return GetReference(component, serviceType);
            }
            catch (UnsatisfiedDependencyException)
            {
                return null;
            }
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

        public object GetReference(IComponent component, Type serviceType)
        {
            return _componentManager.GetReference(component, _creationalContext, serviceType);
        }
    }
}