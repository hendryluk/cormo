using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Cormo.Catch;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Data.EntityFramework.Api.Audits;
using Cormo.Data.EntityFramework.Api.Events;
using Cormo.Events;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    public class EntityAuditor
    {
        static readonly ConcurrentDictionary<Type, EntityInfo> _entityInfos = new ConcurrentDictionary<Type, EntityInfo>();
        public static EntityInfo GetEntityInfo(IComponentManager manager, Type type)
        {
            return _entityInfos.GetOrAdd(type, _ => new EntityInfo(manager, type));
        }

        public void SaveAudits([Observes]TransactionCompleting e, [Unwrap]DbContext dbContext, IComponentManager manager)
        {
            var addedAuditedEntities = dbContext.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Added)
                .Select(p => p.Entity);

            var modifiedAuditedEntities = dbContext.ChangeTracker.Entries()
              .Where(p => p.State == EntityState.Modified)
              .Select(p => p.Entity);

            foreach (var added in addedAuditedEntities)
            {
                var info = GetEntityInfo(manager, added.GetType());
                if (info.HasAudit)
                {
                    info.AuditCreation(added);
                    info.AuditModified(added);
                }
            }

            foreach (var modified in modifiedAuditedEntities)
            {
                var info = GetEntityInfo(manager, modified.GetType());
                if (info.HasModifyAudit)
                {
                    info.AuditCreation(modified);
                    info.AuditModified(modified);
                }
            }
        }
    }

    [Configuration, Singleton]
    public class EntityContextProducer: IEntityRegistrar
    {
        [Inject] IEvents<RegisteringEntities> _registeringEvents;
            
        [PostConstruct]
        public void Init()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/App_Data");
        }

        [Produces, RequestScoped, CurrentAuditor, ConditionalOnMissingComponent]
        public IPrincipal GetCurrentPrincipalAuditor(IPrincipal principal)
        {
            return principal;
        }

        [Produces, RequestScoped, CurrentAuditor, ConditionalOnMissingComponent]
        public string GetCurrentStringAuditor(IPrincipal principal)
        {
            if (principal == null || principal.Identity == null)
                return null;
            return principal.Identity.Name;
        }



        private static readonly ConcurrentBag<Type> _entityTypes = new ConcurrentBag<Type>();

        public void RegisterEntities(DbModelBuilder modelBuilder)
        {
            _registeringEvents.Fire(new RegisteringEntities(modelBuilder));
            foreach (var entity in _entityTypes)
                modelBuilder.RegisterEntityType(entity);    
        }

        [Singleton]
        public class EntityType<T> where T:class
        {
            static EntityType()
            {
                _entityTypes.Add(typeof (T));
            }

            [Produces, RequestScoped]
            public IDbSet<T> GetDbSet([Unwrap]DbContext context)
            {
                return context.Set<T>();
            }
        }

        //[Produces, RequestScoped, Default, EntityContext]
        //DbContext GetDbContext(DbContexts contexts, IInjectionPoint injectionPoint)
        //{
        //    return contexts.GetContext(injectionPoint);
        //}

        //[RequestScoped]
        //public class DbContexts: IDisposable
        //{
        //    [Inject] IComponentManager _manager;
        //    [Inject] IEntityRegistrar _registrar;

        //    [Produces, EntityContextName] string _currentContextName=null;

        //    private readonly ConcurrentDictionary<string, DbContext> _contexts = new ConcurrentDictionary<string, DbContext>(); 
        //    public virtual DbContext GetContext(IInjectionPoint injectionPoint)
        //    {
        //        var connectionName = GetConnectionName(injectionPoint);
        //        return _contexts.GetOrAdd(connectionName, _ => CreateDbContext(connectionName));

        //    }

        //    private DbContext CreateDbContext(string connectionName)
        //    {
        //        try
        //        {
        //            _currentContextName = connectionName;
        //            var component =_manager.GetComponent(typeof (DbContext), new[] {new EntityContextAttribute(connectionName)});
        //            return (DbContext) _manager.GetReference(component, _manager.CreateCreationalContext(component));
        //        }
        //        finally
        //        {
        //            _currentContextName = null;
        //        }
        //    }

        //    private string GetConnectionName(IInjectionPoint injectionPoint)
        //    {
        //        return injectionPoint.Qualifiers.OfType<EntityContextAttribute>()
        //            .Select(x => x.ConnectionName)
        //            .DefaultIfEmpty("DefaultConnection")
        //            .First();
        //    }

        //    public virtual void Dispose()
        //    {
        //        foreach (var context in _contexts.Values)
        //        {
        //            try
        //            {
        //                context.Dispose();
        //            }
        //            catch (Exception e)
        //            {
        //                // TODO log
        //            }
        //        }
        //    }
        //}
    }
}