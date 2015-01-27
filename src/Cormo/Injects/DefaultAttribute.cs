namespace Cormo.Injects
{
    public sealed class DefaultAttribute: QualifierAttribute
    {
        public static readonly DefaultAttribute Instance = new DefaultAttribute();
    }
}