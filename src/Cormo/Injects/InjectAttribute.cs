using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor| AttributeTargets.Property)]
    public class InjectAttribute: Attribute
    {
         
    }
}