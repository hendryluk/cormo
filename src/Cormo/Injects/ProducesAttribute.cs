using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ProducesAttribute: Attribute, IBinderAttribute
    {
    }
}