using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Utils;

namespace Cormo.Impl.Weld.Introspectors
{
    public class InjectableConstructor : InjectableMethodBase
    {
        private readonly ConstructorInfo _ctor;

        public InjectableConstructor(IWeldComponent component, ConstructorInfo ctor)
            : base(component, ctor, null)
        {
            _ctor = ctor;
        }

        public ConstructorInfo Constructor { get { return _ctor; } }

        protected override object Invoke(object[] parameters, ICreationalContext creationalContext)
        {
            return Activator.CreateInstance(_ctor.DeclaringType, parameters);
        }

        public override InjectableMethodBase TranslateGenericArguments(IWeldComponent component, IDictionary<Type, Type> translations)
        {
            var resolvedCtor = GenericUtils.TranslateConstructorGenericArguments(_ctor, translations);
            return new InjectableConstructor(component, resolvedCtor);
        }
    }
}