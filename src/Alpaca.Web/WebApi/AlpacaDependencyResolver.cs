using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Alpaca.Inject;
using Alpaca.Injects.Exceptions;

namespace Alpaca.Web.WebApi
{
    public class AlpacaDependencyResolver: IDependencyResolver
    {
        private readonly IComponentManager _componentManager;

        public AlpacaDependencyResolver(IComponentManager componentManager)
        {
            _componentManager = componentManager;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _componentManager.GetReference(serviceType);
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