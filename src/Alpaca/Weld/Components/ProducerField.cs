using System;
using System.Collections.Generic;
using System.Reflection;
using Alpaca.Injects;
using Alpaca.Injects;
using Alpaca.Weld.Utils;

namespace Alpaca.Weld.Components
{
    public class ProducerField : AbstractProducer
    {
        private readonly FieldInfo _field;

        public ProducerField(FieldInfo field, IEnumerable<QualifierAttribute> qualifiers, Type scope, IComponentManager manager)
            : base(field, field.FieldType, qualifiers, scope, manager)
        {
            _field = field;
        }


        protected override AbstractProducer TranslateTypes(GenericUtils.Resolution resolution)
        {
            var resolvedField = GenericUtils.TranslateFieldType(_field, resolution.GenericParameterTranslations);
            return new ProducerField(resolvedField, Qualifiers, Scope, Manager);
        }

        protected override BuildPlan GetBuildPlan()
        {
            return () =>
            {
                var containingObject = Manager.GetReference(DeclaringComponent);
                return _field.GetValue(containingObject);
            };
        }

        public override string ToString()
        {
            return string.Format("Producer Field [{0}] with Qualifiers [{1}]", _field, string.Join(",", Qualifiers));
        }
    }
}