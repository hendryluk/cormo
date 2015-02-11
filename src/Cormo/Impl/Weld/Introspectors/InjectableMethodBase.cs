using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Injections;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Introspectors
{
    public abstract class InjectableMethodBase
    {
        public IWeldComponent Component { get; private set; }
        private readonly MethodBase _method;
        protected readonly ParameterInfo SpecialParameter;
        private readonly MethodParameterInjectionPoint[] _injectionPoints;

        protected InjectableMethodBase(IWeldComponent component, MethodBase method, ParameterInfo specialParameter)
        {
            Component = component;
            _method = method;
            SpecialParameter = specialParameter;
            IsConstructor = _method is ConstructorInfo;
            _injectionPoints = method.GetParameters()
                .Select(p => p==specialParameter? null: new MethodParameterInjectionPoint(component, p, AttributeUtils.GetBinders(p)))
                .ToArray();
        }

        public IEnumerable<IWeldInjetionPoint> InjectionPoints
        {
            get { return _injectionPoints; }
        }

        public object InvokeWithSpecialValue(ICreationalContext creationalContext, object specialParameterValue)
        {
            var parameters = _injectionPoints
                .Select(x => x == null ? specialParameterValue : x.GetValue(creationalContext))
                .ToArray();
            return Invoke(parameters, creationalContext);
        }

        public object Invoke(ICreationalContext creationalContext)
        {
            if (SpecialParameter != null)
                throw new InvalidOperationException("Must be invoked with InvokeWithSpecialValue");

            return Invoke(GetParameterValues(creationalContext), creationalContext);
        }

        public object[] GetParameterValues(ICreationalContext creationalContext)
        {
            return _injectionPoints
                .Select(x => x.GetValue(creationalContext))
                .ToArray();
        }

        protected abstract object Invoke(object[] parameters, ICreationalContext creationalContext);

        public abstract InjectableMethodBase TranslateGenericArguments(IWeldComponent weldComponent,
            IDictionary<Type, Type> translations);

        public bool IsConstructor { get; private set; }

        public IEnumerable<IChainValidatable> LinearValidatables
        {
            get
            {
                return InjectionPoints
                        .Where(x => !ScopeAttribute.IsNormal(x.Scope))
                        .Select(x => x.Component)
                        .OfType<IWeldComponent>();
            }
        }

        public IEnumerable<IChainValidatable> NonLinearValidatables
        {
            get
            {
                return InjectionPoints
                        .Where(x => ScopeAttribute.IsNormal(x.Scope))
                        .Select(x => x.Component)
                        .OfType<IWeldComponent>();
            }
        }
    }
}