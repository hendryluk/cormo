using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Contexts;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Injections;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Injects.Exceptions;
using Cormo.Interceptions;
using Cormo.Mixins;

namespace Cormo.Impl.Weld
{
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

        public IEnumerable<Assembly> WhereReferencesRecursive(Assembly[] assemblies, params AssemblyName[] names)
        {
            var referencings =
                assemblies.Where(a => a.GetReferencedAssemblies().Any(
                    r => names.Any(n=> AssemblyName.ReferenceMatchesDefinition(r, n))))
                    .ToArray();

            if (!referencings.Any()) 
                return referencings;

            var others = assemblies.Except(referencings).ToArray();
            var referencingNames = referencings.Select(x => x.GetName()).ToArray();
            referencings = referencings.Union(WhereReferencesRecursive(others, referencingNames)).ToArray();

            return referencings;
        }

        // Declaring known built-in types explicitly for performance reason
        private static readonly Type[] BuiltInTypes = {typeof(ValueProvider)};

        public void AutoScan()
        {
            var cormo = typeof(IComponentManager).Assembly;
            var assemblyName = cormo.GetName();

            var assemblies = WhereReferencesRecursive(AppDomain.CurrentDomain.GetAssemblies(), assemblyName).ToArray();

            var types = (from assembly in assemblies.AsParallel()
                            from type in assembly.GetLoadableTypes()
                            where type.IsVisible && type.IsClass && !type.IsPrimitive
                            select type)
                            .AsEnumerable().Union(BuiltInTypes)
                            .ToArray();

            var componentTypes = types.AsParallel().Where(TypeUtils.IsComponent).ToArray();

            var assemblyConfigs = (from assembly in assemblies.AsParallel()
                                  from import in assembly.GetAttributesRecursive<ImportAttribute>()
                                  from type in import.Types
                                  select type).ToArray();
            
            AddTypes(componentTypes);
            var configs = GetConfigs(_environment.Components, assemblyConfigs);

            foreach (var c in configs)
                _environment.AddConfiguration(c);
        }

        public void AddTypes(params Type[] types)
        {
            var components = types.AsParallel().Select(MakeComponent).ToArray();

            var producerFields = (from component in components.AsParallel()
                                  let type = component.Type
                                  from field in type.GetFields(AllBindingFlags)
                                  where field.HasAttributeRecursive<ProducesAttribute>()
                                  select MakeProducerField(component, field)).ToArray();

            var producerMethods = (from component in components.AsParallel()
                                   let type = component.Type
                                   from method in type.GetMethods(AllBindingFlags)
                                   where method.HasAttributeRecursive<ProducesAttribute>()
                                   select MakeProducerMethod(component, method)).ToArray();

            var producerProperties = (from component in components.AsParallel()
                                      let type = component.Type
                                      from property in type.GetProperties(AllBindingFlags)
                                      where property.HasAttributeRecursive<ProducesAttribute>()
                                      select MakeProducerProperty(component, property)).ToArray();

            foreach (var c in components.Union(producerFields).Union(producerMethods).Union(producerProperties))
                _environment.AddComponent(c);   

        }

        public void AddValue(object instance, params IBinderAttribute[] binders)
        {
            _environment.AddValue(instance, binders, _manager);
        }
        

        private static IEnumerable<IWeldComponent> GetConfigs(IEnumerable<IWeldComponent> components, Type[] assemblyConfigs)
        {
            var componentMap = components.OfType<ClassComponent>().ToDictionary(x => x.Type, x => x);
            var configs = new List<ClassComponent>();
            var newConfigs = componentMap.Values
                .Where(x => assemblyConfigs.Contains(x.Type) || x.Type.HasAttributeRecursive<ConfigurationAttribute>())
                .ToArray();

            while (newConfigs.Any())
            {
                configs.AddRange(newConfigs);

                var imports = from config in newConfigs
                    from import in config.Type.GetAttributesRecursive<ImportAttribute>()
                    from importType in import.Types
                    select new {config.Type, importType};

                newConfigs = imports.Select(x =>
                {
                    ClassComponent component;
                    if (componentMap.TryGetValue(x.importType, out component))
                        return component;
                    throw new InvalidComponentException(x.importType,
                        string.Format("Could not import a non-component type from Configuration [{0}]", x.Type));
                })
                .Except(configs)
                .ToArray();
            }
            return configs;
        }

