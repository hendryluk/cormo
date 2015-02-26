using System;
using System.Collections.Generic;
using System.Reflection;
using Cormo.Impl.Weld.Utils;
using Cormo.Injects;

namespace Cormo.Impl.Weld.Components
{
    public class ProducerField : AbstractProducer
    {
        private readonly FieldInfo _field;

        public ProducerField(IWeldComponent component, FieldInfo field, WeldComponentManager manager)
            : base(component, field, field.FieldType, manager)
        {
            _field = field;
        }


        protected override AbstractProducer TranslateTypes(GenericResolver.Resolution resolution)
        {
            var resolvedField = GenericUtils.TranslateFieldType(_field, resolution.GenericParameterTranslations);
            return new ProducerField(DeclaringComponent.Resolve(resolvedField.DeclaringType), resolvedField, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return context =>
            {
                var containingObject = Manager.GetReference(DeclaringComponent, context);
                return _field.GetValue(containingObject);
            };
        }

        public override string ToString()
        {
            return string.Format("Producer Field [{0}] with Qualifiers [{1}]", _field, string.Join(",", Qualifiers));
        }
    }
}