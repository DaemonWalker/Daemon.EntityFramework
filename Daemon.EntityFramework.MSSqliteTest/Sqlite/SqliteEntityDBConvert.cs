using Daemon.EntityFramework.Core;
using Daemon.EntityFramework.Core.AbstractClasses;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.MSSqliteTest.Sqlite
{
    class SqliteEntityDBConvert:EntityDBConvert
    {
        public override int Count<T>(Expression where)
        {
            var sqlTemp = @"
select count(1) from {0}
 where {1}";
            var sql = string.Format(sqlTemp, typeof(T).Name, DefSettings.ExpressionAnalyze.Where(where));
            return (int)Convert.ChangeType(this.dataOperator.ExecuteSclar(sql), typeof(int));
        }

        public override List<T> Delete<T>(IEnumerable<T> ts)
        {
            var pkProp = typeof(T)
                .GetProperties()
                .First(p => p.CustomAttributes
                    .Count(p2 => p2.AttributeType == DefSettings.GetPKAttrType) > 0);
            var sqlTemp = @"
delete from {0}
where {1}='{2}'";

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

        public override List<T> Insert<T>(IEnumerable<T> ts)
        {
            var tableName = typeof(T).Name;
            var props = typeof(T).GetProperties().ToList();

            var pkProp = typeof(T).GetProperties().First(
                p => p.CustomAttributes.Count(
                    p2 => p2.AttributeType == DefSettings.GetPKAttrType) > 0);
            var pk = $@"
SELECT {pkProp.Name}
  FROM {tableName}
 WHERE changes() = 1 
   AND {pkProp.Name} = last_insert_rowid()";

            var col = new StringBuilder();
            var parms = new StringBuilder();
            for (var i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                if (prop.CanWrite &&
                    prop.CustomAttributes
                    .Where(p => p.AttributeType == DefSettings.GetPKAttrType)
                    .Count() == 0)
                {
                    col.AppendFormat("{0}, ", prop.Name);
                    parms.AppendFormat("'{{{0}}}', ", i);
                }
            }
            col.Length = col.Length - 2;
            parms.Length = parms.Length - 2;
            var sqlTemp = new StringBuilder();
            sqlTemp.AppendFormat(
                "insert into {0} ({1}) values ({2})",
                tableName,
                col.ToString(),
                parms.ToString());

            Func<DbCommand, int> execute = (dbCommand) =>
            {
                var result = 0;
                foreach (var t in ts)
                {
                    var sql = string.Format(
                        sqlTemp.ToString(),
                        props.Select(p => p.GetValue(t)).ToArray());
                    dbCommand.CommandText = sql;
                    result = result + dbCommand.ExecuteNonQuery();

                    dbCommand.CommandText = pk;
                    pkProp.SetValue(t, Convert.ChangeType(dbCommand.ExecuteScalar(), pkProp.PropertyType));
                }
                return result;
            };

            this.dataOperator.ExecuteNonQuery(execute);
            return ts.ToList();
        }


        public override List<T> Select<T>(LambdaExpression where, LambdaExpression orderBy, ConstantExpression take)
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
            return dataOperator.QueryObject<T>(sql.ToString());
        }

        public override object Sum<T>(Expression where, Expression prop)
        {
            throw new NotImplementedException();
        }
        public override List<T> Update<T>(IEnumerable<T> ts)
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
