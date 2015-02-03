using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Validations;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class Instance<T>: IInstance<T>
    {
        private readonly IQualifier[] _qualifiers;
        private readonly IWeldComponent[] _components;
        private readonly ICreationalContext _creationalContext;
        private readonly IInjectionPoint _injectionPoint;
        private readonly CurrentInjectionPoint _currentInjectionPoint;

        public Instance(WeldComponentManager manager, IQualifier[] qualifiers, IWeldComponent[] components, ICreationalContext creationalContext)
        {
            _qualifiers = qualifiers;
            _components = components;
            _creationalContext = creationalContext;

            _currentInjectionPoint = manager.GetService<CurrentInjectionPoint>();
            _injectionPoint = _currentInjectionPoint.Peek();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _components.Select(x =>
            {
                try
                {
                    _currentInjectionPoint.Push(_injectionPoint);
                    return x.Manager.GetReference(x, _creationalContext);
                }
                finally
                {
                    _currentInjectionPoint.Pop();
                }
                
            }).Cast<T>().GetEnumerator();
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