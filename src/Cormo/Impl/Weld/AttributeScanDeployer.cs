using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Catch;
using Cormo.Events;
using Cormo.Impl.Solder;
using Cormo.Impl.Utils;
using Cormo.Impl.Weld.Components;
using Cormo.Impl.Weld.Contexts;
using Cormo.Impl.Weld.Introspectors;
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
        private static readonly Type[] BuiltInTypes =
        {
            typeof(ValueProvider), typeof(AppSettingsValueProvider), 
            typeof(ExceptionsHandledInterceptor), typeof(ExceptionsHandled.Configurator)
        };

        public void AutoScan()
        {
            var cormo = typeof(IComponentManager).Assembly;
            var assemblyName = cormo.GetName();

            var assemblies = WhereReferencesRecursive(AppDomain.CurrentDomain.GetAssemblies(), assemblyName).ToArray();

            var types = (from assembly in assemblies.AsParallel()
                            from type in assembly.GetLoadableTypes()
                            where type.IsClass && !type.IsPrimitive
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

        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public void AddTypes(params Type[] types)
        {
            var components = types.AsParallel().Select(MakeComponent).ToArray();
            var eventObservers = components.AsParallel().SelectMany(FindEventObservers).ToArray();
            var exceptionHandlers = components.AsParallel().SelectMany(FindExceptionHandlers).ToArray();

            var producerFields = (from component in components.AsParallel()
                                  let type = component.Type
                                  from field in ScannerUtils.GetAllField(type, AllBindingFlags)
                                  where field.HasAttributeRecursive<ProducesAttribute>()
                                  select (IWeldComponent) new ProducerField(component, field, _manager)).ToArray();

            var producerMethods = (from component in components.AsParallel()
                                   let type = component.Type
                                   from method in type.GetMethods(AllBindingFlags)
                                   where method.HasAttributeRecursive<ProducesAttribute>()
                                   select (IWeldComponent) new ProducerMethod(component, method, _manager)).ToArray();

            var producerProperties = (from component in components.AsParallel()
                                      let type = component.Type
                                      from property in type.GetProperties(AllBindingFlags)
                                      where property.HasAttributeRecursive<ProducesAttribute>()
                                      select (IWeldComponent) new ProducerProperty(component, property, _manager)).ToArray();

            foreach (var c in components.Union(producerFields).Union(producerMethods).Union(producerProperties))
                _environment.AddComponent(c);

            foreach (var observer in eventObservers)
                _environment.AddObserver(observer);
            foreach (var handler in exceptionHandlers)
                _environment.AddExceptionHandlers(handler);
        }

        private static IEnumerable<EventObserverMethod> FindEventObservers(IWeldComponent component)
        {
            foreach (var method in component.Type.GetMethods())
            {
                var injectParams = method.GetParameters()
                    .Where(AttributeUtils.HasAttributeRecursive<ObservesAttribute>)
                    .ToArray();
                if(!injectParams.Any())
                    continue;
                if (injectParams.Length > 1)
                    throw new InvalidComponentException(Formatters.MultipleObservesParameter(method));

                var param = injectParams.Single();
                yield return new EventObserverMethod(component, param, param.GetBinders());
            }
        }

        private static IEnumerable<EventObserverMethod> FindExceptionHandlers(IWeldComponent component)
        {
            foreach (var method in component.Type.GetMethods())
            {
                var injectParams = method.GetParameters()
                    .Where(AttributeUtils.HasAttributeRecursive<HandlesAttribute>)
                    .ToArray();
                if (!injectParams.Any())
                    continue;
                if (injectParams.Length > 1)
                    throw new InvalidComponentException(Formatters.MultipleHandlesParameter(method));

                var param = injectParams.Single();
                if(GenericUtils.OpenIfGeneric(param.ParameterType) != typeof(ICaughtException<>))
                    throw new InvalidComponentException(Formatters.WrongHandlesParamType(param));

                yield return new EventObserverMethod(component, param, param.GetBinders());
            }
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

        public IWeldComponent MakeComponent(Type type)
        {
            ManagedComponent component;
            var binders = type.GetBinders();
            if (binders.OfType<InterceptorAttribute>().Any())
            {
                component = new Interceptor(type, _manager);
            }
            else
            {
                component = binders.OfType<MixinAttribute>().Any()? (ManagedComponent)
                    new Mixin(type, _manager):
                    new ClassComponent(type, _manager);
            }

            return component;
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