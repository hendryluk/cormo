using System.Collections.Generic;

namespace Alpaca.Weld
{
    public class WeldEnvironment
    {
        private readonly List<IWeldComponent> _components = new List<IWeldComponent>();
        private readonly List<IWeldComponent> _configurations = new List<IWeldComponent>();
        //private readonly List<AbstractComponent> _configurations = new List<AbstractComponent>();
        //private readonly List<InjectRegistration> _injectRegistrations = new List<InjectRegistration>();
        //private readonly List<MethodInfo> _postConstructs = new List<MethodInfo>();

        public IEnumerable<IWeldComponent> Components { get { return _components; } }
        public IEnumerable<IWeldComponent> Configurations { get { return _configurations; } }

        //public AbstractComponent RegisterComponent(Type component, params object[] qualifiers)
        //{
        //    ComponentCriteria.Validate(component);

        //    var qualifierSet = new HashSet<object>(qualifiers);
        //    if (qualifierSet.All(x => (x is AnyAttribute)))
        //    {
        //        qualifierSet.Add(DefaultAttributeInstance);
        //    }
        //    qualifierSet.Add(AnyAttributeInstance);

        //    var reg = new ClassComponent(component, qualifierSet);
        //    _components.Add(reg);
        //    return reg;
        //}

        //public void RegisterComponentInstance(object instance, params object[] qualifiers)
        //{
        //    _components.Add(new InstanceComponent(instance, instance.GetType(), qualifiers));
        //}

        //public void RegisterConfigurations(params Type[] configurations)
        //{
        //    foreach (var config in configurations)
        //    {
        //        ConfigurationCriteria.Validate(config);
        //    }
        //    foreach(var config in configurations)
        //        _configurations.Add(new ClassComponent(config, new object[]{AnyAttributeInstance, DefaultAttributeInstance}));
        //}

        //public void RegisterInject(FieldInfo field, params object[] qualifiers)
        //{
        //    InjectionValidator.Validate(field);
        //    _injectRegistrations.Add(new InjectRegistration(field, field.FieldType, qualifiers));
        //}

        //public void RegisterInject(MethodBase method, ResolveSpec[] spec)
        //{
        //    InjectionValidator.Validate(method);
        //    _injectRegistrations.Add(new InjectRegistration(method, spec));
        //}

        //public void RegisterInject(PropertyInfo property, object[] qualifiers)
        //{
        //    InjectionValidator.Validate(property);
        //    _injectRegistrations.Add(new InjectRegistration(property, property.PropertyType, qualifiers));
        //}

        //public void RegisterPostConstructs(params MethodInfo[] postConstructs)
        //{
        //    foreach (var post in postConstructs)
        //    {
        //        PostConstructCriteria.Validate(post);
        //        _postConstructs.Add(post);
        //    }
        //}

        //public IEnumerable<AbstractComponent> Configurations
        //{
        //    get { return _configurations; }
        //}

        //public IEnumerable<AbstractComponent> Components
        //{
        //    get { return _components; }
        //}

        //public IEnumerable<InjectRegistration> InjectRegistrations
        //{
        //    get { return _injectRegistrations;  }
        //}

        //public IEnumerable<MethodInfo> PostConstructs
        //{
        //    get { return _postConstructs; }
        //}

        public void AddComponent(IWeldComponent component)
        {
            _components.Add(component);
        }

        public void AddConfiguration(IWeldComponent component)
        {
            _configurations.Add(component);
        }
    }
}