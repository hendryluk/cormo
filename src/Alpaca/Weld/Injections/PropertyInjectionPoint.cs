using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Injections
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
            return (target, context, ip) =>
            {
                var value = GetValue(context, ip);
                return SetValue(target, value);
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