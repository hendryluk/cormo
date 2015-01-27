using System;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostConstructAttribute: Attribute
    {
    }
}