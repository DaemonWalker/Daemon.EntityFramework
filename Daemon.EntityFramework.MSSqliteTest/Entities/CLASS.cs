using Daemon.EntityFramework.Core.Attrbutes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    class CLASS
    {
        [PrimaryKey]
        public int CLASS_ID { get; set; }
        public string CLASS_NAME { get; set; }
    }
}
