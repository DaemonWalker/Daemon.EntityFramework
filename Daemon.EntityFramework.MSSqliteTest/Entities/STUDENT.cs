using Daemon.EntityFramework.Core.Attrbutes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    class STUDENT
    {
        [PrimaryKey]
        public int STUDENT_ID { get; set; }
        public int CLASS_ID { get; set; }
        public string STUDENT_NAME { get; set; }
    }
}