        public IWeldComponent MakeProducerField(IWeldComponent component, FieldInfo field)
        {
            var binders = field.GetBinders();
            var scope = field.GetAttributesRecursive<ScopeAttribute>().Select(x=> x.GetType()).FirstOrDefault() ?? typeof(DependentAttribute);

            return new ProducerField(component, field, binders, scope, _manager);
        }

        public IWeldComponent MakeProducerProperty(IWeldComponent component, PropertyInfo property)
        {
            var binders = property.GetBinders();
            var scope = property.GetAttributesRecursive<ScopeAttribute>().Select(x => x.GetType()).FirstOrDefault() ?? typeof(DependentAttribute);

            return new ProducerProperty(component, property, binders, scope, _manager);
        }

        public IWeldComponent MakeProducerMethod(IWeldComponent component, MethodInfo method)
        {
            var binders = method.GetBinders();
            var scope = method.GetAttributesRecursive<ScopeAttribute>().Select(x => x.GetType()).FirstOrDefault() ?? typeof(DependentAttribute);

            var producer = new ProducerMethod(component, method, binders, scope, _manager);
            var injects = ToMethodInjections(producer, method).ToArray();
            producer.AddInjectionPoints(injects);
            return producer;
        }

        public IWeldComponent MakeComponent(Type type)
        {
            var binders = type.GetBinders();
            
            var methods = type.GetMethods(AllBindingFlags).ToArray();

            var iMethods = methods.Where(InjectionValidator.ScanPredicate).ToArray();
            var iProperties = type.GetProperties(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var iCtors = type.GetConstructors(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var iFields = type.GetFields(AllBindingFlags).Where(InjectionValidator.ScanPredicate).ToArray();
            var postConstructs = methods.Where(x => x.HasAttributeRecursive<PostConstructAttribute>()).ToArray();
            var scope = type.GetAttributesRecursive<ScopeAttribute>().Select(x=> x.GetType()).FirstOrDefault() ?? typeof(DependentAttribute);

            if (iCtors.Length > 1)
                throw new InvalidComponentException(type, "Multiple [Inject] constructors");
            
            ManagedComponent component;
            if (binders.OfType<InterceptorAttribute>().Any())
            {
                component = new Interceptor(type, binders, scope, _manager, postConstructs);
            }
            else
            {
                var mixinAttr = type.GetAttributesRecursive<MixinAttribute>().FirstOrDefault();
                component = mixinAttr==null? (ManagedComponent)
                new ClassComponent(type, binders, scope, _manager, postConstructs):
                new Mixin(mixinAttr.InterfaceTypes, type, binders, scope, _manager, postConstructs);
            }
            
            var methodInjects = iMethods.SelectMany(m => ToMethodInjections(component, m)).ToArray();
            var ctorInjects = iCtors.SelectMany(ctor => ToMethodInjections(component, ctor)).ToArray();
            var fieldInjects = iFields.Select(f => new FieldInjectionPoint(component, f, f.GetBinders())).ToArray();
            var propertyInjects = iProperties.Select(p => new PropertyInjectionPoint(component, p, p.GetBinders())).ToArray();
            
            component.AddInjectionPoints(methodInjects.Union(ctorInjects).Union(fieldInjects).Union(propertyInjects).ToArray());
            return component;
        }

        private IEnumerable<IWeldInjetionPoint> ToMethodInjections(IComponent component, MethodBase method)
        {
            var parameters = method.GetParameters();
            return parameters.Select(p => new MethodParameterInjectionPoint(component, p, p.GetBinders()));
        }

        public void Deploy()
        {
            AddContexts();
            AddBuiltInComponents();
            _manager.Deploy(_environment);
        }

        private void AddBuiltInComponents()
        {
            _environment.AddComponent(new InjectionPointComponent(_manager));
        }

        private void AddContexts()
        {
            _manager.AddContext(new DependentContext());
            _manager.AddContext(new SingletonContext());
        }
    }
}