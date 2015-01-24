using Alpaca.Weld.Contexts;

namespace Alpaca.Weld.Serialization
{
    public interface ISerializableContextualInstance : IContextualInstance
    {
        new ISerializableContextual Contextual { get; }
    }
}