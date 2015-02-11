using System;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class EventObserverMethod
    {
        private readonly ParameterInfo _parameter;
        private readonly InjectableMethod _method;
        private readonly Type _eventType;

        public EventObserverMethod(IWeldComponent component, ParameterInfo parameter)
        {
            _parameter = parameter;
            _eventType = parameter.ParameterType;
            IsConcrete = _eventType.ContainsGenericParameters;
            _method = new InjectableMethod(component, (MethodInfo) parameter.Member, parameter);
        }

        public bool IsConcrete { get; set; }

        public EventObserverMethod Resolve(Type eventType)
        {
            if (IsConcrete)
                return _eventType.IsAssignableFrom(eventType)? this: null;

            var resolution = GenericResolver.AncestorResolver.ResolveType(_eventType, eventType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            return TranslateTypes(resolution);
        }

        protected EventObserverMethod TranslateTypes(GenericResolver.Resolution resolution)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(_method.Method, resolution.GenericParameterTranslations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;

            var resolvedParam = resolvedMethod.GetParameters()[_parameter.Position];
            var resolvedComponent = _method.Component.Resolve(resolvedMethod.DeclaringType);
            return new EventObserverMethod(resolvedComponent, resolvedParam);
        }

        public void Notify(object ev, ICreationalContext creationalContext)
        {
            _method.InvokeWithSpecialValue(creationalContext, ev);
        }
    }
}