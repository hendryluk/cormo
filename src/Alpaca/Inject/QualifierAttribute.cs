using System;

namespace Alpaca.Inject
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class QualifierAttribute: Attribute
    {
    }
}