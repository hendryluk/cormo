using System;

namespace Alpaca.Weld.Attributes
{
    [Qualifier]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class DefaultAttribute: Attribute
    {
    }
}