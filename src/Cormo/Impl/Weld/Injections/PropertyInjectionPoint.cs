using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Injections
{
    public class PropertyInjectionPoint: AbstractInjectionPoint
    {
        private readonly PropertyInfo _property;

        public PropertyInjectionPoint(IComponent declaringComponent, PropertyInfo property, IBinders binders):
            base(declaringComponent, property, property.PropertyType, binders)
        {
            InjectionValidator.Validate(property);
            _property = property;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var property = GenericUtils.TranslatePropertyType(_property, translations);
            return new PropertyInjectionPoint(component, property, Binders);
        }

        protected override InjectPlan BuildInjectPlan(IComponent component)
        {
            return (target, context) =>
            {
                var value = GetValue(context);
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