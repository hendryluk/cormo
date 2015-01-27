using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Injects;
using Cormo.Weld.Validations;

namespace Cormo.Weld.Components
{
    public class Instance<T>: IInstance<T>
    {
        private readonly QualifierAttribute[] _qualifiers;
        private readonly IWeldComponent[] _components;
        private readonly ICreationalContext _creationalContext;

        public Instance(QualifierAttribute[] qualifiers, IWeldComponent[] components, ICreationalContext creationalContext)
        {
            _qualifiers = qualifiers;
            _components = components;
            _creationalContext = creationalContext;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _components.Select(x => x.Manager.GetReference(x, _creationalContext)).Cast<T>().GetEnumerator();
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