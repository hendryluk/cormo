using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Weld;
using Alpaca.Weld.Utils;

namespace Alpaca.Injects
{
    public class DependentAttribute : ScopeAttribute
    {
        
    }

    public interface IInjectionPoint
    {
        IComponent DeclaringComponent { get; }
        MemberInfo Member { get; }
        Type ComponentType { get; }
        IEnumerable<QualifierAttribute> Qualifiers { get; }
    }

    public interface IInstance<T>
    {

    }

    public interface IComponentManager
    {
        IComponent GetComponent(IInjectionPoint injectionPoint);
        object GetReference(IComponent component);
        object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component);
    }
}

namespace Alpaca.Weld
{
    public interface IWeldInjetionPoint : IInjectionPoint
    {
        IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        void Inject(object target);
        IComponent Component { get; }
    }

    public abstract class AbstractInjectionPoint : IWeldInjetionPoint
    {
        protected readonly bool IsCacheable;
        
        protected AbstractInjectionPoint(IComponent declaringComponent, MemberInfo member, Type type, QualifierAttribute[] qualifiers)
        {
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

        public void Inject(object target)
        {
            _lazyInjectPlan.Value(target);
        }
    }

    public class MethodParameterInjectionPoint : AbstractInjectionPoint
    {
        private readonly ParameterInfo _param;
        private readonly Lazy<BuildPlan> _lazyGetValuePlan = new Lazy<BuildPlan>(); 
        public MethodParameterInjectionPoint(IComponent declaringComponent, ParameterInfo paramInfo, QualifierAttribute[] qualifiers) 
            : base(declaringComponent, paramInfo.Member, paramInfo.ParameterType, qualifiers)
        {
            _param = paramInfo;
            IsConstructor = _param.Member is ConstructorInfo;
            _lazyGetValuePlan = new Lazy<BuildPlan>(BuildGetValuePlan);
        }

        private BuildPlan BuildGetValuePlan()
        {
            var manager = DeclaringComponent.Manager;
            var component = Component;
            if (IsCacheable)
            {
                var instance = manager.GetReference(component);
                return () => instance;
            }

            return () => manager.GetReference(component);
        }

        public bool IsConstructor { get; private set; }
        public int Position { get { return _param.Position; } }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            if (IsConstructor)
            {
                var ctor = (ConstructorInfo) _param.Member;
                ctor = GenericUtils.TranslateConstructorGenericArguments(ctor, translations);
                var param = ctor.GetParameters()[_param.Position];
                return new MethodParameterInjectionPoint(component, param, Qualifiers.ToArray());
            }
            else
            {
                var method = (MethodInfo)_param.Member;
                method = GenericUtils.TranslateMethodGenericArguments(method, translations);
                var param = method.GetParameters()[_param.Position];
                return new MethodParameterInjectionPoint(component, param, Qualifiers.ToArray());
            }
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            throw new NotSupportedException();
        }

        public object GetValue()
        {
            return _lazyGetValuePlan.Value();
        }

        public override string ToString()
        {
            // TODO prettify
            return _param.ToString();
        }
    }

    public class FieldInjectionPoint : AbstractInjectionPoint
    {
        private readonly FieldInfo _field;

        public FieldInjectionPoint(IComponent declaringComponent, FieldInfo field, QualifierAttribute[] qualifiers) :
            base(declaringComponent, field, field.FieldType, qualifiers)
        {
            InjectionValidator.Validate(field);
            _field = field;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var field = GenericUtils.TranslateFieldType(_field, translations);
            return new FieldInjectionPoint(component, field, Qualifiers.ToArray());
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            var manager = DeclaringComponent.Manager;
            if (IsCacheable)
            {
                var instance = manager.GetReference(component);
                return target => SetValue(target, instance);
            }

            return target =>
            {
                var instance = manager.GetReference(component);
                return SetValue(target, instance);
            };
        }

        private object SetValue(object target, object instance)
        {
            _field.SetValue(target, instance);
            return instance;
        }

