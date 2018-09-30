using Daemon.EntityFramework.Core.AbstractClasses;
using Daemon.EntityFramework.Core.Attrbutes;
using Daemon.EntityFramework.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core
{
    public class DefSettings
    {
        public static PrimaryKeyAttribute PrimaryKeyAttribute { get; set; } = new PrimaryKeyAttribute();

        public static Type DataOperatorType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.DataOperator);
        public static DataOperator DataOperator
        {
            get
            {
                return Activator.CreateInstance(DataOperatorType) as DataOperator;
            }
        }

        public static Type DataBaseType { get; set; }
        public static DataBase DataBase
        {
            get
            {
                if (DataBaseType == null)
                {
                    throw new Exception("No DataBase");
                }
                return Activator.CreateInstance(DataBaseType) as DataBase;
            }
        }
        public static Type EntityDBConvertType { get; set; }
        public static EntityDBConvert EntityDBConvert
        {
            get
            {
                if (EntityDBConvertType == null)
                {
                    throw new Exception("No EntityDBConvert");
                }
                return Activator.CreateInstance(EntityDBConvertType) as EntityDBConvert;
            }
        }
        public static Type QueryProviderType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.QueryProvider<>);
        public static QueryProvider<TEntity> GetQueryProvider<TEntity>()
        {
            var qp = QueryProviderType.MakeGenericType(typeof(TEntity));
            return (Activator.CreateInstance(qp) as QueryProvider<TEntity>);
        }

        public static ExpressionAnalyze ExpressionAnalyze { get; set; } = new Daemon.EntityFramework.Core.Demo.ExpressionAnalyze();

        public static Type QueryType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.Query<>);
        public static Query<TElement> GetQuery<TElement>()
        {
            var q = QueryType.MakeGenericType(typeof(TElement));
            return (Activator.CreateInstance(q) as Query<TElement>);
        }
        public static Type GetPKAttrType
        {
            get
            {
                return DefSettings.PrimaryKeyAttribute.GetType();
            }
        }
    }
}
