namespace Cormo.Interceptions
{
    public interface IAroundInvokeInterceptor
    {
        object AroundInvoke(IInvocationContext invocationContext);
    }
}