        public override string ToString()
        {
            // TODO prettify
            return _field.ToString();
        }
    }

    public class PropertyInjectionPoint: AbstractInjectionPoint
    {
        private readonly PropertyInfo _property;

        public PropertyInjectionPoint(IComponent declaringComponent, PropertyInfo property, QualifierAttribute[] qualifiers):
            base(declaringComponent, property, property.PropertyType, qualifiers)
        {
            InjectionValidator.Validate(property);
            _property = property;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var property = GenericUtils.TranslatePropertyType(_property, translations);
            return new PropertyInjectionPoint(component, property, Qualifiers.ToArray());
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            var manager = DeclaringComponent.Manager;
            if (IsCacheable)
            {
                var instance = manager.GetReference(component);
                return target => SetValue(target, instance);
            }

            return target =>
            {
                var instance = manager.GetReference(component);
                return SetValue(target, instance);
            };
        }

        private object SetValue(object target, object instance)
        {
            _property.SetValue(target, instance);
            return instance;
        }

        public override string ToString()
        {
            // TODO prettify
            return _property.ToString();
        }
    }

    public class AttributeScanDeployer
    {
        private readonly WeldComponentManager _manager;
        private readonly WeldEnvironment _environment;
        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public AttributeScanDeployer(WeldComponentManager manager, WeldEnvironment environment)
        {
            _manager = manager;
            _environment = environment;
        }

        public void AutoScan()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var types = (from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                where assembly.GetReferencedAssemblies().Any(x=> AssemblyName.ReferenceMatchesDefinition(x, assemblyName))
                from type in assembly.GetLoadableTypes()
                where type.IsPublic && type.IsClass && !type.IsPrimitive
                select type).ToArray();

            //var classComponents = .ToArray();

            //var configurations = types.AsParallel().Where(ConfigurationCriteria.ScanPredicate).ToArray();

            var componentTypes = types.AsParallel().Where(TypeUtils.IsComponent).ToArray();
            var producesFields = (from type in types.AsParallel()
                from field in type.GetFields(AllBindingFlags)
                where field.HasAttribute<ProducesAttribute>()
                select field).ToArray();

            var producesMethods = (from type in types.AsParallel()
                from method in type.GetMethods(AllBindingFlags)
                where method.HasAttribute<ProducesAttribute>()
                select method).ToArray();

            var producesProperties = (from type in types.AsParallel()
                from property in type.GetProperties(AllBindingFlags)
                where property.HasAttribute<ProducesAttribute>()
                select property).ToArray();

            AddTypes(componentTypes);
        }

        public void AddTypes(Type[] types)
        {
            var components = types.AsParallel().Select(MakeComponent).ToArray();

            foreach (var c in components)
            {
                _environment.AddComponent(c);
                if (c.Type.HasAttribute<ConfigurationAttribute>())
                    _environment.AddConfiguration(c);
            }
        }

        public IWeldComponent AddType(Type type)
        {
            var component = MakeComponent(type);
            _environment.AddComponent(component);
            
            if (type.HasAttribute<ConfigurationAttribute>())
                _environment.AddConfiguration(component);    
            
            return component;
        }

