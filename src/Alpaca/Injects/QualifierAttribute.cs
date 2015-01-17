using System;

namespace Alpaca.Injects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public abstract class QualifierAttribute: Attribute
    {
    }
}