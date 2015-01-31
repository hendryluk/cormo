using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    [Configuration]
    public class EntityContextProducer
    {
        [PostConstruct]
        public void Init()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/App_Data");
            }
        }

        [RequestScoped]
        public class DbContexts
        {
            private readonly ConcurrentDictionary<string, DbContext> _contexts = new ConcurrentDictionary<string, DbContext>(); 
            public virtual DbContext GetContext(IInjectionPoint injectionPoint)
            {
                var connectionName = GetConnectionName(injectionPoint);
                return _contexts.GetOrAdd(connectionName, new CormoDbContext(connectionName));
            }

            private string GetConnectionName(IInjectionPoint injectionPoint)
            {
                return injectionPoint.Qualifiers.OfType<EntityContextAttribute>()
                    .Select(x => x.ConnectionName)
                    .DefaultIfEmpty("Default")
                    .First();
            }
        }

        private static readonly ConcurrentBag<Type> _entityTypes = new ConcurrentBag<Type>(); 
        public class EntityType<T> where T:class
        {
            static EntityType()
            {
                _entityTypes.Add(typeof (T));
            }

            [Produces, RequestScoped, EntityContext]
            public IDbSet<T> GetDbSet(DbContexts contexts, IInjectionPoint injectionPoint)
            {
                return contexts.GetContext(injectionPoint).Set<T>();
            }
        }

        public class CormoDbContext : DbContext
        {
            public CormoDbContext(string connectionName): base(connectionName)
            {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                foreach (var type in _entityTypes)
                    modelBuilder.RegisterEntityType(type);

                base.OnModelCreating(modelBuilder);
            }
        }

        [Produces, RequestScoped, EntityContext]
        DbContext GetDbContext(DbContexts contexts, IInjectionPoint injectionPoint)
        {
            return contexts.GetContext(injectionPoint);
        }

        
    }

    
}