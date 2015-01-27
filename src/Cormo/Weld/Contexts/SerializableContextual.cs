using System;
using Cormo.Contexts;
using Cormo.Weld.Components;
using Cormo.Weld.Serialization;

namespace Cormo.Weld.Contexts
{
    [Serializable]
    public class SerializableContextual : ForwardingContextual, ISerializableContextual
    {
        private readonly IContextual _serializable;
        
        [NonSerialized]
        private IContextualStore _cachedContextualStore;

        [NonSerialized]
        private IContextual _cached;

        private readonly ComponentIdentifier _id;

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
}