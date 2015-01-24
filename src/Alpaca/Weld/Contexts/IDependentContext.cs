using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Utils;
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
        private static readonly String GeneratedIdPrefix = typeof(ContextualStore).Name;

        private readonly ConcurrentDictionary<IContextual, string> _contextuals = new ConcurrentDictionary<IContextual, string>();
        private readonly ConcurrentDictionary<string, IContextual> _contextualsInverse = new ConcurrentDictionary<string, IContextual>();
        private readonly ConcurrentDictionary<string, IContextual> _passivationCapableContextuals = new ConcurrentDictionary<string, IContextual>();

        private int _idIncrement = 0;
        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add a contextual (if not already present) to the store, and return it's
        /// id. If the contextual is passivation capable, it's id will be used,
        /// otherwise an id will be generated
        /// </summary>
        /// <param name="contextual">contextual the contexutal to add</param>
        /// <returns>the current id for the contextual</returns>
        public string PutIfAbsent(IContextual contextual)
        {
            var passivationCapable = contextual as IPassivationCapable;
            if (passivationCapable != null)
            {
                var id = passivationCapable.Id;
                _passivationCapableContextuals.TryAdd(id, contextual);
                return id;
            }

            return _contextuals.GetOrAdd(contextual, _ =>
            {
                var id = string.Format("{0}{1}", GeneratedIdPrefix, Interlocked.Increment(ref _idIncrement));
                _contextualsInverse[id] = contextual;
                return id;
            });
        }

        public IContextual GetContextual(string id)
        {
            if (id.StartsWith(GeneratedIdPrefix))
            {
                return _contextualsInverse.GetOrDefault(id);
            }
            else
            {
                return _passivationCapableContextuals.GetOrDefault(id);
            }
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
            if (!IsActive) 
            {
                throw new ContextNotActiveException(Scope);
            }
            if (creationalContext != null) {
                var instance = contextual.Create(creationalContext);
                var weldContext = creationalContext as IWeldCreationalContext;
                if (weldContext != null) {
                    AddDependentInstance(instance, contextual, weldContext);
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
