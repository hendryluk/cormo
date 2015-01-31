using Cormo.Impl.Weld.Serialization;

namespace Cormo.Impl.Weld.Contexts
{
    public class Container
    {
        public static readonly Container Instance = new Container();
        private WeldComponentManager _componentManager;

        public IContextualStore ContextualStore
        {
            get { return _componentManager.ContextualStore; }
        }

        public void Initialize(WeldComponentManager componentManager)
        {
            _componentManager = componentManager;
        }

        public T GetReference<T>()
        {
            return _componentManager.GetReference<T>();
        }
    }
}