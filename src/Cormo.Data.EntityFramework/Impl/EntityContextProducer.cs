using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Cormo.Contexts;
using Cormo.Data.EntityFramework.Api;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    [Configuration]
    public class EntityContextProducer
    {
        [Inject]
        public void Init()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/App_Data");
            }
        }

        static readonly ConcurrentDictionary<Type, EntityInfo> _entityInfos = new ConcurrentDictionary<Type, EntityInfo>(); 
        public static EntityInfo GetEntityInfo(IComponentManager manager, Type type)
        {
            return _entityInfos.GetOrAdd(type, _ => new EntityInfo(manager, type));
        }

        [RequestScoped]
        public class DbContexts
        {
            [Inject] IComponentManager _manager;
            private readonly ConcurrentDictionary<string, DbContext> _contexts = new ConcurrentDictionary<string, DbContext>(); 
            public virtual DbContext GetContext(IInjectionPoint injectionPoint)
            {
                var connectionName = GetConnectionName(injectionPoint);
                return _contexts.GetOrAdd(connectionName, new CormoDbContext(connectionName,
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