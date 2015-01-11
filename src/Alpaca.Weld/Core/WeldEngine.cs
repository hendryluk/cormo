using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Alpaca.Weld.Attributes;
using Alpaca.Weld.Utils;
using Castle.Components.DictionaryAdapter.Xml;

namespace Alpaca.Weld.Core
{
    public class Scanner
    {
        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
       
        private Type[] _types;
        private Type[] _configurations;
        private IEnumerable<MethodInfo> _producesFields;
        private IEnumerable<MethodInfo> _producesMethods;
        private IEnumerable<PropertyInfo> _producesProperties;
        private IEnumerable<PropertyInfo> _propertyInjects;
        private MemberInfo[] _injects;
        private IEnumerable<Type> _components;

        public void AutoScan()
        {
            _types = (from assembly in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                         from type in assembly.GetLoadableTypes()
                         where !type.IsEnum && !type.IsPrimitive
                         select type).ToArray();

            _components = _types.Where(TypeUtils.IsComponent);
             
            _configurations = _types.Where(ConfigurationCriteria.ScanPredicate).ToArray();

            _injects = (from type in _types
                        where !type.IsInterface && !type.IsAbstract
                        let methods = type.GetMethods(AllBindingFlags)
                        let properties = type.GetProperties(AllBindingFlags).Where(x => x.SetMethod == null || !x.SetMethod.IsAbstract)
                        let ctors = type.GetConstructors(AllBindingFlags)
                        let fields = type.GetFields(AllBindingFlags)
                        from member in methods.Cast<MemberInfo>().Union(properties).Union(ctors).Union(fields)
                        where InjectionCriteria.ScanPredicate(member)
                        select member).ToArray();

            _producesFields = (from type in _types
                               from field in type.GetMethods(AllBindingFlags)
                               where field.HasAttribute<ProducesAttribute>()
                               select field);

            _producesMethods = (from type in _types
                                from method in type.GetMethods(AllBindingFlags)
                                where method.HasAttribute<ProducesAttribute>()
                                select method);

            _producesProperties = (from type in _types
                                from property in type.GetProperties(AllBindingFlags)
                                where property.HasAttribute<ProducesAttribute>()
                                select property);
        }

        public WeldCatalog BuildCatalog()
        {
            var catalog = new WeldCatalog();
            catalog.RegisterConfigurations(_configurations);
            
            foreach (var component in _components)
                catalog.RegisterComponent(component, GetQualifiers(component));

            foreach (var field in _injects.OfType<FieldInfo>())
                catalog.RegisterInject(field, GetQualifiers(field));
            foreach (var method in _injects.OfType<MethodInfo>())
                foreach(var param in method.GetParameters())
                catalog.RegisterInject(param, GetQualifiers(param));
            foreach (var property in _injects.OfType<PropertyInfo>())
                catalog.RegisterInject(property, GetQualifiers(property));

            return catalog;
        }

        private static object[] GetQualifiers(ICustomAttributeProvider attributeProvider)
        {
            return (from attribute in attributeProvider.GetAttributes()
                where attribute.GetType().HasAttribute<QualifierAttribute>()
                select attribute).Cast<object>().ToArray();
        }
    }

    public class WeldCatalog
    {
        private readonly List<ComponentRegistration> _components = new List<ComponentRegistration>();
        private readonly List<Type> _configurations = new List<Type>();
        private readonly List<InjectionPoint> _injectionPoints = new List<InjectionPoint>();

        private static readonly DefaultAttribute DefaultAttributeInstance = new DefaultAttribute();
        private static readonly AnyAttribute AnyAttributeInstance = new AnyAttribute();
        public void RegisterComponent(Type component, object[] qualifiers)
        {
            ComponentCriteria.Validate(component);

            var qualifierSet = new HashSet<object>(qualifiers);
            if (qualifierSet.All(x => (x is AnyAttribute)))
            {
                qualifierSet.Add(DefaultAttributeInstance);
            }
            qualifierSet.Add(AnyAttributeInstance);

            _components.Add(new ClassComponentRegistration
            {
                Component = component,
                Qualifiers = qualifierSet,
            });
        }

