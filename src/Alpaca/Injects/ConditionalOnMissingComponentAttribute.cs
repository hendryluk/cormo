using System;

namespace Alpaca.Injects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConditionalOnMissingComponentAttribute: Attribute
    {
    }
}