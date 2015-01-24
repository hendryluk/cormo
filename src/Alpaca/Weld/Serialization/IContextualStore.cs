using Alpaca.Contexts;
using Alpaca.Weld.Components;

namespace Alpaca.Weld.Serialization
{
    public interface IContextualStore: IService
    {
        ComponentIdentifier PutIfAbsent(IContextual contextual);
        IContextual GetContextual(ComponentIdentifier id);
    }
}