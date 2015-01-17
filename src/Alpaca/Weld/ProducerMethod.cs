using System;
using System.Collections.Generic;
using System.Reflection;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public class ProducerMethod : AbstractComponent
    {
        private readonly bool _containsGenericParameters;
        private readonly MethodInfo _method;

        public ProducerMethod(MethodInfo method, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope, IComponentManager manager)
            : base(method.ReturnType, qualifiers, scope, manager)
        {
            _method = method;
            _containsGenericParameters = GenericUtils.MemberContainsGenericArguments(method);
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!_containsGenericParameters)
                return requestedType.IsAssignableFrom(Type)? this: null;

            var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (typeResolution == null || typeResolution.ResolvedType == null || typeResolution.ResolvedType.ContainsGenericParameters)
                return null;

            var resolvedProducer = GenericUtils.TranslateMethodGenericArguments(_method, typeResolution.GenericParameterTranslations);
            if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
                return null;

            var method = resolvedProducer;
            if (method != null)
            {
                var component = new ProducerMethod(method, Qualifiers, Scope, Manager);
                TransferInjectionPointsTo(component, typeResolution);
            }

            return null;
        }

        protected override BuildPlan GetBuildPlan()
        {
            // TODO
            return null;
            //return engine.MakeExecutionPlan(Producer);
        }
    }
}