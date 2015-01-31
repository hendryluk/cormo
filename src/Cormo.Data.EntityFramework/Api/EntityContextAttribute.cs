using System;
using Cormo.Injects;

namespace Cormo.Data.EntityFramework.Api
{
    public class EntityContextAttribute : QualifierAttribute
    {
        public string ConnectionName { get; private set; }

        public EntityContextAttribute(): this("Default")
        {
        }

        private EntityContextAttribute(string connectionName)
        {
            ConnectionName = connectionName;
        }
    }
}