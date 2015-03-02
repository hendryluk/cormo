using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Introspectors
{
    public class InjectableMethod : InjectableMethodBase
    {
        public InjectableMethod(IWeldComponent component, MethodInfo method, ParameterInfo specialParameter) : 
            base(component, method, specialParameter)
        {
            Method = method;
            IsAsync = typeof (Task).IsAssignableFrom(method.ReturnType);
        }

        public bool IsAsync { get; private set; }

        public MethodInfo Method { get; private set; }

        protected override object Invoke(object[] parameters, ICreationalContext creationalContext)
        {
            var containingObject = Method.IsStatic ? null : Component.Manager.GetReference(Component, creationalContext);
            return Method.Invoke(containingObject, parameters);
        }

        public object InvokeWithInstance(object instance, ICreationalContext creationalContext)
        {
            var parameters = GetParameterValues(creationalContext);
            return Method.Invoke(Method.IsStatic?null: instance, parameters);
        }

        public Task InvokeAsyncWithSpecialValue(ICreationalContext creationalContext, object specialParameterValue)
        {
            var result = InvokeWithSpecialValue(creationalContext, specialParameterValue);
            if (IsAsync)
                return (Task) result;

            return Task.FromResult(result);
        }

        public override InjectableMethodBase TranslateGenericArguments(IWeldComponent component, IDictionary<Type, Type> translations)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(Method, translations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;
            return new InjectableMethod(component, resolvedMethod, SpecialParameter);
        }
    }
}