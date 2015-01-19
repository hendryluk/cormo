using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Injects;
using Alpaca.Weld.Validation;

namespace Alpaca.Weld.Component
{
    public class Instance<T>: IInstance<T>
    {
        private readonly Type _type;
        private readonly QualifierAttribute[] _qualifiers;
        private readonly IWeldComponent[] _components;

        public Instance(Type type, QualifierAttribute[] qualifiers, IWeldComponent[] components)
        {
            _type = type;
            _qualifiers = qualifiers;
            _components = components;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _components.Select(x => x.Manager.GetReference(x)).Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Value
        {
            get
            {
                ResolutionValidator.ValidateSingleResult(_type, _qualifiers, _components);
                return this.First();
            }
        }
    }
}