using System.Data.Entity;

namespace Cormo.Data.EntityFramework.Api.Events
{
    public class ModelCreating
    {
        public ModelCreating(DbModelBuilder modelBuilder)
        {
            ModelBuilder = modelBuilder;
        }

        public DbModelBuilder ModelBuilder { get; private set; } 
    }
}