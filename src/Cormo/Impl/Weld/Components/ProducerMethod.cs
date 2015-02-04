using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ProducerMethod : AbstractProducer
    {
        private readonly MethodInfo _method;

        public ProducerMethod(IWeldComponent declaringComponent, MethodInfo method, IBinders binders, Type scope, WeldComponentManager manager)
            : base(declaringComponent, method, method.ReturnType, binders, scope, manager)
        {
            _method = method;
        }


        protected override AbstractProducer TranslateTypes(GenericUtils.Resolution resolution)
        {
            var resolvedMethod = GenericUtils.TranslateMethodGenericArguments(_method, resolution.GenericParameterTranslations);
            if (resolvedMethod == null || GenericUtils.MemberContainsGenericArguments(resolvedMethod))
                return null;

            return new ProducerMethod(DeclaringComponent.Resolve(resolvedMethod.DeclaringType), resolvedMethod, Binders, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            var paramInjects = InjectionPoints
                .OfType<MethodParameterInjectionPoint>()
                .Where(x => x.Member == _method)
                .OrderBy(x => x.Position).ToArray();

            return context =>
            {
                var containingObject = Manager.GetReference(DeclaringComponent, context);
                var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();

                return _method.Invoke(containingObject, paramVals);
            };
        }

        public override string ToString()
        {
            return string.Format("Producer Method [{0}] with Qualifiers [{1}]", _method, string.Join(",", Qualifiers));
        }
    }
}