using System;

namespace Alpaca.Injects
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ImportAttribute: Attribute
    {
        private readonly Type[] _types;

        public ImportAttribute(params Type[] types)
        {
            _types = types;
        }

        public Type[] Types
        {
            get { return _types; }
        }
    }
}