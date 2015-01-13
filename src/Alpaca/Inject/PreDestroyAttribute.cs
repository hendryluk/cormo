using System;

namespace Alpaca.Inject
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PreDestroyAttribute: Attribute
    { 
    }
}