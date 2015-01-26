using Alpaca.Contexts;

namespace Alpaca.Weld.Contexts
{
    public interface IContextualInstance
    {
        object Instance { get; }
        ICreationalContext CreationalContext { get; }
        IContextual Contextual { get; }
    }
}