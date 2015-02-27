using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ProducerProperty : AbstractProducer
    {
        private readonly PropertyInfo _property;

        public ProducerProperty(IWeldComponent component, PropertyInfo property, IAnnotations annotations, WeldComponentManager manager)
            : base(component, property, property.PropertyType, annotations,manager)
        {
            _property = property;
        }


        protected override AbstractProducer TranslateTypes(GenericResolver.Resolution resolution)
        {
            var resolvedProperty = GenericUtils.TranslatePropertyType(_property, resolution.GenericParameterTranslations);
            return new ProducerProperty(DeclaringComponent.Resolve(resolvedProperty.DeclaringType), resolvedProperty, Annotations, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return context =>
            {
                var containingObject = Manager.GetReference(DeclaringComponent, context);
                return _property.GetValue(containingObject);
            };
        }

        public override string ToString()
        {
            return string.Format("Producer Property [{0}] with Qualifiers [{1}]", _property, string.Join(",", Qualifiers));
        }
    }
}