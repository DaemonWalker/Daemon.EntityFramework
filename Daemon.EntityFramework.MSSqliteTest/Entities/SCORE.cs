using Daemon.EntityFramework.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Entities
{
    class SCORE
    {
        [PrimaryKey]
        public int SCORE_ID { get; set; }
        public int STUDENT_ID { get; set; }
        public int POINTS { get; set; }
        public int SUBJECT_ID { get; set; }
    }
}
