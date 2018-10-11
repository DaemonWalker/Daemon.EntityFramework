using Daemon.EntityFramework.Core.SqlFormatter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class DataOperator
    {
        /// <summary>
        /// sql美化器
        /// </summary>
        private IFormatter sqlFormatter = new BasicFormatter();

        /// <summary>
        /// 设置信息记录实体
        /// </summary>
        public DefSettings DefSettings { get; set; }

        /// <summary>
        /// 后端数据库
        /// </summary>
        protected DataBase DataBase { get { return DefSettings.DataBase; } }

        public virtual int _ExecuteNonQuery(Func<DbCommand, int> executeFunc)
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

        /// <summary>
        /// 数据库操作方法
        /// </summary>
        /// <param name="executeFunc">执行方法</param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(Func<DbCommand, int> executeFunc)
        {

            return _ExecuteNonQuery(executeFunc);
        }
        /// <summary>
        /// 返回查询首个值
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteSclar(string sql)
        {
            OutputLog(sql);
            return _ExecuteSclar(sql);
        }

        /// <summary>
        /// Sql语句转换为List<实体>
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql)
        {
            OutputLog(sql);
            return this._Query<T>(sql);
        }

        /// <summary>
        /// 直接执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual DataTable QuerySql(string sql)
        {
            throw new NotImplementedException();
        }

        protected virtual object _ExecuteSclar(string sql)
        {
            var comm = this.DataBase.GetCommand(false);
            comm.CommandText = sql;
            return comm.ExecuteScalar();
        }
        /// <summary>
        /// Sql语句转换为List<实体>
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        protected virtual List<T> _Query<T>(string sql)
        {
            //创建返回值
            var list = new List<T>();

            //创建数据库连接
            var comm = DataBase.GetCommand(false);
            comm.CommandText = sql;
            var reader = comm.ExecuteReader();

            //判断是否有无参构造函数
            //如果有可以直接使用Activator.CreateInstance创建对象
            if (typeof(T).GetConstructors().Where(p => p.GetParameters().Length == 0).Count() > 0)
            {
                //数据库返回列集合
                var fieldNames = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    fieldNames.Add(reader.GetName(i));
                }

                //实体属性集合
                var props = new Dictionary<string, PropertyInfo>();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (prop.CanWrite)
                    {
                        props.Add(prop.Name, prop);
                    }
                }

                //从数据库读取数据生成实体，加入到返回集合中
                while (reader.Read())
                {
                    var t = Activator.CreateInstance<T>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var obj = reader.GetValue(i);
                        //如果实体属性中有该列 则进行赋值
                        if (props.ContainsKey(fieldNames[i]))
                        {
                            props[fieldNames[i]].SetValue(t, Convert.ChangeType(obj, props[fieldNames[i]].PropertyType));
                        }
                    }
                    list.Add(t);
                }
            }
            //没有无参构造函数
            //一般是匿名类，手工创建的实体不建议没有无参构造函数
            else
            {
                //找到有所有属性的构造函数
                var type = typeof(T);
                var constructor = type.GetConstructors()
                    .Where(p => p.GetParameters().Count() == type.GetProperties().Count())
                    .First();

                //这个也没有老子不玩了
                //其实也可以，加不加看心情:)
                if (constructor == null)
                {
                    throw new Exception("no constructor defined");
                }

                //属性->类型的对照集合
                var buildInfo = constructor
                    .GetParameters()
                    .Select(p => new KeyValuePair<string, Type>(p.Name, p.ParameterType));

                //列名所在位置的对照字典
                var dict = new Dictionary<string, int>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dict.Add(reader.GetName(i), i);
                }

                //读取数据
                while (reader.Read())
                {
                    //将所需数据拼成object数组
                    var args = new List<object>();
                    foreach (var info in buildInfo)
                    {
                        if (dict.ContainsKey(info.Key))
                        {
                            args.Add(Convert.ChangeType(reader.GetValue(dict[info.Key]), info.Value));
                        }
                        //如果返回结果中没有该属性所对应的列，直接创建一个空的对象
                        else
                        {
                            args.Add(Activator.CreateInstance(info.Value));
                        }
                    }

                    //创建实体对象
                    var t = (T)constructor.Invoke(args.ToArray());
                    list.Add(t);
                }
            }
            return list;
        }
        private void OutputLog(string sql)
        {
            if (this.DefSettings.OutputSql)
            {
                this.DefSettings.OutputAction(sql);
            }
        }
    }
}