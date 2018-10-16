using Daemon.EntityFramework.Core;
using Daemon.EntityFramework.MSSqliteTest.Entities;
using Daemon.EntityFramework.MSSqliteTest.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest
{
    class SqliteDbContext : DBContext
    {
        public DBTable<SCORE> Score { get; set; }
        public DBTable<CLASS> Class { get; set; }
        public DBTable<STUDENT> Student { get; set; }
        public DBTable<SUBJECT> Subject { get; set; }
        public SqliteDbContext() : base(
            new DefSettings()
            {
                DataBaseType = typeof(SqliteDataBase),
                EntityDBConvertType = typeof(SqliteEntityDBConvert),
                OutputSql = true
            })
        { }
    }
}
