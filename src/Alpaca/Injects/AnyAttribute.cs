using System;

namespace Alpaca.Injects
{
    [Qualifier]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class AnyAttribute: Attribute
    { 
    }
}