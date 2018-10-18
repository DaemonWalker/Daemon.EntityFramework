using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Sqlite
{
    class SqliteDataBase : Daemon.EntityFramework.Core.AbstractClasses.DataBase
    {
        public string ConnStr { get; set; } = "data source=db.db";
        public override DbCommand GetCommand(bool openTranscation)
        {
            this.connection = this.GetConnection();
            var comm = this.connection.CreateCommand();
            if (openTranscation)
            {
                var trans = this.connection.BeginTransaction();
                comm.Transaction = trans;
            }
            return comm;
        }

        public override DbConnection GetConnection()
        {
            this.connection = new SqliteConnection(ConnStr);
            SQLitePCL.Batteries.Init();
            this.connection.Open();
            return this.connection;
        }

        public override DbDataAdapter GetDataAdapter()
        {
            throw new NotImplementedException();
        }

        public override DbDataReader GetDataReader()
        {
            throw new NotImplementedException();
        }
    }
}
