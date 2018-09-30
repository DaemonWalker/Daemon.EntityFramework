using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class DataBase : IDisposable
    {
        protected DbConnection connection;
        public abstract DbConnection GetConnection();
        public abstract DbDataReader GetDataReader();
        public abstract DbCommand GetCommand(bool openTranscation);
        public abstract DbDataAdapter GetDataAdapter();

        public void Dispose()
        {
            this.connection?.Close();
        }
    }
}
