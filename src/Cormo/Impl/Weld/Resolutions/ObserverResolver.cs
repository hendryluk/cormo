using System.Collections.Generic;
using System.Linq;
using Cormo.Impl.Weld.Introspectors;

namespace Cormo.Impl.Weld.Resolutions
{
    public class ObserverResolver : TypeSafeResolver<EventObserverMethod, ObserverResolvable>
    {
        public ObserverResolver(WeldComponentManager manager, IEnumerable<EventObserverMethod> allEvents)
            : base(manager, allEvents)
        {
        }

        protected override IEnumerable<EventObserverMethod> Resolve(ObserverResolvable resolvable, ref IEnumerable<EventObserverMethod> observers)
        {
            return observers
                .Where(x => x.Qualifiers.CanSatisfy(resolvable.Qualifiers))
                .Select(x => x.Resolve(resolvable.EventType));
        }
    }
}