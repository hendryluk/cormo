using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Alpaca.Weld.Attributes;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Core
{
    public class Scanner
    {
        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
       
        //private Type[] _types;
        //private Type[] _configurations;
        //private IEnumerable<FieldInfo> _producesFields;
        //private IEnumerable<MethodInfo> _producesMethods;
        //private IEnumerable<PropertyInfo> _producesProperties;
        //private IEnumerable<PropertyInfo> _propertyInjects;
        //private MemberInfo[] _injects;
        //private IEnumerable<Type> _components;

        public WeldCatalog AutoScan()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var types = (from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                      where assembly.GetReferencedAssemblies().Any(x=> AssemblyName.ReferenceMatchesDefinition(x, assemblyName))
                      from type in assembly.GetLoadableTypes()
                      where type.IsPublic && type.IsClass && !type.IsPrimitive
                      select type).ToArray();

            var components = types.AsParallel().Where(TypeUtils.IsComponent).ToArray();

            var configurations = types.AsParallel().Where(ConfigurationCriteria.ScanPredicate).ToArray();

            var injects = (from type in types.AsParallel()
                        where !type.IsInterface && !type.IsAbstract
                        let methods = type.GetMethods(AllBindingFlags)
                        let properties = type.GetProperties(AllBindingFlags).Where(x => x.SetMethod == null || !x.SetMethod.IsAbstract)
                        let ctors = type.GetConstructors(AllBindingFlags)
                        let fields = type.GetFields(AllBindingFlags)
                        from member in methods.Cast<MemberInfo>().Union(properties).Union(ctors).Union(fields)
                        where InjectionCriteria.ScanPredicate(member)
                        select member).ToArray();

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

            var postConstructs = (from type in types.AsParallel()
                                   from method in type.GetMethods(AllBindingFlags)
                                   where method.HasAttribute<PostConstructAttribute>()
                                   select method).ToArray();

            var catalog = new WeldCatalog();
            catalog.RegisterConfigurations(configurations);
            
            foreach (var component in components.Except(configurations))
                catalog.RegisterComponent(component, GetQualifiers(component));

            foreach (var field in injects.OfType<FieldInfo>())
                catalog.RegisterInject(field, GetQualifiers(field));
            foreach (var method in injects.OfType<MethodBase>())
            {
                var seeks = method.GetParameters().Select(x => new SeekSpec(x.ParameterType, GetQualifiers(x)));
                catalog.RegisterInject(method, seeks.ToArray());
            }
            foreach (var property in injects.OfType<PropertyInfo>())
                catalog.RegisterInject(property, GetQualifiers(property));

            catalog.RegisterPostConstructs(postConstructs);

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
        private readonly List<ComponentRegistration> _configurations = new List<ComponentRegistration>();
        private readonly List<InjectRegistration> _injectRegistrations = new List<InjectRegistration>();
        private readonly List<MethodInfo> _postConstructs = new List<MethodInfo>();

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

            _components.Add(new ClassComponentRegistration(component, qualifierSet));
        }

        public void RegisterComponentInstance(object instance, params object[] qualifiers)
        {
            _components.Add(new InstanceComponentRegistration(instance, instance.GetType(), qualifiers));
        }

        public void RegisterConfigurations(params Type[] configurations)
        {
            foreach (var config in configurations)
            {
                ConfigurationCriteria.Validate(config);
            }
            foreach(var config in configurations)
                _configurations.Add(new ClassComponentRegistration(config, new object[]{AnyAttributeInstance, DefaultAttributeInstance}));
        }

        public void RegisterInject(FieldInfo field, object[] qualifiers)
        {
            InjectionCriteria.Validate(field);
            _injectRegistrations.Add(new InjectRegistration(field, field.FieldType, qualifiers));
        }

        public void RegisterInject(MethodBase method, SeekSpec[] spec)
        {
            InjectionCriteria.Validate(method);
            _injectRegistrations.Add(new InjectRegistration(method, spec));
        }

        public void RegisterInject(PropertyInfo property, object[] qualifiers)
        {
            InjectionCriteria.Validate(property);
            _injectRegistrations.Add(new InjectRegistration(property, property.PropertyType, qualifiers));
        }

        public void RegisterPostConstructs(params MethodInfo[] postConstructs)
        {
            foreach (var post in postConstructs)
            {
                PostConstructCriteria.Validate(post);
                _postConstructs.Add(post);
            }
        }

        public IEnumerable<ComponentRegistration> Configurations
        {
            get { return _configurations; }
        }

        public IEnumerable<ComponentRegistration> Components
        {
            get { return _components; }
        }

        public IEnumerable<InjectRegistration> InjectRegistrations
        {
            get { return _injectRegistrations;  }
        }

        public IEnumerable<MethodInfo> PostConstructs
        {
            get { return _postConstructs; }
        }
    }

    public abstract class ComponentRegistration
    {
        protected ComponentRegistration(Type type, IEnumerable<object> qualifiers)
        {
            Type = type;
            Qualifiers = new HashSet<object>(qualifiers);
        }

        public Type Type { get; set; }
        public HashSet<object> Qualifiers { get; set; }
        
        public virtual bool CanSatisfy(SeekSpec spec)
        {
            return CanSatisfy(spec.Qualifiers) && CanSatisfy(spec.Type);
        }

        private bool CanSatisfy(IEnumerable<object> qualifiers)
        {
            return qualifiers.All(Qualifiers.Contains);
        }

        protected bool CanSatisfy(Type requestedType)
        {
            return requestedType.IsAssignableFrom(Type);
        }

        public abstract BuildPlan GetBuildPlan(WeldEngine engine);
        //public abstract IComponentFactory GetFactory(WeldEngine engine, SeekSpec spec);
        public abstract ComponentRegistration ChangeType(Type requestedType);
    }

    public delegate object BuildPlan();
    public delegate object InjectPlan(object target);

    public class InstanceComponentRegistration : ComponentRegistration
    {
        private readonly object _instance;

        public InstanceComponentRegistration(object instance, Type type, IEnumerable<object> qualifiers)
            : base(type, qualifiers)
        {
            _instance = instance;
        }

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            return () => _instance;
        }

        public override ComponentRegistration ChangeType(Type requestedType)
        {
            throw new NotSupportedException();
        }
    }

    public class ClassComponentRegistration : ComponentRegistration
    {
        public ClassComponentRegistration(Type type, IEnumerable<object> qualifiers)
            : base(type, qualifiers)
        {
            
        }
        //protected override bool CanSatisfy(Type requestedType)
        //{
        //    var resolution = ResolveType(requestedType);
        //    if (resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
        //        return false;
        //    return true;
        //}

        //private GenericUtils.Resolution ResolveType(Type requestedType)
        //{
        //    return GenericUtils.ResolveGenericType(Type, requestedType);
        //}

        //public override IComponentFactory GetFactory(WeldEngine engine, SeekSpec spec)
        //{
        //    var resolution = ResolveType(spec.Type);
        //    if(resolution == null || resolution.ResolvedType == null || resolution.ResolvedType.ContainsGenericParameters)
        //        return null;

        //    return new ActivatorComponentFactory(engine, resolution.ResolvedType);
        //}

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            return engine.MakeConstructorBuildPlan(this);
        }

        public override ComponentRegistration ChangeType(Type requestedType)
        {
            return new ClassComponentRegistration(requestedType, Qualifiers);
        }
    }

    public class ProducerRegistration: ComponentRegistration
    {
        public ProducerRegistration(Type type, IEnumerable<object> qualifiers, MemberInfo producer)
            : base(type, qualifiers)
        {
            Producer = producer;
        }

        public MemberInfo Producer { get; set; }

        //protected override bool CanSatisfy(Type requestedType)
        //{
        //    var producer = ResolveProducer(requestedType);
        //    if (producer == null || GenericUtils.MemberContainsGenericArguments(producer))
        //        return false;
        //    return true;
        //}

        //private MemberInfo ResolveProducer(Type requestedType)
        //{
        //    var typeResolution = GenericUtils.ResolveGenericType(Type, requestedType);
        //    if (typeResolution == null)
        //        return null;
                
        //    return GenericUtils.TranslateMemberGenericArguments(Producer, typeResolution.GenericParameterTranslations);
        //}

        //public override IComponentFactory GetFactory(WeldEngine engine, SeekSpec spec)
        //{
        //    var resolvedProducer = ResolveProducer(spec.Type);
        //    if (GenericUtils.MemberContainsGenericArguments(resolvedProducer))
        //        return null;

        //    var method = resolvedProducer as MethodInfo;
        //    if (method != null)
        //        return new ProducerMethodComponentFactory(engine, method);

        //    return null;
        //}

        public override BuildPlan GetBuildPlan(WeldEngine engine)
        {
            // TODO
            return null;
            //return engine.MakeExecutionPlan(Producer);
        }

        public override ComponentRegistration ChangeType(Type type)
        {
            var translations = GenericUtils.CreateGenericTranslactions(type);
            var producer = GenericUtils.TranslateMemberGenericArguments(Producer, translations);

            return new ProducerRegistration(type, Qualifiers, producer);
        }
    }

    //public interface IComponentFactory
    //{
    //    object CreateComponent(Type requestedType);
    //    bool GuarantesResult { get; }
    //}

    //public class ProducerMethodComponentFactory : IComponentFactory
    //{
    //    private readonly WeldEngine _engine;
    //    private readonly MethodInfo _producer;

    //    public ProducerMethodComponentFactory(WeldEngine engine, MethodInfo producer)
    //    {
    //        _engine = engine;
    //        _producer = producer;
    //    }

    //    public object CreateComponent(Type requestedType)
    //    {
    //        throw new NotImplementedException();
    //        //return engine.Execute(_producer);
    //    }

    //    public bool GuarantesResult { get { return true; } }
    //}

    //public class ActivatorComponentFactory: IComponentFactory
    //{
    //    private readonly WeldEngine _engine;
    //    private readonly Type _type;
    //    private Lazy<DependencyInjector[]> _dependencies;
    //    private Func<object> _constructor;

    //    public ActivatorComponentFactory(WeldEngine engine, Type type)
    //    {
    //        _engine = engine;
    //        _type = type;
    //        _dependencies = new Lazy<DependencyInjector[]>(LoadDependencies);
    //    }

    //    private DependencyInjector[] LoadDependencies()
    //    {
    //        var dependencies = _engine.GetDependenciesOf(_type);
    //        var constructors = dependencies.Where(x => x.IsConstructor).ToArray();

    //        if (constructors.Length > 1)
    //        {
    //            throw new InvalidComponentException(_type, "Multiple [Inject] constructors");
    //        }
    //        if (constructors.Length == 1)
    //        {
    //            var ctr = constructors[0];
    //            _constructor = () => ctr.Inject(null);
    //        }
    //        else
    //        {
    //            _constructor = () => Activator.CreateInstance(_type, true);
    //        }

    //        return dependencies.Where(x => !x.IsConstructor).ToArray();
    //    }

    //    public object CreateComponent(Type requestedType)
    //    {
    //        var dependencies = _dependencies.Value;
    //        var obj = _constructor();
    //        _engine.InjectDependencies(obj, dependencies);
    //        return obj;
    //    }

    //    public bool GuarantesResult { get { return true; } }
    //}

    public struct SeekSpec
    {
        private static readonly DefaultAttribute DefaultAttributeInstance = new DefaultAttribute();
        public SeekSpec(Type type, object[] qualifiers)
            : this()
        {
            Type = type;
            Qualifiers = SetQualifierDefaults(qualifiers);
        }

        private object[] SetQualifierDefaults(object[] qualifiers)
        {
            if (!qualifiers.Any())
                return new object[] { DefaultAttributeInstance };

            return qualifiers;
        }

        public bool Multiple { get; private set; }
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
                return onMethod(method);
            var field = member as FieldInfo;
            if (field != null)
                return onField(field);
            var property = member as PropertyInfo;
            if (property != null)
                return onProperty(property);

            return default(T);
        }
    }

    //public abstract class DependencyInjector
    //{
    //    protected readonly WeldEngine Engine;
    //    private readonly ComponentRegistration[][] _registrations;

    //    protected DependencyInjector(WeldEngine engine, ComponentRegistration[][] registrations)
    //    {
    //        Engine = engine;
    //        _registrations = registrations;
    //    }

    //    protected virtual object GetDependency()
    //    {
    //        return Engine.GetInstance(_registrations[0]);
    //    }

    //    public abstract object Inject(object target);
    //    public abstract bool IsConstructor { get; }
        
    //    public static DependencyInjector Create(WeldEngine engine, MemberInfo member, ComponentRegistration[][] registrations)
    //    {
    //        return MemberVisitor.VisitInject<DependencyInjector>(member,
    //            method => new ToMethod(engine, registrations, method), 
    //            field => new ToField(engine, registrations, field),
    //            property=> new ToProperty(engine, registrations, property));
    //    }

    //    public class ToMethod : DependencyInjector
    //    {
    //        private readonly MethodBase _method;

    //        public ToMethod(WeldEngine engine, ComponentRegistration[][] registrations, MethodBase method)
    //            : base(engine, registrations)
    //        {
    //            _method = method;
    //        }

    //        public override object Inject(object target)
    //        {
    //            return Engine.Execute(target, _method, _registrations);
    //        }

    //        public override bool IsConstructor
    //        {
    //            get { return _method is ConstructorInfo; }
    //        }
    //    }

    //    public class ToField : DependencyInjector
    //    {
    //        private readonly FieldInfo _field;

    //        public ToField(WeldEngine engine, ComponentRegistration[][] registrations, FieldInfo field)
    //            : base(engine, registrations)
    //        {
    //            _field = field;
    //        }

    //        public override object Inject(object target)
    //        {
    //            var value = GetDependency();
    //            _field.SetValue(target, value);
    //            return value;
    //        }

    //        public override bool IsConstructor
    //        {
    //            get { return false; }
    //        }
    //    }

    //    public class ToProperty : DependencyInjector
    //    {
    //        private readonly PropertyInfo _property;

    //        public ToProperty(WeldEngine engine, ComponentRegistration[][] registrations, PropertyInfo property)
    //            : base(engine, registrations)
    //        {
    //            _property = property;
    //        }

    //        public override object Inject(object target)
    //        {
    //            var value = GetDependency();
    //            _property.SetValue(target, value); 
    //            return value;
    //        }

    //        public override bool IsConstructor
    //        {
    //            get { return false; }
    //        }
    //    }
    //}

    public class WeldEngine
    {
        private readonly WeldCatalog _catalog;
        //private Dictionary<Type, DependencyInjector[]> _typeDependencies;
        private Dictionary<ComponentRegistration, object> _componentValues = new Dictionary<ComponentRegistration, object>();

        public WeldEngine(WeldCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Run()
        {
            BuildIndex();
            BuildDependencyGraph();
            Configure();
        }

        public class InjectionPoint
        {
            public MemberInfo Member { get; private set; }
            public Type RequestedType { get; private set; }
            public IEnumerable<object> Qualifiers { get; private set; }

            public InjectionPoint(MemberInfo member, Type requestedType, IEnumerable<object> qualifiers)
            {
                Member = member;
                RequestedType = requestedType;
                Qualifiers = qualifiers; 
            }

            public override string ToString()
            {
                return string.Format("type [{0}] with qualifiers [{1}] at injection point [{2}]",
                    RequestedType, string.Join(",", Qualifiers.Select(x => x.GetType().Name)), Member);
            }
        }

        IDictionary<Type, MethodInfo[]> _postConstructs;
        Dictionary<ComponentRegistration, ComponentGraphNode> _nodeIndex = new Dictionary<ComponentRegistration, ComponentGraphNode>();
        Dictionary<Type, ComponentRegistration[]> _regIndex;
        Dictionary<Type, InjectRegistration[]> _injectIndex;
        List<ComponentRegistration> _unprocessedRegistrations;

        void BuildIndex()
        {
            _postConstructs = _catalog.PostConstructs.GroupBy(x => x.ReflectedType).ToDictionary(x => x.Key, x => x.ToArray());
        }

        // GRAPH
        public class DependencyLink
        {
            public bool AllowMultiple { get; set; }
            public List<ComponentGraphNode> Components = new List<ComponentGraphNode>();
            public InjectionPoint InjectionPoint { get; set; }

            public void AddAll(ComponentGraphNode[] nodes)
            {
                Components.AddRange(nodes);
                foreach (var node in nodes)
                {
                    node.Dependents.Add(this);
                }
            }
        }

        public class ComponentGraphNode
        {
            public ComponentRegistration Registration { get; private set; }
            public ConcurrentBag<DependencyLink> Dependents = new ConcurrentBag<DependencyLink>();

            public ComponentGraphNode(ComponentRegistration reg)
            {
                Registration = reg;
            }

            public IDictionary<MemberInfo, DependencyLink[]> Dependencies { get; set; }
        }
        
        void BuildDependencyGraph()
        {
            var components = _catalog.Components.Union(_catalog.Configurations).ToArray();
            _regIndex = (from component in components
                         from type in TypeUtils.GetComponentTypes(component.Type)
                         group component by type)
                         .ToDictionary(x => x.Key, x => x.ToArray());
            _injectIndex = _catalog.InjectRegistrations.GroupBy(x => x.MemberInfo.ReflectedType).ToDictionary(x=> x.Key, x=> x.ToArray());

            _nodeIndex = components.ToDictionary(x => x, x => new ComponentGraphNode(x));

            var processing = components;
            while (processing.Any())
            {
                _unprocessedRegistrations = new List<ComponentRegistration>();
                
                foreach (var reg in processing)
                {
                    var node = _nodeIndex[reg];
                    
                    var injects = GetTypeInjects(reg.Type);
                    node.Dependencies = injects.ToDictionary(x => x.MemberInfo, x => x.Dependencies.Select(y => ResolveDependency(x.MemberInfo, y)).ToArray());
                }
                processing = _unprocessedRegistrations.ToArray();
            }
            
        }

        private InjectRegistration[] GetTypeInjects(Type type)
        {
            InjectRegistration[] injects;
            if (!_injectIndex.TryGetValue(type, out injects))
            {
                if (type.IsGenericType)
                {
                    var translations = GenericUtils.CreateGenericTranslactions(type);

                    InjectRegistration[] genericInjects;
                    if (!_injectIndex.TryGetValue(type.GetGenericTypeDefinition(), out genericInjects))
                        genericInjects = new InjectRegistration[0];

                    genericInjects = genericInjects.Select(x =>
                    {
                        var resolvedMethod = GenericUtils.TranslateMemberGenericArguments(x.MemberInfo, translations);
                        return new InjectRegistration(resolvedMethod, x.Dependencies);
                    }).ToArray();

                    _injectIndex[type] = injects = genericInjects;
                }
                else
                    injects = new InjectRegistration[0];
            }


            ValidateInjects(type, injects);
            return injects;
        }

        private MethodInfo[] GetPostConstructs(Type type)
        {
            MethodInfo[] postConstructs;
            if (_postConstructs.TryGetValue(type, out postConstructs))
                return postConstructs;

            if (type.IsGenericType)
            {
                if (_postConstructs.TryGetValue(type, out postConstructs))
                {
                    var translations = GenericUtils.CreateGenericTranslactions(type);
                    postConstructs = postConstructs.Select(x=> GenericUtils.TranslateMemberGenericArguments(x, translations)).Cast<MethodInfo>().ToArray();
                    return postConstructs;
                }
            }

            return new MethodInfo[0];
        }

        private void ValidateInjects(Type type, InjectRegistration[] injects)
        {
            object[] ctors = injects.Select(x => x.MemberInfo).OfType<ConstructorInfo>().ToArray();
            if (ctors.Length > 2)
            {
                throw new InvalidComponentException(type, string.Format("Multiple [Inject] constructors: [{0}]", string.Join(",", ctors)));
            }
        }

        private DependencyLink ResolveDependency(MemberInfo member, SeekSpec spec)
        {
            var link = new DependencyLink
            {
                InjectionPoint = new InjectionPoint(member, spec.Type, spec.Qualifiers),
                AllowMultiple = spec.Multiple
            };

            ComponentRegistration[] registrations;
            
            if (!_regIndex.TryGetValue(spec.Type, out registrations))
            {
                if (spec.Type.IsGenericType)
                {
                    if (_regIndex.TryGetValue(spec.Type.GetGenericTypeDefinition(), out registrations))
                    {
                        registrations = registrations.Select(x => CloseRegistrationGenerics(x, spec.Type)).ToArray();
                    }
                }
            }

            if (registrations == null || !registrations.Any())
            {
                if (!link.AllowMultiple)
                    throw new UnsatisfiedDependencyException(link.InjectionPoint);
            }
            else
            {
                var matched = registrations.Where(x => x.CanSatisfy(spec)).ToArray();
                link.AddAll(matched.Select(x=> _nodeIndex[x]).ToArray());
            }

            return link;
        }

        private ComponentRegistration CloseRegistrationGenerics(ComponentRegistration reg, Type type)
        {
            var registration = reg.ChangeType(type);
            _unprocessedRegistrations.Add(registration);
            _nodeIndex.Add(registration, new ComponentGraphNode(registration));
            return registration;
        }

        // \GRAPH

        //void ResolveInjections()
        //{
        //    var resolves = (from inject in _catalog.InjectRegistrations
        //                    let satisfyingComponents = inject.Dependencies.Select(SatisfyInjection).ToArray()
        //                    let isGeneric = GenericUtils.MemberContainsGenericArguments(inject.MemberInfo)
        //                    select new {inject.MemberInfo, satisfyingComponents, isGeneric}).ToArray();
            
        //    var groupByMember = (from resolve in resolves
        //                        where !resolve.isGeneric
        //                        select new { resolve.MemberInfo, dependency = DependencyInjector.Create(this, resolve.MemberInfo, resolve.satisfyingComponents) });

        //    _typeDependencies = groupByMember.GroupBy(x=> x.MemberInfo.ReflectedType, x=> x.dependency)
        //                            .ToDictionary(g=> g.Key, g=> g.ToArray());

        //    // TODO: generics
        //}

        //private ComponentRegistration[] SatisfyInjection(SeekSpec seek)
        //{
        //    var matches = (from component in _catalog.Components
        //                   let canSatisfy = component.CanSatisfy(seek)
        //                  where canSatisfy != false
        //                  select new {component, maybe = !canSatisfy.HasValue}).ToArray();

        //    if(!matches.Any())
        //        throw new UnsatisfiedDependencyException(seek);

        //    var hasValues = matches.Where(x => !x.maybe).ToArray();
        //    if (hasValues.Length > 1)
        //    {
        //        throw new AmbiguousDependencyException(seek, hasValues.Select(x => x.component).ToArray());
        //    }

        //    return matches.Select(x => x.component).ToArray();
        //}

        private void Configure()
        {
            foreach (var config in _catalog.Configurations)
            {
                // Load Imports
                GetInstance(config);
            }
        }

        public object Execute(object target, MethodBase method, ComponentRegistration[][] registrations)
        {
            if (method.IsStatic)
            {
                target = null;
            }

            // TODO
            return null;
        }

        private readonly ConcurrentDictionary<ComponentRegistration, BuildPlan> _buildPlans = new ConcurrentDictionary<ComponentRegistration, BuildPlan>(); // temporary quick implementation
        
        private readonly ConcurrentDictionary<ComponentRegistration, object> _singletonInstances = new ConcurrentDictionary<ComponentRegistration, object>(); // temporary quick implementation
        private object GetInstance(ComponentRegistration registration)
        {
            return _singletonInstances.GetOrAdd(registration, BuildComponent);
        }

        private object BuildComponent(ComponentRegistration registration)
        {
            var buildPlan = _buildPlans.GetOrAdd(registration, r=> r.GetBuildPlan(this));
            return buildPlan();
        }

        public BuildPlan MakeConstructorBuildPlan(ComponentRegistration registration)
        {
            var node = _nodeIndex[registration];
            var injectCtors = node.Dependencies.Where(x => x.Key is ConstructorInfo).ToArray();
            var postConstructs = GetPostConstructs(registration.Type);

            BuildPlan construction;
            if (injectCtors.Any())
            {
                var inject = injectCtors.First();
                construction = ()=> MakeExecutionPlan((MethodBase)inject.Key, inject.Value)(null);
            }
            else
            {
                construction = () => Activator.CreateInstance(registration.Type, true);
            }

            var injectPlans = node.Dependencies.Except(injectCtors).Select(x=> MakeInjectionPlan(x.Key, x.Value)).ToArray();

            return () =>
            {
                var obj = construction();
                foreach (var inject in injectPlans)
                    inject(obj);
                foreach (var post in postConstructs)
                    post.Invoke(obj, null);
                return obj;
            };
        }

        public InjectPlan MakeInjectionPlan(MemberInfo member, DependencyLink[] dependencies)
        {
            return MemberVisitor.VisitInject(member,
                method => MakeExecutionPlan(method, dependencies),
                field => MakeExecutionPlan(field, dependencies.First().Components.First().Registration),
                property => MakeExecutionPlan(property, dependencies.First().Components.First().Registration)
                );
        }

        public InjectPlan MakeExecutionPlan(FieldInfo field, ComponentRegistration reg)
        {
            return o =>
            {
                var val = GetInstance(reg);
                field.SetValue(o, val);
                return val;
            };
        }

        public InjectPlan MakeExecutionPlan(PropertyInfo property, ComponentRegistration reg)
        {
            return o =>
            {
                var val = GetInstance(reg);
                property.SetValue(o, val);
                return val;
            };
        }

        public InjectPlan MakeExecutionPlan(MethodBase method, DependencyLink[] dependencies)
        {
            var regs = dependencies.Select(x => x.Components.First().Registration).ToArray();
            return o =>
            {
                var args = regs.Select(GetInstance).ToArray();
                return method.Invoke(o, args);
            };
        }

        //public object GetInstance(ComponentRegistration[] registrations)
        //{
        //    throw new NotImplementedException();
        //}

        //public DependencyInjector[] GetDependenciesOf(Type type)
        //{
        //    DependencyInjector[] dependencies;
        //    if(!_typeDependencies.TryGetValue(type, out dependencies))
        //        return new DependencyInjector[0];

        //    return dependencies;
        //}

        //public void InjectDependencies(object target, DependencyInjector[] dependencies)
        //{
        //    foreach (var dependency in dependencies.Where(x=> !x.IsConstructor))
        //    {
        //        // TODO
        //    }
        //}

        
    }
}