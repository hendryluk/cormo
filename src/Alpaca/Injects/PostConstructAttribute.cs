using System;

namespace Alpaca.Injects
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostConstructAttribute: Attribute
    {
    }
}