using Daemon.EntityFramework.Core.Attrbutes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    class SUBJECT
    {
        [PrimaryKey]
        public int SUBJECT_ID { get; set; }

        public string SUBJECT_NAME { get; set; }
    }
}
