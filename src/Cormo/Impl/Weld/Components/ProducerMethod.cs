using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Components
{
    public class ProducerMethod : AbstractProducer
    {
        private readonly InjectableMethod _method;

        public ProducerMethod(IWeldComponent declaringComponent, IAnnotatedMethod method, WeldComponentManager manager)
            : this(declaringComponent, method.Method, method.Annotations, manager)
        {
        }

        public ProducerMethod(IWeldComponent declaringComponent, MethodInfo method, IAnnotations annotations, WeldComponentManager manager)
            : base(declaringComponent, method, method.ReturnType, annotations, manager)
        {
            _method = new InjectableMethod(declaringComponent, method, null);
        }

        protected override AbstractProducer TranslateTypes(GenericResolver.Resolution resolution)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(_method.Method, resolution.GenericParameterTranslations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;

            return new ProducerMethod(DeclaringComponent.Resolve(resolvedMethod.DeclaringType), resolvedMethod, Annotations, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return _method.Invoke;
        }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get
            {
                if (!IsConcrete)
                    return Enumerable.Empty<IChainValidatable>();

                return base.NextLinearValidatables.Union(
                    _method.InjectionPoints
                        .Where(x => !ScopeAttribute.IsNormal(x.Scope))
                        .Select(x => x.Component).OfType<IWeldComponent>());
            }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get
            {
                if (!IsConcrete)
                    return Enumerable.Empty<IChainValidatable>();
                
                return _method.NonLinearValidatables;
            }
        }

        public override string ToString()
        {
            return string.Format("Producer Method [{0}] with Qualifiers [{1}]", _method.Method, string.Join(",", Qualifiers));
        }
    }
}