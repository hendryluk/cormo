using System;
using Cormo.Contexts;

namespace Cormo.Injects
{
    [AttributeUsage(AttributeTargets.Class)]
    [Singleton]
    public sealed class ConfigurationAttribute: StereotypeAttribute
    {
         
    }
}