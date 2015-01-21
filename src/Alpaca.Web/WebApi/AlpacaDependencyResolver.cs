using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Castle.DynamicProxy;

namespace Alpaca.Web.WebApi
{
    public class AlpacaDependencyResolver: IDependencyResolver
    {
        private readonly IComponentManager _componentManager;
        ProxyGenerator gen = new ProxyGenerator();
        DefaultProxyBuilder builder = new DefaultProxyBuilder();
                
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