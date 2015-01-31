using Cormo.Contexts;
using Cormo.Impl.Weld.Components;

namespace Cormo.Impl.Weld.Serialization
{
    public interface IContextualStore: IService
    {
        ComponentIdentifier PutIfAbsent(IContextual contextual);
        IContextual GetContextual(ComponentIdentifier id);
    }
}