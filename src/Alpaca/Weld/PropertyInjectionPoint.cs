using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld
{
    public class PropertyInjectionPoint: AbstractInjectionPoint
    {
        private readonly PropertyInfo _property;

        public PropertyInjectionPoint(IComponent declaringComponent, PropertyInfo property, QualifierAttribute[] qualifiers):
            base(declaringComponent, property, property.PropertyType, qualifiers)
        {
            InjectionValidator.Validate(property);
            _property = property;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var property = GenericUtils.TranslatePropertyType(_property, translations);
            return new PropertyInjectionPoint(component, property, Qualifiers.ToArray());
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            var manager = DeclaringComponent.Manager;
            if (IsCacheable)
            {
                var instance = manager.GetReference(component);
                return target => SetValue(target, instance);
            }

            return target =>
            {
                var instance = manager.GetReference(component);
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
}