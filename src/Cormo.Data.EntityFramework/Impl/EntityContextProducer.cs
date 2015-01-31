using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    public class EntityContextProducer
    {
        [Produces] [RequestScoped] DbContext GetDbContext(IInjectionPoint injectionPoint)
        {
            return new DbContext(GetConnectionName(injectionPoint));
        }

        [Produces][RequestScoped] IDbSet<T> GetDbSet<T>(IInjectionPoint injectionPoint) where T : class
        {
            return GetDbContext(injectionPoint).Set<T>();
        }

        [Produces][RequestScoped][EntityContext] IQueryable<T> GetQueryable<T>(IInjectionPoint injectionPoint) where T : class
        {
            return GetDbSet<T>(injectionPoint);
        }

        private string GetConnectionName(IInjectionPoint injectionPoint)
        {
            return injectionPoint.Qualifiers.OfType<EntityContextAttribute>()
                .Select(x => x.ConnectionName)
                .DefaultIfEmpty("Default")
                .First();
        }
    }
}