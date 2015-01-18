using System;

namespace Alpaca.Injects
{
    public sealed class DefaultAttribute: QualifierAttribute
    {
        public static readonly DefaultAttribute Instance = new DefaultAttribute();
    }
}