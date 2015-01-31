using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Injections
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
           return (target, context, ip) =>
           {
               var value = GetValue(context, ip);
               return SetValue(target, value);
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