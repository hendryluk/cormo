using System;
using System.Collections.Generic;

namespace Cormo.Injects
{
    public interface IAnnotations : IEnumerable<IAnnotation>
    {
        IQualifiers Qualifiers { get; }
        Type[] Types { get; }

        bool Any<T>();
    }
}