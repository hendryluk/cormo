using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Introspectors;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public abstract class ManagedComponent : AbstractComponent
    {
        private readonly bool _isConcrete;
        public IEnumerable<MethodInfo> PostConstructs { get; private set; }
        private readonly InjectableConstructor _injectableConstructor;

        protected ManagedComponent(ComponentIdentifier id, ConstructorInfo ctor, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs)
            : base(id, ctor.DeclaringType, binders, scope, manager)
        {
            _injectableConstructor = new InjectableConstructor(this, ctor);
            
            PostConstructs = postConstructs;
            _isConcrete = !Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected ManagedComponent(ConstructorInfo ctor, IBinders binders, Type scope, WeldComponentManager manager, MethodInfo[] postConstructs) 
            : base(ctor.DeclaringType.FullName, ctor.DeclaringType, binders, scope, manager)
        {
            _injectableConstructor = new InjectableConstructor(this, ctor);
            
            PostConstructs = postConstructs;
            _isConcrete = !Type.ContainsGenericParameters;
            IsDisposable = typeof(IDisposable).IsAssignableFrom(Type);

            ValidateMethodSignatures();
        }

        protected InjectableConstructor InjectableConstructor { get { return _injectableConstructor; } }

        public override void Touch()
        {
            RuntimeHelpers.RunClassConstructor(Type.TypeHandle);
        }

        public override void Destroy(object instance, ICreationalContext creationalContext)
        {
            try 
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
                
                // WELD-1010 hack?
                var context = creationalContext as IWeldCreationalContext;
                if (context != null) {
                    context.Release(this, instance);
                } else {
                    creationalContext.Release();
                }
            } 
            catch (Exception e) {
                // TODO log.error(ERROR_DESTROYING, this, instance);
                // TODO xLog.throwing(Level.DEBUG, e);
                throw;
            }
        }

        private readonly ISet<IWeldInjetionPoint> _memberInjectionPoints = new HashSet<IWeldInjetionPoint>();
        private readonly ISet<InjectableMethod> _injectableMethods = new HashSet<InjectableMethod>();
        
        protected IEnumerable<InjectableMethodBase> InjectableMethods
        {
            get { return _injectableMethods; }
        }

        public void AddMemberInjectionPoints(params IWeldInjetionPoint[] injectionPoints)
        {
            foreach (var inject in injectionPoints)
            {
                _memberInjectionPoints.Add(inject);
            }
        }

        public void AddInjectableMethods(IEnumerable<InjectableMethod> methods)
        {
            foreach (var method in methods)
            {
                _injectableMethods.Add(method);
            }
        }

        protected void TransferInjectionPointsTo(ManagedComponent component, GenericResolver.Resolution resolution)
        {
            component.AddMemberInjectionPoints(_memberInjectionPoints.Select(x => 
                x.TranslateGenericArguments(component, resolution.GenericParameterTranslations))
                .ToArray());

            component.AddInjectableMethods(_injectableMethods.Select(m=> m.TranslateGenericArguments(component, resolution.GenericParameterTranslations))
                .Cast<InjectableMethod>().ToArray());
        }

        protected override BuildPlan GetBuildPlan()
        {
            var constructPlan = MakeConstructPlan();
            return context =>
            {
                var instance = constructPlan(context);
                context.Push(instance);

                foreach (var i in _memberInjectionPoints)
                    i.Inject(instance, context);
                foreach (var injectableMethod in _injectableMethods)
                    injectableMethod.InvokeWithInstance(instance, context);
                foreach (var post in PostConstructs)
                    post.Invoke(instance, new object[0]);

                return instance;
            };
        }

        protected abstract BuildPlan MakeConstructPlan();

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

        public bool IsConcrete { get { return _isConcrete; } }

        public bool IsDisposable { get; private set; }

        public override IEnumerable<IChainValidatable> NextLinearValidatables
        {
            get
            {
                if (!IsConcrete)
                    return new IChainValidatable[0];

                return InjectableConstructor.LinearValidatables;
            }
        }

        public override IEnumerable<IChainValidatable> NextNonLinearValidatables
        {
            get
            {
                if (!IsConcrete)
                    return new IChainValidatable[0];

                return InjectableConstructor.LinearValidatables.Union(
                    _memberInjectionPoints.Select(x => x.Component).OfType<IWeldComponent>());
            }
        }
    }
}