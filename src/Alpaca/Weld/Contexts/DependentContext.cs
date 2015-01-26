using System;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Components;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Serialization;

namespace Alpaca.Weld.Contexts
{
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

        public object Get(IContextual contextual, ICreationalContext creationalContext, IInjectionPoint injectionPoint)
        {
            if (!IsActive) 
            {
                throw new ContextNotActiveException(Scope);
            }
            if (creationalContext != null) {
                var instance = contextual.Create(creationalContext, injectionPoint);
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
                    if (!classComponent.IsDisposable
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
            return Get(contextual, null, null);
        }
    }
}