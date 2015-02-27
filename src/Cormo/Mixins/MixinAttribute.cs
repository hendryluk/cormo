using System;
using Cormo.Injects;

namespace Cormo.Mixins
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MixinAttribute: Attribute, IAnnotation
    {
        public Type[] InterfaceTypes { get; private set; }

        public MixinAttribute(params Type[] interfaceTypes)
        {
            InterfaceTypes = interfaceTypes;
        }
    }
}