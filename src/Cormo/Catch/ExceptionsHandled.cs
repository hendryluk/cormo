using System;
using System.Runtime.ExceptionServices;
using Cormo.Impl.Weld.Catch;
using Cormo.Injects;

namespace Cormo.Catch
{
    public static class ExceptionsHandled
    {
        private static IExceptionHandlerDispatcher _dispatcher;

        [Configuration]
        internal class Configurator
        {
            [Inject]
            public void Init(IServiceRegistry registry)
            {
                _dispatcher = registry.GetService<IExceptionHandlerDispatcher>();
            }
        }
        
        public static void Throw(Exception e, params IQualifier[] qualifiers)
        {
            if (!_dispatcher.Dispatch(null, e, qualifiers))
            {
                throw (e);
            }
        }
    }
}