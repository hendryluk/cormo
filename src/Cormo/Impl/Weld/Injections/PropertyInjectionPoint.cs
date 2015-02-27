using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;
using Cormo.Reflects;

namespace Cormo.Impl.Weld.Injections
{
    public class PropertyInjectionPoint: AbstractInjectionPoint
    {
        private readonly PropertyInfo _property;

        public PropertyInjectionPoint(IComponent declaringComponent, IAnnotatedProperty property):
            this(declaringComponent, property.Property, property.Annotations)
        {
            InjectionValidator.Validate(property);
        }

        private PropertyInjectionPoint(IComponent declaringComponent, PropertyInfo property, IAnnotations annotations) :
            base(declaringComponent, property, property.PropertyType, annotations)
        {
            _property = property;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            if (DeclaringComponent.Type == component.Type)
                return this;

            var property = GenericUtils.TranslatePropertyType(_property, translations);
            return new PropertyInjectionPoint(component, property, Annotations);
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