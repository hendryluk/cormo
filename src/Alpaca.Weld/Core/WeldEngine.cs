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
        public MemberInfo Producer { get; set; }

        public virtual IComponentFactory CreateFactory(Type requestedType, object[] qualifiers)
        {
            if (!qualifiers.All(Qualifiers.Contains))
                return null;

            var typeResolution = GenericUtils.ResolveGenericType(Component, requestedType);
            if (typeResolution == null)
                return null;

            return BuildFactory(typeResolution);
        }

        private IComponentFactory BuildFactory(GenericUtils.Resolution typeResolution)
        {
            //var containsGenericParameters = typeResolution.ResolvedType.ContainsGenericParameters;

            //var producer = Producer;
            //if (!containsGenericParameters && producer != null)
            //{
            //    producer = GenericUtils.MakeGenericMember(producer, typeResolution.GenericParameterTranslations);
            //    if (producer == null)
            //        return null;

            //}

            //if (typeResolution.ResolvedType.ContainsGenericParameters)
            //{
                
            //}

            //if(typeResolution.)
            return null;
        }

        private IComponentFactory BuildProducerFactory(GenericUtils.Resolution typeResolution)
        {
            var producerField = Producer as FieldInfo;
            if (producerField != null)
            {
                producerField = GenericUtils.ResolveFieldToReturn(producerField, typeResolution.ResolvedType);
                if (producerField != null)
                {
                    // TODO
                    return null;
                }
            }

            var producerMethod = Producer as MethodInfo;
            if (producerMethod != null)
            {
                producerMethod = GenericUtils.ResolveMethodToReturn(producerMethod, typeResolution.ResolvedType);
                if (producerMethod != null)
                {
                    // TODO
                    return null;
                }
            }

            var producerProperty = Producer as PropertyInfo;
            if (producerProperty != null)
            {
                producerProperty = GenericUtils.ResolvePropertyToReturn(producerProperty, typeResolution.ResolvedType);
                if (producerProperty != null)
                {
                    // TODO
                    return null;
                }
            }

            return null;
        }
    }

    public interface IComponentFactory
    {
        object CreateComponent(Type requestedType);
    }

    public class ActivatorComponentFactory: IComponentFactory
    {
        private readonly GenericUtils.Resolution _typeResolution;

        public ActivatorComponentFactory(GenericUtils.Resolution typeResolution)
        {
            _typeResolution = typeResolution;
        }

        public object CreateComponent(Type requestedType)
        {
            return null;
        }
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