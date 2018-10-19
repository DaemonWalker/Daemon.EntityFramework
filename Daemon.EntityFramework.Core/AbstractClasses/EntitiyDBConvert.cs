using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class EntityDBConvert
    {
        /// <summary>
        /// 设置信息
        /// </summary>
        public DefSettings DefSettings { get; set; }

        /// <summary>
        /// DataOperator对象
        /// </summary>
        protected DataOperator dataOperator { get { return DefSettings.DataOperator; } }
        /// <summary>
        /// 行数查询操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual int Count<T>(Expression where)
        {
            var sql = this.DefSettings.CountSqlTemp;
            if (where != null)
            {
                sql = sql + @" where {1}";
                sql = string.Format(sql, typeof(T).Name, DefSettings.ExpressionAnalyze.Where(where));
            }
            else
            {
                sql = this.DefSettings.CountSqlTemp;
                sql = string.Format(sql, typeof(T).Name);
            }
            return (int)Convert.ChangeType(this.dataOperator.ExecuteSclar(sql), typeof(int));
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public virtual List<T> Delete<T>(IEnumerable<T> ts)
        {
            var pkProp = typeof(T)
                .GetProperties()
                .First(p => p.CustomAttributes
                    .Count(p2 => p2.AttributeType == DefSettings.GetPKAttrType) > 0);
            var sqlTemp = this.DefSettings.DeleteSqlTemp;

            Func<DbCommand, int> foo = (command) =>
            {
                var result = 0;
                foreach (var t in ts)
                {
                    var sql = string.Format(sqlTemp, typeof(T).Name, pkProp.Name, pkProp.GetValue(t));
                    command.CommandText = sql;
                    result = result + command.ExecuteNonQuery();
                }
                return result;
            };
            this.dataOperator.ExecuteNonQuery(foo);
            return ts.ToList();
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public abstract List<T> Insert<T>(IEnumerable<T> ts);

        /// <summary>
        /// Inner Join 操作
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="joinInfo"></param>
        /// <param name="selectInfo"></param>
        /// <param name="whereInfo"></param>
        /// <param name="orderByInfo"></param>
        /// <returns></returns>
        public virtual List<TResult> Join<TResult>(
            IDictionary<Type, List<KeyValuePair<string, string>>> joinInfo,
            IEnumerable<Tuple<string, string, string>> selectInfo,
            IDictionary<string, LambdaExpression> whereInfo,
            LambdaExpression orderByInfo,
            IEnumerable<Tuple<string, string, string>> mapRelation)
        {
            var sql = new StringBuilder("select ");
            foreach (var tuple in selectInfo)
            {
                sql.AppendFormat(" {0}.{1} {2},", tuple.Item3, tuple.Item2, tuple.Item1);
            }
            sql.Length = sql.Length - 1;
            var baseTable = joinInfo.First();
            sql.AppendFormat(" from {0} ", baseTable.Key.Name);
            for (int i = 1; i < joinInfo.Count(); i++)
            {
                var info = joinInfo.ElementAt(i);
                sql.AppendFormat(" inner join {0} on ", info.Key.Name);
                for (int k = 0; k < i; k++)
                {
                    var basic = joinInfo.ElementAt(k);
                    foreach (var item in info.Value)
                    {
                        var match = basic.Value.Where(p => p.Key == item.Key);
                        if (match.Count() > 0)
                        {
                            var tempItem = match.First();
                            sql.AppendFormat(
                                "{0}.{1} = {2}.{3} and ",
                                basic.Key.Name,
                                tempItem.Value,
                                info.Key.Name,
                                item.Value);
                        }
                    }
                }
                sql.Length = sql.Length - 4;
            }
            if (whereInfo.Count() != 0)
            {
                sql.Append("where ");
                foreach (var lambda in whereInfo)
                {
                    if (lambda.Key == "AnonymousType")
                    {
                        sql.AppendFormat("{0} and ",
                            DefSettings.ExpressionAnalyze.Where(
                                lambda.Value, mapRelation));
                    }
                    else
                    {
                        sql.AppendFormat("{0} and ", DefSettings.ExpressionAnalyze.Where(lambda.Value));
                    }
                }
                sql.Length = sql.Length - 4;
            }
            return dataOperator.Query<TResult>(sql.ToString());
        }

        /// <summary>
        /// 查询操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public virtual List<T> Select<T>(LambdaExpression where, LambdaExpression orderBy, ConstantExpression take)
        {
            var tableName = typeof(T).Name;
            var sql = new StringBuilder($@"
select * from {tableName} 
 where {DefSettings.ExpressionAnalyze.Where(where)}");
            if (orderBy != null)
            {
                sql.Append($@"
 order by {DefSettings.ExpressionAnalyze.OrderBy(orderBy)}");
            }
            if (take != null)
            {
                sql.Append($@"
 limit 0, {DefSettings.ExpressionAnalyze.OrderBy(take)}");
            }
            return dataOperator.Query<T>(sql.ToString());
        }

        /// <summary>
        /// 求和操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public virtual object Sum<T>(Expression where, Expression prop)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public virtual List<T> Update<T>(IEnumerable<T> ts)
        {
            var pkProp = typeof(T)
                .GetProperties()
                .First(p => p.CustomAttributes
                    .Count(p2 => p2.AttributeType == DefSettings.GetPKAttrType) > 0);
            var tableName = typeof(T).Name;
            var props = typeof(T).GetProperties().ToList();
            var sqlTemp = new StringBuilder($@"update {tableName} set ");
            for (int i = 0; i < props.Count; i++)
            {
                var prop = typeof(T).GetProperties()[i];
                if (prop.GetHashCode() != pkProp.GetHashCode() &&
                    prop.CanWrite == false)
                {
                    continue;
                }
                sqlTemp.AppendFormat("{0} = '{{{1}}}',", prop.Name, i);
            }
            sqlTemp.Length = sqlTemp.Length - 1;
            sqlTemp.AppendFormat(
                " where {0} = '{{{1}}}'",
                pkProp.Name,
                props.IndexOf(pkProp));

            Func<DbCommand, int> func = (command) =>
            {
                var result = 0;
                foreach (var t in ts)
                {
                    var sql = string.Format(
                        sqlTemp.ToString(),
                        props.Select(p => p.GetValue(t)).ToArray());
                    command.CommandText = sql;
                    result = result + command.ExecuteNonQuery();
                }
                return result;
            };
            this.dataOperator.ExecuteNonQuery(func);

            return ts.ToList();
        }
    }
}
