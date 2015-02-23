using System.Data.Entity;

namespace Cormo.Data.EntityFramework.Api
{
    public interface IEntityRegistrar
    {
        void RegisterEntities(DbModelBuilder modelBuilder);
    }
}