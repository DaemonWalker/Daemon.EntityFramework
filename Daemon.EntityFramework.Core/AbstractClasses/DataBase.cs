using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class DataBase : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        protected DbConnection connection;

        /// <summary>
        /// 设置信息
        /// </summary>
        public DefSettings DefSettings { get; set; }

        /// <summary>
        /// IDisposable接口实现
        /// </summary>
        public void Dispose()
        {
            this.connection?.Close();
        }

        /// <summary>
        /// 获取数据库命令对象
        /// </summary>
        /// <param name="openTranscation"></param>
        /// <returns></returns>
        public abstract DbCommand GetCommand(bool openTranscation);

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public abstract DbConnection GetConnection();

        /// <summary>
        /// 获取数据适配器
        /// </summary>
        /// <returns></returns>
        public abstract DbDataAdapter GetDataAdapter();

        /// <summary>
        /// 获取数据读取对象
        /// </summary>
        /// <returns></returns>
        public abstract DbDataReader GetDataReader();
    }
}
