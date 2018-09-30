using Daemon.EntityFramework.Core;
using Daemon.EntityFramework.MSSqliteTest.Sqlite;
using System;
using System.Linq;

namespace Daemon.EntityFramework.MSSqliteTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DefSettings.DataBaseType = typeof(SqliteDataBase);
            DefSettings.EntityDBConvertType = typeof(SqliteEntityDBConvert);

            using (var db = new SqliteDbContext())
            {
                Console.WriteLine(db.Where(p => p.TEST_ID > 1000).Count());
            }
        }
    }
}
