using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Contexts;
using Alpaca.Injects;
using Alpaca.Injects.Exceptions;
using Alpaca.Weld;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
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
}