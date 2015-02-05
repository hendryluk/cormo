using System;
using System.Collections.Generic;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public interface IResolvable
    {
        IQualifiers Qualifiers { get; }
        Type Type { get; }
    }

    public class Resolvable: IResolvable
    {
        public Type Type { get; private set; }
        public IQualifiers Qualifiers { get; private set; }

        public Resolvable(Type type, IEnumerable<IQualifier> qualifiers):
            this(type, new Qualifiers(qualifiers))
        {
        }

        public Resolvable(Type type, IQualifiers qualifiers)
        {
            Type = type;
            Qualifiers = qualifiers;
        }

        protected bool Equals(Resolvable other)
        {
            return Type == other.Type && Equals(Qualifiers, other.Qualifiers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Resolvable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0)*397) ^ (Qualifiers != null ? Qualifiers.GetHashCode() : 0);
            }
        }
    }
}