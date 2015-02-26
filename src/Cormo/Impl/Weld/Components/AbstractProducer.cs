using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Utils;

namespace Cormo.Impl.Weld.Components
{
    public abstract class AbstractProducer: AbstractComponent
    {
        private readonly bool _containsGenericParameters;
        
        protected AbstractProducer(IWeldComponent declaringComponent, MemberInfo member, Type returnType, WeldComponentManager manager)
            : base(member.ToString(), returnType, member.GetBinders(), manager)
        {
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(member);
            DeclaringComponent = declaringComponent;
        }

        public bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override void Touch()
        {
            DeclaringComponent.Touch();
        }

        public IWeldComponent DeclaringComponent { get; private set; }
        
        public override IWeldComponent Resolve(Type requestedType)
        {
            if (IsConcrete)
                return requestedType.IsAssignableFrom(Type) ? this : null;

            var typeResolution = GenericResolver.ImplementerResolver.ResolveType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            return TranslateTypes(typeResolution);
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            // TODO DisposeAttribute
        }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get { yield return DeclaringComponent; }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get { yield break; }
        }

        protected abstract AbstractProducer TranslateTypes(GenericResolver.Resolution resolution);
    }
}