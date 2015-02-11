using System;
using System.Reflection;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ProducerMethod : AbstractProducer
    {
        private readonly InjectableMethod _method;

        public ProducerMethod(IWeldComponent declaringComponent, MethodInfo method, IBinders binders, Type scope, WeldComponentManager manager)
            : base(declaringComponent, method, method.ReturnType, binders, scope, manager)
        {
            _method = new InjectableMethod(declaringComponent, method, null);
        }

        protected override AbstractProducer TranslateTypes(GenericResolver.Resolution resolution)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(_method.Method, resolution.GenericParameterTranslations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;

            return new ProducerMethod(DeclaringComponent.Resolve(resolvedMethod.DeclaringType), resolvedMethod, Binders, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return _method.Invoke;
        }

        public override string ToString()
        {
            return string.Format("Producer Method [{0}] with Qualifiers [{1}]", _method.Method, string.Join(",", Qualifiers));
        }
    }
}