using System.Linq;
using System.Threading.Tasks;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Injects;

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
            var tasks = Task.WhenAll(_methods.Select(m => m.Notify(@event)).ToArray());
            Task.Run(() => tasks).Wait();
        }
        public Task FireAsync(T @event)
        {
            return Task.WhenAll(_methods.Select(m => m.Notify(@event)).ToArray());
        }
    }
}