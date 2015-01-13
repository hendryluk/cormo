using System;

namespace Alpaca.Inject
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConditionalOnMissingBeanAttribute: Attribute
    {        
    }
}