        public void RegisterConfigurations(params Type[] configurations)
        {
            foreach (var config in configurations)
            {
                ConfigurationCriteria.Validate(config);
            }
            _configurations.AddRange(configurations);
        }

        private object[] SetQualifierDefaults(object[] qualifiers)
        {
            if (!qualifiers.Any())
                return new object[] { DefaultAttributeInstance };

            return qualifiers;
        }

        public void RegisterInject(FieldInfo field, object[] qualifiers)
        {
            InjectionCriteria.Validate(field);
            qualifiers = SetQualifierDefaults(qualifiers);
            _injectionPoints.Add(new InjectionPoint(field.FieldType, field, qualifiers));
        }

        public void RegisterInject(ParameterInfo parameter, object[] qualifiers)
        {
            InjectionCriteria.Validate((MethodBase)parameter.Member);
            qualifiers = SetQualifierDefaults(qualifiers);
            _injectionPoints.Add(new InjectionPoint(parameter.ParameterType, parameter.Member, qualifiers){Index = parameter.Position});
        }

        public void RegisterInject(PropertyInfo property, object[] qualifiers)
        {
            InjectionCriteria.Validate(property);
            qualifiers = SetQualifierDefaults(qualifiers);
            _injectionPoints.Add(new InjectionPoint(property.PropertyType, property, qualifiers));
        }

        public IEnumerable<Type> Configurations
        {
            get { return _configurations; }
        }

        public IEnumerable<ComponentRegistration> Components
        {
            get { return _components; }
        }

        public IEnumerable<InjectionPoint> InjectionPoints
        {
            get { return _injectionPoints;  }
        }
    }

    public abstract class ComponentRegistration
    {
        public Type Component { get; set; }
        public HashSet<object> Qualifiers { get; set; }
        
        public virtual bool? CanSatisfy(LookupSpec spec)
        {
            return spec.Qualifiers.All(Qualifiers.Contains)? CanSatisfy(spec.Type): false;
        }

        protected abstract bool? CanSatisfy(Type requestedType);
        public abstract IComponentFactory GetFactory(WeldEngine engine, LookupSpec spec);
    }

