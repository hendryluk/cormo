using System;
using System.Collections.Generic;
using System.Linq;
using Cormo.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public abstract class AbstractComponent : IWeldComponent
    {
        private AbstractComponent(Type type, IEnumerable<QualifierAttribute> qualifiers,
            Type scope, WeldComponentManager manager)
        {
            var qualifierSet = new HashSet<QualifierAttribute>(qualifiers);
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
            var qualifierTypes = qualifiers.Select(x => x.GetType()).ToArray();
            if (qualifierTypes.Contains(typeof (AnyAttribute)))
                return true;

            return qualifierTypes.All(Qualifiers.Select(x=> x.GetType()).Contains);
        }

        public abstract IWeldComponent Resolve(Type requestedType);

        public object Create(ICreationalContext context, IInjectionPoint injectionPoint)
        {
            return _lazyBuildPlan.Value(context, injectionPoint);
        }

        public abstract void Destroy(object instance, ICreationalContext creationalContext);

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