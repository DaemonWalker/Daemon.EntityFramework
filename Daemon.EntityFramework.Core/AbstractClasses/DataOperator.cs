using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class DataOperator
    {
        public DataOperator()
        {
            this.DataBase = DefSettings.DataBase;
        }
        protected DataBase DataBase { get; set; }
        public virtual List<T> QueryObject<T>(string sql)
        {
            var list = new List<T>();
            var props = new Dictionary<string, PropertyInfo>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.CanWrite)
                {
                    props.Add(prop.Name, prop);
                }
            }

            var comm = DataBase.GetCommand(false);
            comm.CommandText = sql;
            var reader = comm.ExecuteReader();

            var fieldNames = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                fieldNames.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                var t = Activator.CreateInstance<T>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var obj = reader.GetValue(i);
                    if (props.ContainsKey(fieldNames[i]))
                    {
                        props[fieldNames[i]].SetValue(t, Convert.ChangeType(obj, props[fieldNames[i]].PropertyType));
                    }
                }
                list.Add(t);
            }
            return list;
        }
        public virtual DataTable QuerySql(string sql)
        {
            throw new NotImplementedException();
        }
        public virtual int ExecuteNonQuery(Func<DbCommand, int> executeFunc)
        {
            var conn = DataBase.GetConnection();
            var trans = conn.BeginTransaction();
            var comm = conn.CreateCommand();
            comm.Transaction = trans;
            var result = 0;
            try
            {
                result = executeFunc(comm);
                trans.Commit();
                return result;
            }
            catch (Exception e)
            {
                trans.Rollback();
                throw e;
            }
        }
        public virtual object ExecuteSclar(string sql)
        {
            var comm = this.DataBase.GetCommand(false);
            comm.CommandText = sql;
            return comm.ExecuteScalar();
        }
    }
}
