using System;

namespace Alpaca.Weld.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor| AttributeTargets.Property)]
    public class InjectAttribute: Attribute
    {
         
    }
}