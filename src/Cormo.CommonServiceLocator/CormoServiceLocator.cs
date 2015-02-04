using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Injects;
using Microsoft.Practices.ServiceLocation;

namespace Cormo.CommonServiceLocator
{
    [Configuration]
    public class ServiceLocatorRegistrar
    {
        [Inject]
        void SetupServiceLocator(IComponentManager manager)
        {
            ServiceLocator.SetLocatorProvider(()=> new CormoServiceLocator(manager));
        }
    }

    public class CormoServiceLocator: IServiceLocator
    {
        private readonly IComponentManager _manager;

        public CormoServiceLocator(IComponentManager manager)
        {
            _manager = manager;
        }

        public object GetService(Type serviceType)
        {
            return GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }

        public object GetInstance(Type serviceType, string key)
        {
            // Key could be more meaningful if cormo implements [Named] from CDI spec
            // But [Named] is currently not useful because it's mostly only used for JSF.
            // Perhaps could become useful once we have ASP.NET MVC/razor support, but not now

            // Also, due to the unpredictablity of what developers / other framworks will use CommonServiceLocator for, we won't using proxy
            var component = _manager.GetComponent(serviceType);
            return _manager.GetReference(component, _manager.CreateCreationalContext(component));
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var creationalContext = _manager.CreateCreationalContext(null);
            return _manager.GetComponents(serviceType)
                .Select(x => _manager.GetReference(x, creationalContext.GetCreationalContext(x)));
        }

        public TService GetInstance<TService>()
        {
            return (TService) GetInstance(typeof (TService));
        }

        public TService GetInstance<TService>(string key)
        {
            return (TService)GetInstance(typeof(TService), key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return (IEnumerable<TService>) GetAllInstances(typeof(TService));
        }
    }
}