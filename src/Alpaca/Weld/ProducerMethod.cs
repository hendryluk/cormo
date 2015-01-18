using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public abstract class AbstractProducer: AbstractComponent
    {
        private readonly MemberInfo _member;
        private readonly bool _containsGenericParameters;
        private IComponent _containingComponent;
        
        protected AbstractProducer(MemberInfo member, Type returnType,
            IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope,
            IComponentManager manager): base(returnType, qualifiers, scope, manager)
        {
            _member = member;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(member);
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override void OnDeploy()
        {
            base.OnDeploy();
            if (IsConcrete)
            {
                _containingComponent = Manager.GetComponent(_member.ReflectedType);
            }
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

        protected override BuildPlan GetBuildPlan()
        {
            return GetBuildPlan(_containingComponent);
        }

        protected abstract BuildPlan GetBuildPlan(IComponent containingComponent);
        protected abstract AbstractProducer TranslateTypes(GenericUtils.Resolution resolution);
    }

    public class ProducerField : AbstractProducer
    {
        private readonly FieldInfo _field;

        public ProducerField(FieldInfo field, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(field, field.FieldType, qualifiers, scope, manager)
        {
            _field = field;
        }


        protected override AbstractProducer TranslateTypes(GenericUtils.Resolution resolution)
        {
            var resolvedField = GenericUtils.TranslateFieldType(_field, resolution.GenericParameterTranslations);
            return new ProducerField(resolvedField, Qualifiers, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan(IComponent containingComponent)
        {
            return () =>
            {
                var containingObject = Manager.GetReference(containingComponent);
                return _field.GetValue(containingObject);
            };
        }
    }

    public class ProducerMethod : AbstractProducer
    {
        private readonly MethodInfo _method;
        
        public ProducerMethod(MethodInfo method, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(method, method.ReturnType, qualifiers, scope, manager)
        {
            _method = method;
        }


        protected override AbstractProducer TranslateTypes(GenericUtils.Resolution resolution)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(_method, resolution.GenericParameterTranslations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;

            return new ProducerMethod(resolvedMethod, Qualifiers, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan(IComponent containingComponent)
        {
            var paramInjects = InjectionPoints
                .OfType<MethodParameterInjectionPoint>()
                .Where(x => x.Member == _method)
                .OrderBy(x => x.Position).ToArray();

            return () =>
            {
                var containingObject = Manager.GetReference(containingComponent);
                var paramVals = paramInjects.Select(p => p.GetValue()).ToArray();

                return _method.Invoke(containingObject, paramVals);
            };
        }
    }
}