using System;
using System.Collections.Generic;
using Cormo.Contexts;

namespace Cormo.Weld.Contexts
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

        public WeldCreationalContext GetComponentContext(IContextual contextual)
        {
            return new WeldCreationalContext(contextual,
                new Dictionary<IContextual, object>(_incompleteInstanceMap),
                _dependentInstances);
        }

        public void Push(object incompleteInstance)
        {
            _incompleteInstanceMap.Add(_contextual, incompleteInstance);
        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IContextualInstance> DependentInstances { get { return _dependentInstances; } }
        public void AddDependentInstance(IContextualInstance contextualInstance)
        {
            _parentDependentInstances.Add(contextualInstance);
        }
    }
}