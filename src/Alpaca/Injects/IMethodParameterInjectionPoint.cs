using System.Reflection;

namespace Alpaca.Injects
{
    public interface IMethodParameterInjectionPoint: IInjectionPoint
    {
        ParameterInfo ParameterInfo { get; }
    }
}