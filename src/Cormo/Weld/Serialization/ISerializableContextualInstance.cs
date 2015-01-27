using Cormo.Weld.Contexts;

namespace Cormo.Weld.Serialization
{
    public interface ISerializableContextualInstance : IContextualInstance
    {
        new ISerializableContextual Contextual { get; }
    }
}