using System;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Resolutions
{
    public class ObserverResolvable : IResolvable
    {
        public Type EventType { get; private set; }
        public IQualifiers Qualifiers { get; private set; }

        public ObserverResolvable(Type eventType, IQualifiers qualifiers)
        {
            EventType = eventType;
            Qualifiers = qualifiers;
        }

        protected bool Equals(ObserverResolvable other)
        {
            return Equals(EventType, other.EventType) && Equals(Qualifiers, other.Qualifiers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObserverResolvable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EventType != null ? EventType.GetHashCode() : 0)*397) ^ (Qualifiers != null ? Qualifiers.GetHashCode() : 0);
            }
        }
    }
}