using System.Collections.Generic;
using Cormo.Contexts;
using Cormo.Impl.Utils;

namespace Cormo.Impl.Weld.Contexts
{
    public class WeldCreationalContext : IWeldCreationalContext
    {
        private readonly IContextual _contextual;
        private readonly Dictionary<IContextual, object> _incompleteInstanceMap;
        private readonly List<IContextualInstance> _parentDependentInstances;
        private readonly List<IContextualInstance> _dependentInstances = new List<IContextualInstance>();

        public WeldCreationalContext(IContextual contextual)
            : this(contextual, 
                new Dictionary<IContextual, object>(), 
                new List<IContextualInstance>())
        {
        }

        private WeldCreationalContext(IContextual contextual,
            Dictionary<IContextual, object> incompleteInstanceMap,
            List<IContextualInstance> parentDependentInstances)
        {
            _contextual = contextual;
            _incompleteInstanceMap = incompleteInstanceMap;
            _parentDependentInstances = parentDependentInstances;
        }

        public ICreationalContext GetCreationalContext(IContextual contextual)
        {
            return new WeldCreationalContext(contextual,
                new Dictionary<IContextual, object>(_incompleteInstanceMap),
                _dependentInstances);
        }

        public void Push(object incompleteInstance)
        {
            _incompleteInstanceMap.Add(_contextual, incompleteInstance);
        }

        public object GetIncompleteInstance(IContextual contextual)
        {
            return _incompleteInstanceMap.GetOrDefault(contextual);
        }

        public void Release()
        {
            Release(null, null);
        }

        public IEnumerable<IContextualInstance> DependentInstances { get { return _dependentInstances; } }
        public void AddDependentInstance(IContextualInstance contextualInstance)
        {
            _parentDependentInstances.Add(contextualInstance);
        }

        public void Release(IContextual contextual, object instance)
        {
            foreach(var dependentInstance in _dependentInstances)
            {
                if (contextual == null || !Equals(dependentInstance.Contextual, contextual))
                {
                    Destroy(dependentInstance);
                }
            }
        }

        private void Destroy(IContextualInstance instance)
        {
            instance.Contextual.Destroy(instance.Instance, instance.CreationalContext);
        }
    }
}