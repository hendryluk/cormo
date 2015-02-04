using System.Collections.Generic;

namespace Cormo.Injects
{
    public interface IQualifiers: IEnumerable<IQualifier>
    {
        bool CanSatisfy(IQualifiers qualifiers);
    }
}