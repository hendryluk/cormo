using System.Collections.Generic;
using Alpaca.Contexts;
using Alpaca.Injects;

namespace Alpaca.Contexts
{
    public interface IContextual
    {
        object Create(ICreationalContext context);
        void Destroy();
    }

    public interface ICreationalContext
    {
        void Push(object incompleteInstance);
        void Release();
    }
}

namespace Alpaca.Weld.Contexts
{
    public interface IContextualInstance
    {
        object Instance { get; }
        ICreationalContext CreationalContext { get; }
        IContextual Contextual { get; }
    }

    public interface IWeldCreationalContext : ICreationalContext
    {
        
    }

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
            throw new System.NotImplementedException();
        }
    }
}