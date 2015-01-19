using System;
using System.Collections.Generic;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Component
{
    public class ProducerProperty : AbstractProducer
    {
        private readonly PropertyInfo _property;

        public ProducerProperty(PropertyInfo property, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager)
            : base(property, property.PropertyType, qualifiers, scope, manager)
        {
            _property = property;
        }


        protected override AbstractProducer TranslateTypes(GenericUtils.Resolution resolution)
        {
            var resolvedProperty = GenericUtils.TranslatePropertyType(_property, resolution.GenericParameterTranslations);
            return new ProducerProperty(resolvedProperty, Qualifiers, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return () =>
            {
                var containingObject = Manager.GetReference(DeclaringComponent);
                return _property.GetValue(containingObject);
            };
        }

        public override string ToString()
        {
            return string.Format("Producer Property [{0}] with Qualifiers [{1}]", _property, string.Join(",", Qualifiers));
        }
    }
}