using Daemon.EntityFramework.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class EntityTypeAttribute : Attribute
    {
        public EntityType EntityType { get; private set; }
        public EntityTypeAttribute(EntityType entityType)
        {
            this.EntityType = entityType;
        }
    }
}
