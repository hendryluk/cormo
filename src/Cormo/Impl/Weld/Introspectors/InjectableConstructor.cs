using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Introspectors
{
    public class InjectableConstructor : InjectableMethodBase
    {
        private readonly ConstructorInfo _ctor;

        public InjectableConstructor(IComponent component, ConstructorInfo ctor)
            : base(component, ctor, null)
        {
            _ctor = ctor;
        }

        protected override object Invoke(object[] parameters, ICreationalContext creationalContext)
        {
            return Activator.CreateInstance(_ctor.DeclaringType, parameters);
        }

        public override InjectableMethodBase TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var resolvedCtor = GenericUtils.TranslateConstructorGenericArguments(_ctor, translations);
            return new InjectableConstructor(component, resolvedCtor);
        }
    }
}