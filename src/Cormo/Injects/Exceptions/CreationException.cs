using System;
using Cormo.Injects.Exceptions;

namespace Cormo.Injects.Exceptions
{
    public class CreationException: InjectionException
    {
        public CreationException(Type type, string reason)
            : base(string.Format("Type cannot be instantiated: [{0}]. Reason: {1}", type.FullName, reason))
        {
        }

        public CreationException(Type type, Exception innerException)
            : base(string.Format("Type cannot be instantiated: [{0}]. ", type.FullName), innerException)
        {
        }
    }
}