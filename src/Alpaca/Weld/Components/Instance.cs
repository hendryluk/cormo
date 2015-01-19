using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Injects;
using Alpaca.Weld.Validations;

namespace Alpaca.Weld.Components
{
    public class Instance<T>: IInstance<T>
    {
        private readonly QualifierAttribute[] _qualifiers;
        private readonly IWeldComponent[] _components;

        public Instance(QualifierAttribute[] qualifiers, IWeldComponent[] components)
        {
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
                ResolutionValidator.ValidateSingleResult(typeof(T), _qualifiers, _components);
                return this.First();
            }
        }
    }
}