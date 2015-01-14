using System;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public class AttributeScannerCatalogFactory
    {
        private const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
       
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
}