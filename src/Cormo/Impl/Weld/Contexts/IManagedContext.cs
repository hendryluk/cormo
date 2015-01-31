using Cormo.Contexts;

namespace Cormo.Impl.Weld.Contexts
{
    public interface IManagedContext : IContext
    {
        void Activate();
        void Deactivate();
    }
}