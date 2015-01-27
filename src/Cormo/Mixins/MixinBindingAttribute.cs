using System;
using Cormo.Injects;

namespace Cormo.Mixins
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class MixinBindingAttribute : QualifierAttribute
    {
    }
}