using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Weld;
using Alpaca.Weld.Utils;

namespace Alpaca.Injects
{
    public interface IInjectionPoint
    {
        IComponent DeclaringComponent { get; }
        MemberInfo Member { get; }
        Type ComponentType { get; }
        IEnumerable<Attribute> Qualifiers { get; }
    }

    public interface IInstance<T>
    {

    }

    public interface IComponentManager
    {
        IComponent GetComponents(IInjectionPoint injectionPoint);
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
    }

    public abstract class AbstractInjectionPoint : IWeldInjetionPoint
    {
        protected readonly bool IsCacheable;
        private readonly Lazy<InjectPlan> _lazyInjectPlan; 

        protected AbstractInjectionPoint(IComponent declaringComponent, MemberInfo member, Type type, IEnumerable<Attribute> qualifiers)
        {
            DeclaringComponent = declaringComponent;
            Member = member;
            ComponentType = type;
            Qualifiers = qualifiers;
            IsCacheable = IsCacheableType(type);
            _lazyInjectPlan = new Lazy<InjectPlan>(BuildInjectPlan);
        }

        private static bool IsCacheableType(Type type)
        {
            return !typeof(IInjectionPoint).IsAssignableFrom(type) && !typeof(IInstance<>).IsAssignableFrom(GenericUtils.OpenIfGeneric(type));
        }

        public MemberInfo Member { get; private set; }
        public IComponent DeclaringComponent { get; private set; }
        public Type ComponentType { get; private set; }
        public IEnumerable<Attribute> Qualifiers { get; private set; }
        public abstract IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations);
        protected abstract InjectPlan BuildInjectPlan();

