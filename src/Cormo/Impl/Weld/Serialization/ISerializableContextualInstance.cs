using Cormo.Impl.Weld.Contexts;

namespace Cormo.Impl.Weld.Serialization
{
    public interface ISerializableContextualInstance : IContextualInstance
    {
        new ISerializableContextual Contextual { get; }
    }
}