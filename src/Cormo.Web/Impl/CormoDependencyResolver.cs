using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Castle.DynamicProxy;

namespace Cormo.Web.Impl
{
    public class CormoDependencyResolver: IDependencyResolver
    {
        [Inject] IComponentManager _componentManager;
        
        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            try
            {
                var instance = _componentManager.GetReference(serviceType);
                return instance;
            }
            catch (UnsatisfiedDependencyException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return new []{ _componentManager.GetReference(serviceType)};
            }
            catch (UnsatisfiedDependencyException)
            {
                return Enumerable.Empty<object>();
            }
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}