        public IWeldComponent MakeComponent(Type type)
        {
            var methods = type.GetMethods(AllBindingFlags).ToArray();

            var iMethods = methods.Where(InjectionValidator.ScanPredicate).ToArray();
            var iProperties = type.GetProperties(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var iCtors = type.GetConstructors(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var iFields = type.GetFields(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var postConstructs = methods.Where(x => x.HasAttribute<PostConstructAttribute>()).ToArray();
            var preDestroys = methods.Where(x => x.HasAttribute<PreDestroyAttribute>()).ToArray();
            var scopes = type.GetRecursiveAttributes<ScopeAttribute>().ToArray();

            if (iCtors.Length > 1)
                throw new InvalidComponentException(type, "Multiple [Inject] constructors");

            var scope = scopes.FirstOrDefault() ?? new DependentAttribute();
            var component = new ClassComponent(type, type.GetQualifiers(), scope, _manager, postConstructs, preDestroys);
            var methodInjects = iMethods.SelectMany(m => ToMethodInjections(component, m)).ToArray();
            var ctorInjects = iCtors.SelectMany(ctor => ToMethodInjections(component, ctor)).ToArray();
            var fieldInjects = iFields.Select(f => new FieldInjectionPoint(component, f, f.GetQualifiers())).ToArray();
            var propertyInjects = iProperties.Select(p => new PropertyInjectionPoint(component, p, p.GetQualifiers())).ToArray();

            foreach (var inject in methodInjects.Union(ctorInjects).Union(fieldInjects).Union(propertyInjects))
                component.AddInjectionPoints(inject);

            return component;
        }

        private IEnumerable<IWeldInjetionPoint> ToMethodInjections(IComponent component, MethodBase method)
        {
            var parameters = method.GetParameters();
            return parameters.Select(p => new MethodParameterInjectionPoint(component, p, p.GetQualifiers()));
        }

        public void Deploy()
        {
            _manager.Deploy(_environment);
        }
    }

    public class WeldComponentManager : IComponentManager
    {
        private ConcurrentBag<IWeldComponent> _unresolvedComponents;
        private ConcurrentBag<IWeldComponent> _allComponents;
        private readonly ConcurrentDictionary<Type, IWeldComponent[]> _typeComponents = new ConcurrentDictionary<Type, IWeldComponent[]>();

        private IEnumerable<IWeldComponent> GetComponentsForType(Type type)
        {
            return _typeComponents.GetOrAdd(type, t => 
                _allComponents.Select(x => x.Resolve(t)).Where(x => x != null).ToArray());
        }

        public IComponent GetComponent(IInjectionPoint injectionPoint)
        {
            var unwrappedType = UnwrapType(injectionPoint.ComponentType);
            var isWrapped = unwrappedType != injectionPoint.ComponentType;

            var resolved = GetComponentsForType(unwrappedType).Where(x=> x.CanSatisfy(injectionPoint.Qualifiers)).ToArray();
            if (!isWrapped)
            {
                if (resolved.Length > 1)
                {
                    throw new AmbiguousResolutionException(injectionPoint, resolved.Cast<IComponent>().ToArray());
                }
                if (!resolved.Any())
                {
                    throw new UnsatisfiedDependencyException(injectionPoint);
                }

                return resolved.Single();
            }

            return resolved.Single();
        }

        public object GetReference(IComponent component)
        {
            return ((IWeldComponent) component).Build();
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component)
        {
            return GetReference(component);
        }

        public void Deploy(WeldEnvironment environment)
        {
            _allComponents = new ConcurrentBag<IWeldComponent>(environment.Components);
            _unresolvedComponents = new ConcurrentBag<IWeldComponent>(environment.Components.Where(x=> x.IsConcrete));
            
            while (_unresolvedComponents.Any())
                ResolveComponents();

            ResolveConfigurations(environment);
        }

        private void ResolveConfigurations(WeldEnvironment environment)
        {
            foreach (var config in environment.Configurations)
            {
                GetReference(config);
            }
        }

        private void ResolveComponents()
        {
            var toBeResolved = _unresolvedComponents;
            _unresolvedComponents = new ConcurrentBag<IWeldComponent>();
            foreach (var component in toBeResolved)
            {
                var _ = component.InjectionPoints.OfType<IWeldInjetionPoint>().Select(x => x.Component).ToArray();
            }
        }

        public bool IsWrappedType(Type type)
        {
            return type.IsGenericType && typeof (IInstance<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public Type UnwrapType(Type type)
        {
            return IsWrappedType(type) ? type.GetGenericArguments()[0] : type;
        }

        public bool IsProxyRequired(IComponent component)
        {
            return IsNormalScope(component.Scope);
        }

        private bool IsNormalScope(Attribute scope)
        {
            return scope is NormalScopeAttribute;
        }
    }

    public class ComponentResolver
    {

    }
}