using System.Collections.Generic;
using Alpaca.Injects;
using Alpaca.Weld.Component;

namespace Alpaca.Weld
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
    }
}