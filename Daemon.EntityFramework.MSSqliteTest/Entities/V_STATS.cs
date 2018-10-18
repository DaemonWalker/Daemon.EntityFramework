using Daemon.EntityFramework.Core.Attributes;
using Daemon.EntityFramework.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    [EntityType(EntityType.View)]
    class V_STATS
    {
        public string STUDENT_NAME { get; set; }
        public string CLASS_NAME { get; set; }
        public string SUBJECT_NAME { get; set; }
        public int POINTS { get; set; }
    }
}
