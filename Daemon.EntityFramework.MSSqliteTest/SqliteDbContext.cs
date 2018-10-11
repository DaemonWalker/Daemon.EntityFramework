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
        public DBTable<TEST_TABLE> TestTable { get; set; }
        public DBTable<JOIN_TABLE> JoinTable { get; set; }
        public SqliteDbContext() : base(
            new DefSettings()
            {
                DataBaseType = typeof(SqliteDataBase),
                EntityDBConvertType = typeof(SqliteEntityDBConvert)
            })
        { }
    }
}
