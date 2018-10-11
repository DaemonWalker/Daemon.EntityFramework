using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class DataOperator
    {
        protected DataBase DataBase { get { return DefSettings.DataBase; } }
        public DefSettings DefSettings { get; set; }
        public virtual List<T> QueryObject<T>(string sql)
        {
            Console.WriteLine(sql);
            var list = new List<T>();


            var comm = DataBase.GetCommand(false);
            comm.CommandText = sql;
            var reader = comm.ExecuteReader();


            if (typeof(T).GetConstructors().Where(p => p.GetParameters().Length == 0).Count() > 0)
            {
                var fieldNames = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    fieldNames.Add(reader.GetName(i));
                }
                var props = new Dictionary<string, PropertyInfo>();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (prop.CanWrite)
                    {
                        props.Add(prop.Name, prop);
                    }
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
            }
            else
            {
                var type = typeof(T);
                var constructor = type.GetConstructors()
                    .Where(p => p.GetParameters().Count() == type.GetProperties().Count())
                    .First();

                if (constructor == null)
                {
                    throw new Exception("no constructor defined");
                }
                var buildInfo = constructor
                    .GetParameters()
                    .Select(p => new KeyValuePair<string, Type>(p.Name, p.ParameterType));
                var dict = new Dictionary<string, int>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict.Add(reader.GetName(i), i);
                }
                while (reader.Read())
                {
                    var args = new List<object>();
                    foreach (var info in buildInfo)
                    {
                        if (dict.ContainsKey(info.Key))
                        {
                            args.Add(Convert.ChangeType(reader.GetValue(dict[info.Key]), info.Value));
                        }
                        else
                        {
                            args.Add(Activator.CreateInstance(info.Value));
                        }
                    }
                    var t = (T)constructor.Invoke(args.ToArray());
                    list.Add(t);
                }
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
