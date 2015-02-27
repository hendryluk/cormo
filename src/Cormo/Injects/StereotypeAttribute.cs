using System;
using System.Collections.Generic;
using System.Linq;

namespace Cormo.Injects
{
    public abstract class StereotypeAttribute: Attribute, IAnnotation, IStereotype
    {
        private readonly Attribute[] _attributes;
        IEnumerable<Attribute> IStereotype.Attributes { get { return _attributes.Union(GetType().GetCustomAttributes(true).OfType<Attribute>()); } }

        protected StereotypeAttribute(): this(new Attribute[0])
        {
        }

        protected StereotypeAttribute(params Attribute[] attributes)
        {
            _attributes = attributes;
        }
    }

    public interface IStereotype
    {
        IEnumerable<Attribute> Attributes { get; }
    }
}