    public class ClassComponentRegistration : ComponentRegistration
    {
        protected override bool? CanSatisfy(Type requestedType)
        {
            var resolution = ResolveType(requestedType);
            if (resolution == null)
                return false;
            if (resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;
            return true;
        }

        private GenericUtils.Resolution ResolveType(Type requestedType)
        {
            return GenericUtils.ResolveGenericType(Component, requestedType);
        }

        public override IComponentFactory GetFactory(WeldEngine engine, LookupSpec spec)
        {
            var resolution = ResolveType(spec.Type);
            if(resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
                return null;

            return new ActivatorComponentFactory(engine, resolution.ResolvedType);
        }
    }

    public class ProducerRegistration: ComponentRegistration
    {
        public MemberInfo Producer { get; set; }

        protected override bool? CanSatisfy(Type requestedType)
        {
            var producer = ResolveProducer(requestedType);
            if (producer == null)
                return false;
            if (GenericUtils.MemberContainsGenericArguments(producer))
                return null;
            return true;
        }

        private MemberInfo ResolveProducer(Type requestedType)
        {
            var typeResolution = GenericUtils.ResolveGenericType(Component, requestedType);
            if (typeResolution == null)
                return null;
                
            return GenericUtils.TranslateMemberGenericArguments(Producer, typeResolution.GenericParameterTranslations);
        }

        public override IComponentFactory GetFactory(WeldEngine engine, LookupSpec spec)
        {
            var resolvedProducer = ResolveProducer(spec.Type);
            if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
                return null;

            var method = resolvedProducer as MethodInfo;
            if (method != null)
                return new ProducerMethodComponentFactory(engine, method);

            return null;
        }
    }

    public interface IComponentFactory
    {
        object CreateComponent(Type requestedType);
        bool GuarantesResult { get; }
    }

    public class ProducerMethodComponentFactory : IComponentFactory
    {
        private readonly WeldEngine _engine;
        private readonly MethodInfo _producer;

        public ProducerMethodComponentFactory(WeldEngine engine, MethodInfo producer)
        {
            _engine = engine;
            _producer = producer;
        }

        public object CreateComponent(Type requestedType)
        {
            throw new NotImplementedException();
            //return engine.Execute(_producer);
        }

        public bool GuarantesResult { get { return true; } }
    }

    public class ActivatorComponentFactory: IComponentFactory
    {
        private readonly WeldEngine _engine;
        private readonly Type _type;
        private Lazy<DependencyInjector[]> _dependencies;
        private Func<object> _constructor;

        public ActivatorComponentFactory(WeldEngine engine, Type type)
        {
            _engine = engine;
            _type = type;
            _dependencies = new Lazy<DependencyInjector[]>(LoadDependencies);
        }

        private DependencyInjector[] LoadDependencies()
        {
            var dependencies = _engine.GetDependenciesOf(_type);
            var constructors = dependencies.Where(x => x.IsConstructor).ToArray();

            if (constructors.Length > 1)
            {
                throw new InvalidComponentException(_type, "Multiple [Inject] constructors");
            }
            if (constructors.Length == 1)
            {
                var ctr = constructors[0];
                _constructor = () => ctr.Inject(null);
            }
            else
            {
                _constructor = () => Activator.CreateInstance(_type, true);
            }

            return dependencies.Where(x => !x.IsConstructor).ToArray();
        }

        public object CreateComponent(Type requestedType)
        {
            var dependencies = _dependencies.Value;
            var obj = _constructor();
            _engine.InjectDependencies(obj, dependencies);
            return obj;
        }

        public bool GuarantesResult { get { return true; } }
    }

    public struct LookupSpec
    {
        public LookupSpec(Type type, IEnumerable<object> qualifiers): this()
        {
            Type = type;
            Qualifiers = qualifiers;
        }

        public Type Type { get; private set; }
        public IEnumerable<object> Qualifiers { get; private set; }
    }

    public static class MemberVisitor
    {
        public static T VisitInject<T>(MemberInfo member, 
            Func<MethodBase, T> onMethod,
            Func<FieldInfo, T> onField,
            Func<PropertyInfo, T> onProperty)
        {
            var method = member as MethodBase;
            if (method != null)
                onMethod(method);
            var field = member as FieldInfo;
            if (field != null)
                onField(field);
            var property = member as PropertyInfo;
            if (property != null)
                onProperty(property);

            return default(T);
        }
    }

    public abstract class DependencyInjector
    {
        protected readonly WeldEngine Engine;
        private readonly IDictionary<int, ComponentRegistration[]> _registrations;

        protected DependencyInjector(WeldEngine engine, IDictionary<int, ComponentRegistration[]> registrations)
        {
            Engine = engine;
            _registrations = registrations;
        }

        protected virtual object GetDependency()
        {
            return Engine.GetInstance(_registrations[0]);
        }

        public abstract object Inject(object target);
        public abstract bool IsConstructor { get; }
        
        public static DependencyInjector Create(WeldEngine engine, MemberInfo member, IDictionary<int, ComponentRegistration[]> registrations)
        {
            return MemberVisitor.VisitInject<DependencyInjector>(member,
                method => new ToMethod(engine, registrations, method), 
                field => new ToField(engine, registrations, field),
                property=> new ToProperty(engine, registrations, property));
        }

        public class ToMethod : DependencyInjector
        {
            private readonly MethodBase _method;

            public ToMethod(WeldEngine engine, IDictionary<int, ComponentRegistration[]> registrations, MethodBase method)
                : base(engine, registrations)
            {
                _method = method;
            }

            public override object Inject(object target)
            {
                return Engine.Execute(target, _method, _registrations);
            }

            public override bool IsConstructor
            {
                get { return _method is ConstructorInfo; }
            }
        }

        public class ToField : DependencyInjector
        {
            private readonly FieldInfo _field;

            public ToField(WeldEngine engine, IDictionary<int, ComponentRegistration[]> registrations, FieldInfo field)
                : base(engine, registrations)
            {
                _field = field;
            }

            public override object Inject(object target)
            {
                var value = GetDependency();
                _field.SetValue(target, value);
                return value;
            }

            public override bool IsConstructor
            {
                get { return false; }
            }
        }

        public class ToProperty : DependencyInjector
        {
            private readonly PropertyInfo _property;

            public ToProperty(WeldEngine engine, IDictionary<int, ComponentRegistration[]> registrations, PropertyInfo property)
                : base(engine, registrations)
            {
                _property = property;
            }

            public override object Inject(object target)
            {
                var value = GetDependency();
                _property.SetValue(target, value); 
                return value;
            }

            public override bool IsConstructor
            {
                get { return false; }
            }
        }
    }

    public class WeldEngine
    {
        private readonly WeldCatalog _catalog;
        private Dictionary<Type, DependencyInjector[]> _typeDependencies;
        private Dictionary<ComponentRegistration, object> _componentValues = new Dictionary<ComponentRegistration, object>();

        public WeldEngine(WeldCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Run()
        {
            LoadCatalog();
            Configure();
        }

        private void LoadCatalog()
        {
            ResolveInjections();
        }

        void ResolveInjections()
        {
            var resolves = (from inject in _catalog.InjectionPoints
                            let satisfyingComponents = SatisfyInjection(inject)
                            let isGeneric = inject.RequestedType.ContainsGenericParameters
                            select new {inject.MemberInfo, inject.Index, satisfyingComponents, isGeneric}).ToArray();
            
            var groupByMember = (from resolve in resolves
                                where !resolve.isGeneric
                                group new {resolve.Index, resolve.satisfyingComponents} by resolve.MemberInfo into byMembers
                                let dependencies = byMembers
                                    .GroupBy(x=> x.Index, x=> x.satisfyingComponents)
                                    .ToDictionary(x=> x.Key, x=> x.SelectMany(_=> _).ToArray())
                                let member = byMembers.Key
                                select new { member, dependency = DependencyInjector.Create(this, member, dependencies) });

            _typeDependencies = groupByMember.GroupBy(x=> x.member.ReflectedType, x=> x.dependency)
                                    .ToDictionary(g=> g.Key, g=> g.ToArray());

            // TODO: generics
        }

        private ComponentRegistration[] SatisfyInjection(InjectionPoint inject)
        {
            var matches = (from component in _catalog.Components
                          let canSatisfy = component.CanSatisfy(new LookupSpec(inject.RequestedType, inject.Qualifiers.ToArray()))
                          where canSatisfy != false
                          select new {component, maybe = !canSatisfy.HasValue}).ToArray();

            if(!matches.Any())
                throw new UnsatisfiedDependencyException(inject);

            var hasValues = matches.Where(x => !x.maybe).ToArray();
            if (hasValues.Length > 1)
            {
                throw new AmbiguousDependencyException(inject, hasValues.Select(x => x.component).ToArray());
            }

            return matches.Select(x => x.component).ToArray();
        }

        private void Configure()
        {
            foreach (var config in _catalog.Configurations)
            {
            }
        }

        private void ExecuteConfig(Type config)
        {
            var configInstance = CreateComponent(config, new ActivatorComponentFactory(this, config));
            
        }

        private object CreateComponent(Type type, IComponentFactory factory)
        {
            var component = factory.CreateComponent(type);
            InvokePostConstruct(component);
            return component;
        }

        private void InvokePostConstruct(object component)
        {
            // TODO
        }

        public object Execute(object target, MethodBase method, IDictionary<int, ComponentRegistration[]> registrations)
        {
            if (method.IsStatic)
            {
                target = null;
            }

            registrations.ToDictionary(x => x.Key, x =>
            {
                var param = method.GetParameters()[x.Key];
                return GetInstance(x.Value);
            });

            // TODO
            return null;
        }

        private object GetInstance(ComponentRegistration registration)
        {
            // TODO
            return null;
        }

        public object GetInstance(ComponentRegistration[] registrations)
        {
            throw new NotImplementedException();
        }

        public DependencyInjector[] GetDependenciesOf(Type type)
        {
            DependencyInjector[] dependencies;
            if(!_typeDependencies.TryGetValue(type, out dependencies))
                return new DependencyInjector[0];

            return dependencies;
        }

        public void InjectDependencies(object target, DependencyInjector[] dependencies)
        {
            foreach (var dependency in dependencies.Where(x=> !x.IsConstructor))
            {
                // TODO
            }
        }
    }
}