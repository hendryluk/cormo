using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Cormo.Contexts;
using Cormo.Impl.Weld.Components;
using Cormo.Interceptions;

namespace Cormo.Impl.Weld.Interceptions
{
    public class InterceptorMethodHandler
    {
        private readonly LinkedList<IAroundInvokeInterceptor> _interceptorReferences;
        private readonly bool _isAsync;
        private readonly ITaskCaster _taskCaster;

        public InterceptorMethodHandler(WeldComponentManager manager, MethodInfo method, Interceptor[] interceptors, ICreationalContext creationalContext)
        {
            _interceptorReferences = new LinkedList<IAroundInvokeInterceptor>(
                interceptors.Select(x => manager.GetReference(x, creationalContext))
                    .Cast<IAroundInvokeInterceptor>());

            var returnType = method.ReturnType;
            if (returnType == typeof (Task))
            {
                _isAsync = true;
                _taskCaster = null;
            }
            else if (returnType.IsGenericParameter)
            {
                var genericType = returnType.GetGenericTypeDefinition();
                if (genericType == typeof (Task<>))
                {
                    _isAsync = true;
                    _taskCaster = TaskCasters.ForType(returnType.GetGenericArguments()[0]);
                }
            }
        }

        public void Invoke(IInvocation castleInvocation)
        {
            var result = new InvocationContext(castleInvocation, _interceptorReferences.First, _isAsync, _taskCaster).Proceed();

            if (_isAsync)
            {
                if (_taskCaster == null)
                    castleInvocation.ReturnValue = result;
                else
                    castleInvocation.ReturnValue = _taskCaster.Cast(result);
            }
            else
            {
                try
                {
                    result.Wait();
                    castleInvocation.ReturnValue = result.Result;
                }
                catch (AggregateException e)
                {
                    ExceptionDispatchInfo.Capture(e.GetBaseException()).Throw();
                }
            }
        }
    }
}