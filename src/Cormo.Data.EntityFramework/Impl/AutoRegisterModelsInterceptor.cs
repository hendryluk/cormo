using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cormo.Data.EntityFramework.Api;
using Cormo.Impl.Weld;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Data.EntityFramework.Impl
{
    [Interceptor(AllowPartialInterception=true), AutoRegisterModels]
    public class AutoRegisterModelsInterceptor : IAroundInvokeInterceptor
    {
        [Inject] IEntityRegistrar _entityRegistrar;
        
        static readonly MethodInfo OnModelCreatingMethod = typeof(DbContext).GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic).GetBaseDefinition();
        public Task<object> AroundInvoke(IInvocationContext invocationContext)
        {
            if (OnModelCreatingMethod == invocationContext.Method.GetBaseDefinition())
            {
                _entityRegistrar.RegisterEntities((DbModelBuilder) invocationContext.Arguments[0]);
            }

            return invocationContext.Proceed();
        }
    }
}