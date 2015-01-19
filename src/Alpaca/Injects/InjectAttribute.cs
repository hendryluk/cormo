using System;

namespace Alpaca.Inject
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor| AttributeTargets.Property)]
    public class InjectAttribute: Attribute
    {
         
    }
}