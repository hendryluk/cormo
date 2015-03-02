using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Cormo.Data.EntityFramework.Api;
using Cormo.Data.EntityFramework.Api.Events;
using Cormo.Events;
using Cormo.Injects;
using Cormo.Interceptions;

namespace Cormo.Data.EntityFramework.Impl
{
    [Interceptor, Transactional]
    public class TransactionalInterceptor: IAroundInvokeInterceptor
    {
        [Inject] IInstance<DbContext> _dbContext;
        [Inject] IEvents<TransactionCompleting> _completingEvents; 
        [Inject] IEvents<TransactionComitted> _successfulEvents; 

        public async Task<object> AroundInvoke(IInvocationContext invocationContext)
        {
            // May consider TransactionScope because EF one really sucks at nesting
            // But TransactionScope is not good for async before .NET 4.5.1.
            // May consider for future version
            var dbContext = _dbContext.Value;
            if (dbContext.Database.CurrentTransaction != null)
            {
                var result = await invocationContext.Proceed();
                await _completingEvents.FireAsync(new TransactionCompleting());
                dbContext.SaveChanges();
                return result;
            }
                
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var result = await invocationContext.Proceed();
                    await _completingEvents.FireAsync(new TransactionCompleting());
                    dbContext.SaveChanges();
                    transaction.Commit();
                    await _successfulEvents.FireAsync(new TransactionComitted());
                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}