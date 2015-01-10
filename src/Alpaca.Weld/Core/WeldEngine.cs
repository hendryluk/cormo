using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Alpaca.Weld.Attributes;
using Alpaca.Weld.Utils;

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

            foreach (var member in _injects)
                catalog.RegisterInject(member, GetQualifiers(member));

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

            _components.Add(new ComponentRegistration
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

        public void RegisterInject(MemberInfo member, object[] qualifiers)
        {
            InjectionCriteria.Validate(member);
            if (!qualifiers.Any())
                qualifiers = new object[] {DefaultAttributeInstance};
            _injectionPoints.Add(new InjectionPoint(member, qualifiers));
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

    public class ComponentRegistration
    {
        public Type Component { get; set; }
        public HashSet<object> Qualifiers { get; set; }
    }

    public abstract class ComponentFactory
    {
        private readonly Type _type;
        private readonly object[] _qualifiers;

        protected ComponentFactory(Type type, object[] qualifiers)
        {
            _type = type;
            _qualifiers = qualifiers;
        }

        public virtual bool CanSatisfy(Type type, object[] qualifiers)
        {
            return _type.IsAssignableFrom(type) && qualifiers.All(_qualifiers.Contains);
        }

        public abstract object Construct();
    }

    public class ActivatorComponentFactory
    {
        
    }

    public class WeldEngine
    {
        private readonly WeldCatalog _catalog;

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
            LoadComponents();
            ResolveInjections();
        }

        void LoadComponents()
        {
            //var components = from reg in _catalog.Components
            //                 select new 
        }

        void ResolveInjections()
        {
            //var x  = from injection in _catalog.InjectionPoints
            //         _catalog.SatisfyInjection(injection)
        }


        private void Configure()
        {
            foreach (var config in _catalog.Configurations)
            {
                var configInstance = GetInstance(config, typeof(AnyAttribute));
            }
        }

        private object GetInstance(Type type, params Type[] qualifiers)
        {
            return null;
        }

        
    }
}