        public void Inject(object target)
        {
            _lazyInjectPlan.Value(target);
        }
    }

    public class MethodParameterInjectionPoint : AbstractInjectionPoint
    {
        private readonly ParameterInfo _param;
        private Lazy<BuildPlan> _lazyGetValuePlan = new Lazy<BuildPlan>(); 
        public MethodParameterInjectionPoint(IComponent declaringComponent, ParameterInfo paramInfo, IEnumerable<Attribute> qualifiers) 
            : base(declaringComponent, paramInfo.Member, paramInfo.ParameterType, qualifiers)
        {
            _param = paramInfo;
            IsConstructor = _param.Member is ConstructorInfo;
            _lazyGetValuePlan = new Lazy<BuildPlan>(BuildGetValuePlan);
        }

        private BuildPlan BuildGetValuePlan()
        {
            var manager = DeclaringComponent.Manager;
            var components = manager.GetComponents(this);
            if (IsCacheable)
            {
                var instance = manager.GetReference(components);
                return () => instance;
            }

            return () => manager.GetReference(components);
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
                return new MethodParameterInjectionPoint(component, param, Qualifiers);
            }
            else
            {
                var method = (MethodInfo)_param.Member;
                method = GenericUtils.TranslateMethodGenericArguments(method, translations);
                var param = method.GetParameters()[_param.Position];
                return new MethodParameterInjectionPoint(component, param, Qualifiers);
            }
        }

        protected override InjectPlan BuildInjectPlan()
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

        public FieldInjectionPoint(IComponent declaringComponent, FieldInfo field, IEnumerable<Attribute> qualifiers) :
            base(declaringComponent, field, field.FieldType, qualifiers)
        {
            InjectionValidator.Validate(field);
            _field = field;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var field = GenericUtils.TranslateFieldType(_field, translations);
            return new FieldInjectionPoint(component, field, Qualifiers);
        }

        protected override InjectPlan BuildInjectPlan()
        {
            var manager = DeclaringComponent.Manager;
            var components = manager.GetComponents(this);
            if (IsCacheable)
            {
                var instance = manager.GetReference(components);
                return target => SetValue(target, instance);
            }

            return target =>
            {
                var instance = manager.GetReference(components);
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

        public PropertyInjectionPoint(IComponent declaringComponent, PropertyInfo property, IEnumerable<Attribute> qualifiers):
            base(declaringComponent, property, property.PropertyType, qualifiers)
        {
            InjectionValidator.Validate(property);
            _property = property;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var property = GenericUtils.TranslatePropertyType(_property, translations);
            return new PropertyInjectionPoint(component, property, Qualifiers);
        }

        protected override InjectPlan BuildInjectPlan()
        {
            var manager = DeclaringComponent.Manager;
            var components = manager.GetComponents(this);
            if (IsCacheable)
            {
                var instance = manager.GetReference(components);
                return target => SetValue(target, instance);
            }

            return target =>
            {
                var instance = manager.GetReference(components);
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

    public class AttributeScannerCatalogFactory
    {
        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
       
        public WeldEnvironment AutoScan(IComponentManager manager)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var types = (from assembly in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                where assembly.GetReferencedAssemblies().Any(x=> AssemblyName.ReferenceMatchesDefinition(x, assemblyName))
                from type in assembly.GetLoadableTypes()
                where type.IsPublic && type.IsClass && !type.IsPrimitive
                select type).ToArray();

            //var classComponents = .ToArray();

            //var configurations = types.AsParallel().Where(ConfigurationCriteria.ScanPredicate).ToArray();

            var classComponents = (
                from type in types.AsParallel().Where(TypeUtils.IsComponent).AsParallel()
                let methods = type.GetMethods(AllBindingFlags).Where(InjectionValidator.ScanPredicate)
                let properties = type.GetProperties(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray()
                let ctors = type.GetConstructors(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray()
                let fields = type.GetFields(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray()
                let postConstructs = methods.Where(x=> x.HasAttribute<PostConstructAttribute>()).ToArray()
                let preDestroys = methods.Where(x => x.HasAttribute<PreDestroyAttribute>()).ToArray()
                select new { type,
                             injects = new { methods, properties, ctors, fields }, 
                             postConstructs, preDestroys }).ToArray();

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

            var environment = new WeldEnvironment();
            //catalog.RegisterConfigurations(configurations);

            foreach (var c in classComponents)
            {
                if(c.injects.ctors.Length > 1)
                    throw new InvalidComponentException(c.type, "Multiple [Inject] constructors");

                var component = new ClassComponent(c.type, c.type.GetQualifiers(), manager, c.postConstructs, c.preDestroys);
                var methodInjects = c.injects.methods.SelectMany(m => ToMethodInjections(component, m)).ToArray();
                var ctorInjects = c.injects.ctors.SelectMany(ctor => ToMethodInjections(component, ctor)).ToArray();
                var fieldInjects = c.injects.fields.Select(f => new FieldInjectionPoint(component, f, f.GetQualifiers())).ToArray();
                var propertyInjects = c.injects.fields.Select(f => new FieldInjectionPoint(component, f, f.GetQualifiers())).ToArray();

                foreach (var inject in methodInjects.Union(ctorInjects).Union(fieldInjects).Union(propertyInjects))
                    component.AddInjectionPoints(inject);    
                
                environment.AddComponent(component);
            }
                
            return environment;
        }

        private IEnumerable<IWeldInjetionPoint> ToMethodInjections(IComponent component, MethodBase method)
        {
            var parameters = method.GetParameters();
            return parameters.Select(p => new MethodParameterInjectionPoint(component, p, p.GetQualifiers()));
        }
    }

    public class WeldComponentManager : IComponentManager
    {
        public IComponent GetComponents(IInjectionPoint injectionPoint)
        {
            throw new NotImplementedException();
        }

        public object GetReference(IComponent component)
        {
            throw new NotImplementedException();
        }

        public object GetInjectableReference(IInjectionPoint injectionPoint, IComponent component)
        {
            throw new NotImplementedException();
        }

        public void Deploy(WeldEnvironment environment)
        {
            throw new NotImplementedException();
        }
    }
}