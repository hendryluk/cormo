namespace Cormo.Impl.Weld.Contexts
{
    public abstract class AbstractSharedContext : AbstractContext
    {
        private readonly IComponentStore _componentStore;
        protected override IComponentStore ComponentStore
        {
            get { return _componentStore; }
        }

        protected AbstractSharedContext()
        {
            _componentStore = new ConcurrentDictionaryComponentStore();
        }

        public override bool IsActive { get { return true; } }
        
    }
}