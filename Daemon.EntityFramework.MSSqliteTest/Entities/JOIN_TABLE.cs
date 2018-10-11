using Daemon.EntityFramework.Core.Attrbutes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    public class JOIN_TABLE
    {
        [PrimaryKey]
        public int JOIN_ID { get; set; }

        public int TEST_ID { get; set; }
        public string NAME { get; set; }
    }
}
