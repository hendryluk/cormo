using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public class ClassComponent : AbstractComponent
    {
        private readonly IEnumerable<MethodInfo> _postConstructs;
        private readonly IEnumerable<MethodInfo> _preDestroys;
        private readonly bool _containsGenericParameters;
        
        public ClassComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, ScopeAttribute scope,  IComponentManager manager, MethodInfo[] postConstructs, MethodInfo[] preDestroys)
            : base(type, qualifiers, scope, manager)
        {
            _postConstructs = postConstructs;
            _preDestroys = preDestroys;
            _containsGenericParameters = Type.ContainsGenericParameters;

            ValidateMethodSignatures();
        }

        private void ValidateMethodSignatures()
        {
            foreach (var m in _postConstructs)
            {
                PostConstructCriteria.Validate(m);
            }
            foreach (var m in _preDestroys)
            {
                PreDestroyCriteria.Validate(m);
            }
        }

        public override bool IsConcrete
        {
            get { return !_containsGenericParameters; }
        }

        public override IWeldComponent Resolve(Type requestedType)
        {
            if (!_containsGenericParameters)
                return requestedType.IsAssignableFrom(Type)? this: null;

            var resolution = GenericUtils.ResolveGenericType(Type, requestedType);
            if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            var postConstructs = _postConstructs.Select(x=> GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();
            var preDestroys = _preDestroys.Select(x => GenericUtils.TranslateMethodGenericArguments(x, resolution.GenericParameterTranslations)).ToArray();

            var components = new ClassComponent(resolution.ResolvedType, Qualifiers, Scope, Manager, postConstructs, preDestroys);
            TransferInjectionPointsTo(components, resolution);
            return components;
        }

        protected override BuildPlan GetBuildPlan()
        {
            var paramInject = InjectionPoints.OfType<MethodParameterInjectionPoint>().ToArray();
            var ctorInject = InjectMethods(paramInject.Where(x => x.IsConstructor)).FirstOrDefault();
            var methodInject = InjectMethods(paramInject.Where(x=> !x.IsConstructor)).ToArray();
            var otherInjects = InjectionPoints.Except(paramInject).Cast<IWeldInjetionPoint>();

            var create = ctorInject == null? 
                new BuildPlan(() => Activator.CreateInstance(Type, true)): 
                () => ctorInject(null);

            return () =>
            {
                var instance = create();
                foreach (var i in otherInjects)
                    i.Inject(instance);
                foreach (var i in methodInject)
                    i(instance);
                foreach (var post in _postConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        private IEnumerable<InjectPlan> InjectMethods(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            return from g in injects.GroupBy(x => x.Member) 
                let method = (MethodBase)g.Key 
                let paramInjects = g.OrderBy(x => x.Position).ToArray() 
                select (InjectPlan) (x =>
                {
                    var paramVals = paramInjects.Select(p => p.GetValue()).ToArray();
                    return method.Invoke(x, paramVals);
                });
        }

        public override string ToString()
        {
            return string.Format("Component [{0}] with Qualifiers [{1}]", Type, string.Join(",", Qualifiers));
        }
    }
}