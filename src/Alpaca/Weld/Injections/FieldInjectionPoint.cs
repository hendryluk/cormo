using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alpaca.Inject;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Injections
{
    public class FieldInjectionPoint : AbstractInjectionPoint
    {
        private readonly FieldInfo _field;

        public FieldInjectionPoint(IComponent declaringComponent, FieldInfo field, QualifierAttribute[] qualifiers) :
            base(declaringComponent, field, field.FieldType, qualifiers)
        {
            InjectionValidator.Validate(field);
            _field = field;
        }

        public override IWeldInjetionPoint TranslateGenericArguments(IComponent component, IDictionary<Type, Type> translations)
        {
            var field = GenericUtils.TranslateFieldType(_field, translations);
            return new FieldInjectionPoint(component, field, Qualifiers.ToArray());
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
            _field.SetValue(target, instance);
            return instance;
        }

        public override string ToString()
        {
            // TODO prettify
            return _field.ToString();
        }
    }
}