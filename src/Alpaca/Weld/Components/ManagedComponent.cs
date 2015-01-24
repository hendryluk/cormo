using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Components
{
    public abstract class ManagedComponent : AbstractComponent
    {
        public IEnumerable<MethodInfo> PostConstructs { get; private set; }
        protected readonly bool ContainsGenericParameters;

        protected ManagedComponent(ComponentIdentifier id, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs)
            : base(id, type, qualifiers, scope, manager)
        {
            PostConstructs = postConstructs;
            ContainsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected ManagedComponent(Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(type.FullName, type, qualifiers, scope, manager)
        {
            PostConstructs = postConstructs;
            ContainsGenericParameters = Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected override BuildPlan GetBuildPlan()
        {
            var paramInject = InjectionPoints.OfType<MethodParameterInjectionPoint>().ToArray();
            var constructPlan = MakeConstructPlan(paramInject.Where(x => x.IsConstructor));
            var methodInject = InjectMethods(paramInject.Where(x => !x.IsConstructor)).ToArray();
            var otherInjects = InjectionPoints.Except(paramInject).Cast<IWeldInjetionPoint>();

            //var create = ctorInject == null? 
            //    new BuildPlan(context => Activator.CreateInstance(Type, true)): 
            //    context => ctorInject(null, context);

            return context =>
            {
                var instance = constructPlan(context);
                foreach (var i in otherInjects)
                    i.Inject(instance, context);
                foreach (var i in methodInject)
                    i(instance, context);
                foreach (var post in PostConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        protected abstract BuildPlan MakeConstructPlan(IEnumerable<MethodParameterInjectionPoint> injects);

        private IEnumerable<InjectPlan> InjectMethods(IEnumerable<MethodParameterInjectionPoint> injects)
        {
            return from g in injects.GroupBy(x => x.Member)
                let method = (MethodInfo)g.Key
                let paramInjects = g.OrderBy(x => x.Position).ToArray()
                select (InjectPlan)((target, context) =>
                {
                    var paramVals = paramInjects.Select(p => p.GetValue(context)).ToArray();
                    return method.Invoke(target, paramVals);
                });
        }

        private void ValidateMethodSignatures()
        {
            foreach (var m in PostConstructs)
            {
                PostConstructCriteria.Validate(m);
            }
        }

        public override bool IsConcrete
        {
            get { return !ContainsGenericParameters; }
        }

        public bool IsDisposable { get; private set; }
    }
}