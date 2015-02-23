using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cormo.Catch;
using Cormo.Data.EntityFramework.Api;
using Cormo.Data.EntityFramework.Api.Events;
using Cormo.Events;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Impl
{
    [EntityContext, ConditionalOnMissingComponent]
    public class CormoDbContext : DbContext
    {
        private readonly IEntityRegistrar _entityRegistrar;
        
        [Inject]
        public CormoDbContext(IEntityRegistrar entityRegistrar)
        {
            _entityRegistrar = entityRegistrar;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            _entityRegistrar.RegisterEntities(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
    }
}