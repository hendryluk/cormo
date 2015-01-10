using System;

namespace Alpaca.Weld.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ProducesAttribute: Attribute
    {
    }
}