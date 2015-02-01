using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class StereotypeAttribute: Attribute, IBinderAttribute
    {
    }
}