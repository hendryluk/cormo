using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Introspectors;

namespace Cormo.Impl.Weld.Components
{
    public class EventComponent : AbstractComponent
    {
        private readonly Type _eventType;
        private readonly Lazy<EventObserverMethod[]> _lazyEventObserverMethods;
        
        public EventComponent(Type eventType, Binders binders, WeldComponentManager manager) :
            base("", typeof(Events<>).MakeGenericType(eventType), binders, typeof(DependentAttribute), manager)
        {
            _eventType = eventType;
            _lazyEventObserverMethods = new Lazy<EventObserverMethod[]>(ResolveObserverMethods);
        }

        private EventObserverMethod[] ResolveObserverMethods()
        {
            return Manager.ResolveObservers(_eventType, Binders.Qualifiers).ToArray();
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            // WELD-1010 hack?
            var context = creationalContext as IWeldCreationalContext;
            if (context != null)
            {
                context.Release(this, instance);
            }
            else
            {
                creationalContext.Release();
            }
        }

        protected override BuildPlan GetBuildPlan()
        {
            return context => Activator.CreateInstance(Type,
                new object[]{_lazyEventObserverMethods.Value});
        }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get { yield break; }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get { return _lazyEventObserverMethods.Value; }
        }
    }
}