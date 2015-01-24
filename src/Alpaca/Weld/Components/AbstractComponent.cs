using System;
using System.Collections.Generic;
using System.Linq;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects;
using Alpaca.Weld.Injections;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Components
{
    public abstract class AbstractComponent : IWeldComponent
    {
        private AbstractComponent(Type type, IEnumerable<QualifierAttribute> qualifiers,
            Type scope, WeldComponentManager manager)
        {
            var qualifierSet = new HashSet<QualifierAttribute>(qualifiers);
            if (!qualifierSet.OfType<AnyAttribute>().Any())
                qualifierSet.Add(AnyAttribute.Instance);
            if (qualifierSet.All(x => (x is AnyAttribute)))
                qualifierSet.Add(DefaultAttribute.Instance);

            Type = type;
            Manager = manager;
            Qualifiers = qualifierSet;
            Scope = scope;
            _lazyBuildPlan = new Lazy<BuildPlan>(GetBuildPlan);
        }

        protected AbstractComponent(ComponentIdentifier id, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope,
            WeldComponentManager manager)
            : this(type, qualifiers, scope, manager)
        {
            _id = id;
        }

        protected AbstractComponent(string idSuffix, Type type, IEnumerable<QualifierAttribute> qualifiers, Type scope, WeldComponentManager manager)
            : this(type, qualifiers, scope, manager)
        {
            _id = new ComponentIdentifier(string.Format("{0}-{1}-{2}", manager.Id, GetType().Name, idSuffix));
        }

        public IEnumerable<QualifierAttribute> Qualifiers { get; set; }
        public Type Scope { get; private set; }
        public Type Type { get; set; }

        IComponentManager IComponent.Manager
        {
            get { return Manager; }
        }

        public WeldComponentManager Manager { get; set; }
        public abstract bool IsConcrete { get; }
        public IEnumerable<IInjectionPoint> InjectionPoints
        {
            get { return _injectionPoints; }
        }

        public void AddInjectionPoints(params IWeldInjetionPoint[] injectionPoints)
        {
            foreach(var inject in injectionPoints)
                _injectionPoints.Add(inject);
        }

        private readonly Lazy<BuildPlan> _lazyBuildPlan;
        private readonly ISet<IWeldInjetionPoint> _injectionPoints = new HashSet<IWeldInjetionPoint>();
        private ComponentIdentifier _id;

        public bool IsProxyRequired { get; private set; }

        public bool CanSatisfy(IEnumerable<QualifierAttribute> qualifiers)
        {
            return qualifiers.All(Qualifiers.Contains);
        }

        public abstract IWeldComponent Resolve(Type requestedType);

        public object Create(ICreationalContext context)
        {
            return _lazyBuildPlan.Value(context);
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        protected void TransferInjectionPointsTo(AbstractComponent component, GenericUtils.Resolution resolution)
        {
            component.AddInjectionPoints(_injectionPoints.Select(x =>
            {
                if (x.DeclaringComponent.Type == component.Type)
                    return x;
                return x.TranslateGenericArguments(component, resolution.GenericParameterTranslations);
            }).ToArray());
        }

        protected abstract BuildPlan GetBuildPlan();
        public ComponentIdentifier Id { get { return _id; } }
    }
}