using Cormo.Contexts;
using Cormo.Weld.Components;

namespace Cormo.Weld.Serialization
{
    public interface IContextualStore: IService
    {
        ComponentIdentifier PutIfAbsent(IContextual contextual);
        IContextual GetContextual(ComponentIdentifier id);
    }
}