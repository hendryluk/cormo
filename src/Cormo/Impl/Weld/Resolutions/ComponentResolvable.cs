using System;
using System.Collections.Generic;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Resolutions
{
    public class ComponentResolvable: IResolvable
    {
        public Type Type { get; private set; }
        public IQualifiers Qualifiers { get; set; }

        public ComponentResolvable(Type type, IEnumerable<IQualifier> qualifiers):
            this(type, new Qualifiers(qualifiers))
        {
        }

        public ComponentResolvable(Type type, IQualifiers qualifiers)
        {
            Type = type;
            Qualifiers = qualifiers;
        }

        protected bool Equals(ComponentResolvable other)
        {
            return Type == other.Type && Equals(Qualifiers.Types, other.Qualifiers.Types);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ComponentResolvable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Qualifiers.Types != null ? Qualifiers.Types.GetHashCode() : 0);
            }
        }
    }
}