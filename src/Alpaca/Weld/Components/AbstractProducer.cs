using System;
using System.Collections.Generic;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Components
{
    public abstract class AbstractProducer: AbstractComponent
    {
        private readonly MemberInfo _member;
        private readonly bool _containsGenericParameters;
        private readonly Lazy<IComponent> _lazyDeclaringComponent;

        protected AbstractProducer(MemberInfo member, Type returnType,
            IEnumerable<QualifierAttribute> qualifiers, Type scope,
            WeldComponentManager manager)
            : base(member.ToString(), returnType, qualifiers, scope, manager)
        {
            _member = member;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(member);
            _lazyDeclaringComponent = new Lazy<IComponent>(GetDeclaringComponent);
        }

        private IComponent GetDeclaringComponent()
        {
            return Manager.GetComponent(_member.ReflectedType);
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public IComponent DeclaringComponent
        {
            get { return _lazyDeclaringComponent.Value; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (IsConcrete)
                return requestedType.IsAssignableFrom(Type) ? this : null;

            var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            var component = TranslateTypes(typeResolution);
            if(component != null)
                TransferInjectionPointsTo(component, typeResolution);
            return component;
        }

        protected abstract AbstractProducer TranslateTypes(GenericUtils.Resolution resolution);
    }
}