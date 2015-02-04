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

        public static readonly Binders Empty = new Binders(new IBinderAttribute[0]);
        public Binders (IEnumerable<IBinderAttribute> binders)
        {
            _binders = binders.ToArray();
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
        private readonly IQualifier[] _qualifiers;

        public Qualifiers(IEnumerable<IQualifier> qualifiers)
        {
            _qualifiers = qualifiers.DefaultIfEmpty(DefaultAttribute.Instance).ToArray();
        }

        public IEnumerator<IQualifier> GetEnumerator()
        {
            return _qualifiers.AsEnumerable().GetEnumerator();
        }

        public bool CanSatisfy(IQualifiers qualifiers)
        {
            var qualifierTypes = qualifiers.Select(x => x.GetType()).ToArray();
            return qualifierTypes.All(_qualifiers.Select(x => x.GetType()).Contains);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}