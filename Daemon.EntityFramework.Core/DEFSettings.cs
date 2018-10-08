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
        public PrimaryKeyAttribute PrimaryKeyAttribute { get; set; } = new PrimaryKeyAttribute();

        public Type DataOperatorType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.DataOperator);
        private DataOperator dataOperator;
        public DataOperator DataOperator
        {
            get
            {
                if (dataOperator == null)
                {
                    dataOperator = Activator.CreateInstance(DataOperatorType) as DataOperator;
                    dataOperator.DefSettings = this;
                }
                return dataOperator;
            }
        }

        public Type DataBaseType { get; set; }
        private DataBase dataBase;
        public DataBase DataBase
        {
            get
            {
                if (DataBaseType == null)
                {
                    throw new Exception("No DataBase");
                }
                if (dataBase == null)
                {
                    dataBase = Activator.CreateInstance(DataBaseType) as DataBase;
                    dataBase.DefSettings = this;
                }
                return dataBase;
            }
        }
        public Type EntityDBConvertType { get; set; }
        private EntityDBConvert entityDBConvert;
        public EntityDBConvert EntityDBConvert
        {
            get
            {
                if (EntityDBConvertType == null)
                {
                    throw new Exception("No EntityDBConvert");
                }
                if (entityDBConvert == null)
                {
                    entityDBConvert = Activator.CreateInstance(EntityDBConvertType) as EntityDBConvert;
                    entityDBConvert.DefSettings = this;
                }
                return entityDBConvert;
            }
        }
        public Type QueryProviderType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.QueryProvider<>);
        private Dictionary<Type, object> dictQueryPrivider = new Dictionary<Type, object>();
        public QueryProvider<TEntity> GetQueryProvider<TEntity>()
        {
            if (dictQueryPrivider.ContainsKey(typeof(TEntity)) == false)
            {
                var qpType = QueryProviderType.MakeGenericType(typeof(TEntity));
                var qp = (Activator.CreateInstance(qpType) as QueryProvider<TEntity>);
                qp.DefSettings = this;
                dictQueryPrivider.Add(typeof(TEntity), qp);
            }
            var obj = dictQueryPrivider[typeof(TEntity)];
            return obj as QueryProvider<TEntity>;
        }

        public ExpressionAnalyze ExpressionAnalyze { get; set; } = new Daemon.EntityFramework.Core.Demo.ExpressionAnalyze();

        public Type QueryType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.Query<>);
        private Dictionary<Type, object> dictQuery = new Dictionary<Type, object>();
        public Query<TElement> GetQuery<TElement>()
        {
            if (dictQuery.ContainsKey(typeof(TElement)) == false)
            {
                var qType = QueryType.MakeGenericType(typeof(TElement));
                var q = (Activator.CreateInstance(qType) as Query<TElement>);
                q.DefSettings = this;
                dictQuery.Add(typeof(TElement), q);
            }
            return dictQuery[typeof(TElement)] as Query<TElement>;
        }
        public Type GetPKAttrType
        {
            get
            {
                return PrimaryKeyAttribute.GetType();
            }
        }
    }
}
