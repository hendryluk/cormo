using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Castle.DynamicProxy;

namespace Alpaca.Web.Impl
{
    public class AlpacaDependencyResolver: IDependencyResolver
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