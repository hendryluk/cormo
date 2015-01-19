using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Injection
{
    public abstract class AbstractInjectionPoint : IWeldInjetionPoint
    {
        protected readonly bool IsCacheable;
        
        protected AbstractInjectionPoint(IComponent declaringComponent, MemberInfo member, Type type, QualifierAttribute[] qualifiers)
        {
            qualifiers = qualifiers.DefaultIfEmpty(DefaultAttribute.Instance).ToArray();

            DeclaringComponent = declaringComponent;
            Member = member;
            ComponentType = type;
            Qualifiers = qualifiers;
            IsCacheable = IsCacheableType(type);
            _lazyComponents = new Lazy<IComponent>(ResolveComponents);
            _lazyInjectPlan = new Lazy<InjectPlan>(()=> BuildInjectPlan(Component));
        }

        private static bool IsCacheableType(Type type)
        {
            return !typeof(IInjectionPoint).IsAssignableFrom(type) && !typeof(IInstance<>).IsAssignableFrom(GenericUtils.OpenIfGeneric(type));
        }

        public MemberInfo Member { get; private set; }
        public IComponent DeclaringComponent { get; private set; }
        public Type ComponentType { get; set; }
        public IEnumerable<QualifierAttribute> Qualifiers { get; set; }
        public abstract IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        protected abstract InjectPlan BuildInjectPlan(IComponent components);
        private readonly Lazy<InjectPlan> _lazyInjectPlan;
        private readonly Lazy<IComponent> _lazyComponents;

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

        public void Inject(object target)
        {
            _lazyInjectPlan.Value(target);
        }
    }
}