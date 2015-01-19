using System;

namespace Alpaca.Injects
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConditionalOnMissingBeanAttribute: Attribute
    {        
    }
}