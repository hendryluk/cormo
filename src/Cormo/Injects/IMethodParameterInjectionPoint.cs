using System.Reflection;

namespace Cormo.Injects
{
    public interface IMethodParameterInjectionPoint: IInjectionPoint
    {
        ParameterInfo ParameterInfo { get; }
    }
}