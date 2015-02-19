using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class Binders : IBinders
    {
        private readonly IBinderAttribute[] _binders;
        public IQualifiers Qualifiers { get; private set; }
        public Type[] Types { get; private set; }

        public static readonly Binders Empty = new Binders(new IBinderAttribute[0]);
        public Binders (IEnumerable<IBinderAttribute> binders)
        {
            _binders = binders.ToArray();
            Types = _binders.Select(x => x.GetType()).ToArray();
            Qualifiers = new Qualifiers(_binders.OfType<IQualifier>().ToArray());
        }

        public IEnumerator<IBinderAttribute> GetEnumerator()
        {
            return _binders.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Qualifiers: IQualifiers
    {
        public static readonly Qualifiers Empty = new Qualifiers(new IQualifier[0]);

        protected bool Equals(Qualifiers other)
        {
            return Equals(Types, other.Types);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Qualifiers) obj);
        }

        public override int GetHashCode()
        {
            return (Types != null ? Types.GetHashCode() : 0);
        }

        private readonly IQualifier[] _qualifiers;

        public Qualifiers(IEnumerable<IQualifier> qualifiers)
        {
            _qualifiers = qualifiers.DefaultIfEmpty(DefaultAttribute.Instance).ToArray();
            Types = _qualifiers.Select(x => x.GetType()).ToArray();
        }

        public IEnumerator<IQualifier> GetEnumerator()
        {
            return _qualifiers.AsEnumerable().GetEnumerator();
        }

        public bool CanSatisfy(IQualifiers qualifiers)
        {
            return qualifiers.Types.All(Types.Contains);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type[] Types { get; private set; }
    }
}