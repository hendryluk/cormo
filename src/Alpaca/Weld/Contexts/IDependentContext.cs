using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Components;
using Alpaca.Weld.Contexts;
using Alpaca.Weld.Serialization;

namespace Alpaca.Weld
{
    public interface IService
    {
        /// <summary>
        /// Called by Weld when it is shutting down, allowing the service to
        /// perform any cleanup needed.
        /// </summary>
        void Cleanup();
    }
}

namespace Alpaca.Weld.Serialization
{
    public class ContextualStore : IContextualStore
    {
        private Dictionary<IContextual, Guid> _contextuals;
        private Dictionary<Guid, IContextual> _contextualsInverse;
        private Dictionary<IContextual, Guid> _passivationCapableContextuals;
 
        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public string PutIfAbsent(IContextual contextual)
        {
            throw new NotImplementedException();
        }

        public IContextual GetContextual(string id)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISerializableContextual : IContextual
    {
    }

    public interface ISerializableContextualInstance : IContextualInstance
    {
        new ISerializableContextual Contextual { get; }
    }

    public interface IContextualStore: IService
    {
        string PutIfAbsent(IContextual contextual);
        IContextual GetContextual(string id);
    }
}
namespace Alpaca.Weld.Contexts
{
    [Serializable]
    public class SerializableContextualInstance : ISerializableContextualInstance
    {
        public SerializableContextualInstance(IContextual contextual, object instance, ICreationalContext creationalContext, IContextualStore contextualStore)
        {
            Contextual = new SerializableContextual(contextual, contextualStore);
            Instance = instance;
            CreationalContext = creationalContext;
        }

        public object Instance { get; private set; }
        public ICreationalContext CreationalContext { get; private set; }
        public ISerializableContextual Contextual { get; private set; }

        IContextual IContextualInstance.Contextual
        {
            get { return Contextual; }
        }
    }

    public abstract class ForwardingContextual : IContextual
    {
        protected abstract IContextual Delegate { get;  }

        public object Create(ICreationalContext context)
        {
            return Delegate.Create(context);
        }

        public void Destroy()
        {
            Delegate.Destroy();
        }

        public override bool Equals(object obj)
        {
            return this == obj || Delegate.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Delegate.GetHashCode();
        }

        public override string ToString()
        {
            return Delegate.ToString();
        }
    }

    public class Container
    {
        public static readonly Container Instance = new Container();
        private IComponentManager _componentManager;

        public void Initialize(IComponentManager componentManager)
        {
            _componentManager = componentManager;
        }

        public T GetReference<T>()
        {
            return _componentManager.GetReference<T>();
        }
    }

    [Serializable]
    public class SerializableContextual : ForwardingContextual, ISerializableContextual
    {
        private readonly IContextual _serializable;
        
        [NonSerialized]
        private IContextualStore _cachedContextualStore;

        [NonSerialized]
        private IContextual _cached;

        private readonly string _id;

        public SerializableContextual(IContextual contextual, IContextualStore contextualStore)
        {
            _cachedContextualStore = contextualStore;
            if (contextual.GetType().IsSerializable)
            {
                _serializable = contextual;
            }
            else
            {
                _id = contextualStore.PutIfAbsent(contextual);
            }

            _cached = contextual;
        }

        protected override IContextual Delegate
        {
            get { return _cached ?? (_cached = LoadContextual()); }
        }

        private IContextual LoadContextual()
        {
            if (_serializable != null) {
                return _serializable;
            }
            if (_id != null) {
                return GetContextualStore().GetContextual(_id);
            }
            throw new InvalidOperationException("Error restoring serialized contextual with id " + _id);
        }

        private IContextualStore GetContextualStore()
        {
            return _cachedContextualStore ??
                   (_cachedContextualStore = Container.Instance.GetReference<IContextualStore>());
        }

        public override bool Equals(object obj)
        {
            // if the arriving object is also a SerializableContextual, then unwrap it
            var contextual = obj as SerializableContextual;
            if (contextual != null) {
                return Delegate.Equals(contextual.Delegate);
            }
            return Delegate.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Delegate.GetHashCode();
        }
    }

    public interface IDependentContext: IContext
    {
    }

    public class DependentContext : IDependentContext
    {
        readonly IContextualStore _store;

        public DependentContext(IContextualStore store)
        {
            _store = store;
        }

        public Type Scope
        {
            get { return typeof (DependentAttribute); }
        }

        public object Get(IContextual contextual, ICreationalContext creationalContext)
        {
            if (IsActive) 
            {
                throw new ContextNotActiveException();
            }
            if (creationalContext != null) {
                var instance = contextual.Create(creationalContext);
                if (creationalContext is IWeldCreationalContext) {
                    AddDependentInstance(instance, contextual, (IWeldCreationalContext)creationalContext);
                }
                return instance;
            }
            return null;
        }

        private void AddDependentInstance(object instance, IContextual contextual, IWeldCreationalContext creationalContext)
        {
            // by this we are making sure that the dependent instance has no transitive dependency with @PreDestroy / disposal method
            if (!creationalContext.DependentInstances.Any())
            {
                var classComponent = contextual as ClassComponent;
                if (classComponent != null)
                {
                    if (!classComponent.PreDestroys.Any()
                        /* TODO: && component.HasInterceptors && component.HasDefaultProducer */)
                    {
                        return;
                    }
                }
                var producer = contextual as ProducerMethod;
                if (producer != null)
                {
                    /*TODO: if (producer.DisposalMethod == null && producer.HasDefaultProducer) */
                        return;
                }

                var componentInstance = new SerializableContextualInstance(contextual, instance, creationalContext, _store);
                creationalContext.AddDependentInstance(componentInstance);
            }
        }

        public bool IsActive
        {
            get { return true; }
        }

        public object Get(IContextual contextual)
        {
            return Get(contextual, null);
        }
    }

    
}
