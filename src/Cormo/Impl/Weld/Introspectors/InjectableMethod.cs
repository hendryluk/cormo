using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Introspectors
{
    public class InjectableMethod : InjectableMethodBase
    {
        private readonly IComponent _component;
        
        public InjectableMethod(IComponent component, MethodInfo method, ParameterInfo specialParameter) : 
            base(component, method, specialParameter)
        {
            _component = component;
            Method = method;
        }

        public MethodInfo Method { get; private set; }

        protected override object Invoke(object[] parameters, ICreationalContext creationalContext)
        {
            var containingObject = Method.IsStatic ? null : Component.Manager.GetReference(_component, creationalContext);
            return Method.Invoke(containingObject, parameters);
        }

        public override InjectableMethodBase TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(Method, translations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;
            return new InjectableMethod(component, resolvedMethod, SpecialParameter);
        }
    }
}