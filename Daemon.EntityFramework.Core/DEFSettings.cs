﻿using Daemon.EntityFramework.Core.AbstractClasses;
using Daemon.EntityFramework.Core.Attrbutes;
using Daemon.EntityFramework.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Daemon.EntityFramework.Core
{
    public class DefSettings
    {
        private DataBase dataBase;
        private DataOperator dataOperator;
        private Dictionary<Type, object> dictQueryPrivider = new Dictionary<Type, object>();
        private EntityDBConvert entityDBConvert;
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

        public Type DataBaseType { get; set; }
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

        public Type DataOperatorType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.DataOperator);
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

        public Type EntityDBConvertType { get; set; }
        public ExpressionAnalyze ExpressionAnalyze { get; set; } = new Daemon.EntityFramework.Core.Demo.ExpressionAnalyze();
        public Type GetPKAttrType
        {
            get
            {
                return PrimaryKeyAttribute.GetType();
            }
        }

        public Action<string> OutputAction { get; set; } = sql => Console.WriteLine(sql);
        public bool OutputSql { get; set; }
        public PrimaryKeyAttribute PrimaryKeyAttribute { get; set; } = new PrimaryKeyAttribute();
        public Type QueryProviderType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.QueryProvider<>);
        public Type QueryType { get; set; } = typeof(Daemon.EntityFramework.Core.Demo.Query<>);
        public Query<TElement> GetQuery<TElement>()
        {
            var qType = QueryType.MakeGenericType(typeof(TElement));
            var q = (Activator.CreateInstance(qType) as Query<TElement>);
            q.DefSettings = this;
            return q;
        }

        public Query<TElement> GetQuery<TElement>(IQueryProvider queryProvider, Expression expression)
        {
            var qType = QueryType.MakeGenericType(typeof(TElement));
            var q = (Activator.CreateInstance(qType, queryProvider, expression) as Query<TElement>);
            q.DefSettings = this;
            return q;
        }

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
    }
}
