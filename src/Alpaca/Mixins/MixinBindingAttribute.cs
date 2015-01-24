using System;
using Alpaca.Injects;

namespace Alpaca.Mixins
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class MixinBindingAttribute : QualifierAttribute
    {
    }
}