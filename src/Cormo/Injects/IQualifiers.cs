using System;
using System.Collections.Generic;
using Cormo.Impl.Weld.Resolutions;

namespace Cormo.Injects
{
    public interface IQualifiers: IEnumerable<IQualifier>
    {
        Type[] Types { get; }
        bool CanSatisfy(IQualifiers qualifiers);
    }
}