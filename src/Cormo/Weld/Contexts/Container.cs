using Cormo.Injects;

namespace Cormo.Weld.Contexts
{
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
}