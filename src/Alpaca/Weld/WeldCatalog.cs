using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Inject;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public class WeldCatalog
    {
        private readonly List<ComponentRegistration> _components = new List<ComponentRegistration>();
        private readonly List<ComponentRegistration> _configurations = new List<ComponentRegistration>();
        private readonly List<InjectRegistration> _injectRegistrations = new List<InjectRegistration>();
        private readonly List<MethodInfo> _postConstructs = new List<MethodInfo>();

        private static readonly DefaultAttribute DefaultAttributeInstance = new DefaultAttribute();
        private static readonly AnyAttribute AnyAttributeInstance = new AnyAttribute();
        public ComponentRegistration RegisterComponent(Type component, params object[] qualifiers)
        {
            ComponentCriteria.Validate(component);

            var qualifierSet = new HashSet<object>(qualifiers);
            if (qualifierSet.All(x => (x is AnyAttribute)))
            {
                qualifierSet.Add(DefaultAttributeInstance);
            }
            qualifierSet.Add(AnyAttributeInstance);

            var reg = new ClassComponentRegistration(component, qualifierSet);
            _components.Add(reg);
            return reg;
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

        public void RegisterInject(FieldInfo field, params object[] qualifiers)
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
}