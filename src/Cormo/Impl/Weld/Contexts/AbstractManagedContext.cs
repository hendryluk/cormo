namespace Cormo.Impl.Weld.Contexts
{
    public abstract class AbstractManagedContext : AbstractContext, IManagedContext
    {
        public abstract void Activate();

        public virtual void Deactivate()
        {
            foreach (var instance in ComponentStore.AllInstances)
                Destroy(instance);
        }

        private void Destroy(IContextualInstance instance)
        {
            instance.Contextual.Destroy(instance.Instance, instance.CreationalContext);
        }
    }
}