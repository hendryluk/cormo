using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Introspectors
{
    public class EventObserverMethod: IChainValidatable
    {
        public IWeldComponent Component { get; private set; }
        private readonly ParameterInfo _parameter;
        private readonly IAnnotations _annotations;
        private readonly InjectableMethod _method;
        private readonly Type _eventType;

        public EventObserverMethod(IWeldComponent component, ParameterInfo parameter, IAnnotations annotations)
        {
            Component = component;
            _parameter = parameter;
            _annotations = annotations;
            _eventType = parameter.ParameterType;
            IsConcrete = _eventType.ContainsGenericParameters;
            _method = new InjectableMethod(component, (MethodInfo) parameter.Member, parameter);
            NextNonLinearValidatables = _method.LinearValidatables.Union(_method.NonLinearValidatables);
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
            return new EventObserverMethod(resolvedComponent, resolvedParam, _annotations);
        }

        public void Notify(object ev)
        {
            var creationalContext = _method.Component.Manager.CreateCreationalContext(_method.Component);
            _method.InvokeWithSpecialValue(creationalContext, ev);
        }

        public IEnumerable<IChainValidatable> NextLinearValidatables { get { yield break; } }
        public IEnumerable<IChainValidatable> NextNonLinearValidatables { get; private set; }
        public IQualifiers Qualifiers { get { return _annotations.Qualifiers; } }
    }
}