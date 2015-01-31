using Cormo.Contexts;

namespace Cormo.Impl.Weld.Contexts
{
    public interface IContextualInstance
    {
        object Instance { get; }
        ICreationalContext CreationalContext { get; }
        IContextual Contextual { get; }
    }
}