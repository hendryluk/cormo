using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Cormo.Data.EntityFramework.Impl
{
    public class CormoDbContext : DbContext
    {
        private readonly EntityInfo[] _entities;

        public CormoDbContext(string connectionName, EntityInfo[] entities)
            : base(connectionName)
        {
            _entities = entities;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            foreach (var entity in _entities)
                modelBuilder.RegisterEntityType(entity.Type);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SaveAudits();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            SaveAudits();
            return base.SaveChangesAsync();
        }

        private void SaveAudits()
        {
            var audits = _entities.Where(x => x.HasAudit).ToArray();
            var auditModifies = _entities.Where(x => x.HasModifyAudit).ToArray();

            var addedAuditedEntities = ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Added)
                .Select(p => p.Entity);
            
            var modifiedAuditedEntities = ChangeTracker.Entries()
              .Where(p => p.State == EntityState.Modified)
              .Select(p => p.Entity);

            foreach (var added in addedAuditedEntities)
                foreach (var a in audits.Where(a => a.Type.IsInstanceOfType(added)))
                {
                    a.AuditCreation(added);
                    a.AuditModified(added);
                }
            
            foreach (var modified in modifiedAuditedEntities)
                foreach (var a in auditModifies.Where(a => a.Type.IsInstanceOfType(modified)))
                {
                    a.AuditModified(modified);
                }
        }
    }
}