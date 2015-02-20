using System;
using System.Threading.Tasks;
using Cormo.Catch;
using Cormo.Impl.Weld.Catch;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Impl.Solder
{
    [Interceptor, ExceptionsHandled]
    public class ExceptionsHandledInterceptor: IAroundInvokeInterceptor
    {
        private readonly IExceptionHandlerDispatcher _dispatcher;

        [Inject]
        public ExceptionsHandledInterceptor(IServiceRegistry services)
        {
            _dispatcher = services.GetService<IExceptionHandlerDispatcher>();
        }

        public async Task<object> AroundInvoke(IInvocationContext invocationContext)
        {
            try
            {
                return await invocationContext.Proceed();
            }
            catch (Exception e)
            {
                if (_dispatcher.Dispatch(invocationContext, e))
                    return null;

                throw;
            }
        }
    }

    
}