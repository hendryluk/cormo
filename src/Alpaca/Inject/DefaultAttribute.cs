using System;

namespace Alpaca.Inject
{
    [Qualifier]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class DefaultAttribute: Attribute
    {
    }
}