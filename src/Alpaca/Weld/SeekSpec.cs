using System;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Inject;

namespace Alpaca.Weld
{
    public struct SeekSpec
    {
        private static readonly DefaultAttribute DefaultAttributeInstance = new DefaultAttribute();
        public SeekSpec(Type type, object[] qualifiers)
            : this()
        {
            Type = type;
            Qualifiers = SetQualifierDefaults(qualifiers);
        }

        private object[] SetQualifierDefaults(object[] qualifiers)
        {
            if (!qualifiers.Any())
                return new object[] { DefaultAttributeInstance };

            return qualifiers;
        }

        public bool Multiple { get; private set; }
        public Type Type { get; private set; }
        public IEnumerable<object> Qualifiers { get; private set; }
    }
}