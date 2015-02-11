using Cormo.Events;
using Cormo.Impl.Weld.Introspectors;

namespace Cormo.Impl.Weld.Components
{
    public class Events<T>: IEvents<T>
    {
        private readonly EventObserverMethod[] _methods;

        public Events(EventObserverMethod[] methods)
        {
            _methods = methods;
        }

        public void Fire(T @event)
        {
            foreach(var m in _methods)
                m.Notify(@event);
        }
    }
}