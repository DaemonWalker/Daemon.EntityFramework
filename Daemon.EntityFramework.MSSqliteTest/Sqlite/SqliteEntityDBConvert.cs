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
    class SqliteEntityDBConvert : EntityDBConvert
    {
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

    }
}
