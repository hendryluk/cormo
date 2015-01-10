using System;

namespace Alpaca.Weld.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PreDestroyAttribute: Attribute
    { 
    }
}