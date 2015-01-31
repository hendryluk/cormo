using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class WeldEnvironment
    {
        private readonly List<IWeldComponent> _components = new List<IWeldComponent>();
        private readonly List<IWeldComponent> _configurations = new List<IWeldComponent>();
        public IEnumerable<IWeldComponent> Components { get { return _components; } }
        public IEnumerable<IWeldComponent> Configurations { get { return _configurations; } }
        
        public void AddComponent(IWeldComponent component)
        {
            _components.Add(component);
        }

        public void AddConfiguration(IWeldComponent component)
        {
            _configurations.Add(component);
        }

        public void AddValue(object instance, QualifierAttribute[] qualifiers, WeldComponentManager manager)
        {
            AddComponent(new ValueComponent(instance,
                new QualifierAttribute[0], typeof (DependentAttribute),
                manager));
        }
    }
}