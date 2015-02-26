using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class WeldEnvironment
    {
        private readonly List<IWeldComponent> _components = new List<IWeldComponent>();
        private readonly List<IWeldComponent> _configurations = new List<IWeldComponent>();
        private readonly List<EventObserverMethod> _observers = new List<EventObserverMethod>();
        private readonly List<EventObserverMethod> _exceptionHandlers = new List<EventObserverMethod>();
        private readonly List<ExtensionComponent> _extensions = new List<ExtensionComponent>();

        public IEnumerable<IWeldComponent> Components { get { return _components; } }
        public IEnumerable<IWeldComponent> Configurations { get { return _configurations; } }
        public IEnumerable<ExtensionComponent> Extensions { get { return _extensions; } }
        public IEnumerable<EventObserverMethod> Observers { get { return _observers; } }
        public IEnumerable<EventObserverMethod> ExceptionHandlers { get { return _exceptionHandlers; } }
        
        public void AddComponent(IWeldComponent component)
        {
            _components.Add(component);
        }

        public void AddConfiguration(IWeldComponent component)
        {
            _configurations.Add(component);
        }

        public void AddValue(object instance, IBinderAttribute[] binders, WeldComponentManager manager)
        {
            AddComponent(new ValueComponent(instance, manager));
        }

        public void AddObserver(EventObserverMethod observer)
        {
            _observers.Add(observer);
        }

        public void AddExceptionHandlers(EventObserverMethod observer)
        {
            _exceptionHandlers.Add(observer);
        }

        public void AddExtension(ExtensionComponent extension)
        {
            _extensions.Add(extension);
        }
    }
}