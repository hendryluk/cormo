using Cormo.Contexts;

namespace Cormo.Weld.Contexts
{
    public interface IContextualInstance
    {
        object Instance { get; }
        ICreationalContext CreationalContext { get; }
        IContextual Contextual { get; }
    }
}