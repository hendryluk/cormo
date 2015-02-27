using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConditionalOnMissingComponentAttribute: Attribute, IAnnotation
    {
    }
}