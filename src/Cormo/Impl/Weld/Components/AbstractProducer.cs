using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public abstract class AbstractProducer: AbstractComponent
    {
        private readonly MemberInfo _member;
        private readonly bool _containsGenericParameters;
        private readonly Lazy<IComponent> _lazyDeclaringComponent;

        protected AbstractProducer(MemberInfo member, Type returnType,
            IEnumerable<IBinderAttribute> binders, Type scope,
            WeldComponentManager manager)
            : base(member.ToString(), returnType, binders, scope, manager)
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

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            // TODO DisposeAttribute
        }

        protected abstract AbstractProducer TranslateTypes(GenericUtils.Resolution resolution);
    }
}