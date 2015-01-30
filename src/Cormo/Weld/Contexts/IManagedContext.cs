using Cormo.Contexts;

namespace Cormo.Weld.Contexts
{
    public interface IManagedContext : IContext
    {
        void Activate();
        void Deactivate();
    }
}