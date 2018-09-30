using Daemon.EntityFramework.Core.Attrbutes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    public class TEST_TABLE
    {
        [PrimaryKey]
        public int TEST_ID { get; set; }

        public string NAME { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_OPER { get; set; }
        public string VALID_STATE { get; set; }
        public string TEL { get; set; }
        public string SEX { get; set; }
        public int AGE { get; set; }
        public string ADDRESS { get; set; }
    }
}
