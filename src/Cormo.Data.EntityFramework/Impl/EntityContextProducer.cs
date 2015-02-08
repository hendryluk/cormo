using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Data.EntityFramework.Api.Audits;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    [Configuration, Singleton]
    public class EntityContextProducer
    {
        [PostConstruct]
        public void Init()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/App_Data");
        }

        static readonly ConcurrentDictionary<Type, EntityInfo> _entityInfos = new ConcurrentDictionary<Type, EntityInfo>(); 
        public static EntityInfo GetEntityInfo(IComponentManager manager, Type type)
        {
            return _entityInfos.GetOrAdd(type, _ => new EntityInfo(manager, type));
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

        [RequestScoped]
        public class DbContexts
        {
            [Inject] IComponentManager _manager;
            private readonly ConcurrentDictionary<string, DbContext> _contexts = new ConcurrentDictionary<string, DbContext>(); 
            public virtual DbContext GetContext(IInjectionPoint injectionPoint)
            {
                var connectionName = GetConnectionName(injectionPoint);
                return _contexts.GetOrAdd(connectionName, _=> new CormoDbContext(connectionName,
                    _entityTypes.Select(x => GetEntityInfo(_manager, x)).ToArray()));
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
        
        [Singleton]
        public class EntityType<T> where T:class
        {
            static EntityType()
            {
                _entityTypes.Add(typeof (T));
            }

            [Produces, RequestScoped, Default, EntityContext]
            public IDbSet<T> GetDbSet(DbContexts contexts, IInjectionPoint injectionPoint)
            {
                return contexts.GetContext(injectionPoint).Set<T>();
            }
        }

        [Produces, RequestScoped, Default, EntityContext]
        DbContext GetDbContext(DbContexts contexts, IInjectionPoint injectionPoint)
        {
            return contexts.GetContext(injectionPoint);
        }

        
    }
}