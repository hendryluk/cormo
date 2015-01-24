using System;
using System.Collections.Generic;
using Alpaca.Injects;

namespace Alpaca.Weld.Components
{
    public interface IWeldComponent : IComponent, IPassivationCapable<ComponentIdentifier>
    {
        IWeldComponent Resolve(Type type);
        bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers);

        bool IsProxyRequired { get; }
        bool IsConcrete { get; }
    }

    [Serializable]
    public class ComponentIdentifier
    {
        protected bool Equals(ComponentIdentifier other)
        {
            return string.Equals(Key, other.Key) && ResolverType == other.ResolverType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ComponentIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ (ResolverType != null ? ResolverType.GetHashCode() : 0);
            }
        }

        public ComponentIdentifier(string key, Type resolverType=null)
        {
            Key = key;
            ResolverType = resolverType;
        }

        public string Key { get; set; }
        public Type ResolverType { get; set; }
    }
}