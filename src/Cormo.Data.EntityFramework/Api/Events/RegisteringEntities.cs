using System.Data.Entity;

namespace Cormo.Data.EntityFramework.Api.Events
{
    public class RegisteringEntities
    {
        public RegisteringEntities(DbModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
        }

        public DbModelBuilder ModelBuilder { get; private set; } 
    }
}