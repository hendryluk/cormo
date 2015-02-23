using System.Security.Cryptography.X509Certificates;

namespace Cormo.Injects
{
    public interface IValueProvider
    {
        string GetValue(string key);
        int Priority { get; }
    }
}