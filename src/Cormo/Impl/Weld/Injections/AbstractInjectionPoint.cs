using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Utils;

namespace Cormo.Impl.Weld.Injections
{
    public abstract class AbstractInjectionPoint : IWeldInjetionPoint
    {
        protected readonly bool IsCacheable;
        
        protected AbstractInjectionPoint(IComponent declaringComponent, MemberInfo member, Type type, IBinders binders)
        {
            DeclaringComponent = declaringComponent;
            Member = member;
            ComponentType = type;
            Binders = binders;
            IsCacheable = IsCacheableType(type);
            Unwraps = binders.OfType<UnwrapAttribute>().Any();
            _lazyComponents = new Lazy<IComponent>(ResolveComponents);
            _lazyInjectPlan = new Lazy<InjectPlan>(()=> BuildInjectPlan(Component));
            _lazyGetValuePlan = new Lazy<BuildPlan>(BuildGetValuePlan);
        }

        public IBinders Binders { get; private set; }

        private static bool IsCacheableType(Type type)
        {
            return typeof(IInstance<>).IsAssignableFrom(GenericUtils.OpenIfGeneric(type));
        }

        private BuildPlan BuildGetValuePlan()
        {
            var manager = DeclaringComponent.Manager;
            var component = Component;
            if (IsCacheable || component.IsProxyRequired)
            {
                return CacheUtils.Cache(context => manager.GetInjectableReference(this, context));
            }

            return context => manager.GetInjectableReference(this, context);
        }

        public MemberInfo Member { get; private set; }
        public IComponent DeclaringComponent { get; private set; }
        public Type ComponentType { get; set; }
        public IQualifiers Qualifiers { get { return Binders.Qualifiers; } }
        public abstract IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        protected abstract InjectPlan BuildInjectPlan(IComponent components);
        private readonly Lazy<InjectPlan> _lazyInjectPlan;
        private readonly Lazy<IComponent> _lazyComponents;
        private readonly Lazy<BuildPlan> _lazyGetValuePlan;

        public object GetValue(ICreationalContext context)
        {
            return _lazyGetValuePlan.Value(context);
        }

        private IComponent ResolveComponents()
        {
            return DeclaringComponent.Manager.GetComponent(this);
        }

        public IComponent Component
        {
            get { return _lazyComponents.Value; }    
        }

        public Type Scope
        {
            get { return Component.Scope; }
        }

        public bool Unwraps { get; private set; }

        public void Inject(object target, ICreationalContext context)
        {
            _lazyInjectPlan.Value(target, context);
        }
    }
}