using Daemon.EntityFramework.Core.AbstractClasses;
using Daemon.EntityFramework.Core.Attributes;
using Daemon.EntityFramework.Core.Exceptions;
using Daemon.EntityFramework.Core.SqlFormatter;
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
        public string CountSqlTemp { get; private set; } = @"select count(1) from {0}";
        public string DeleteSqlTemp { get; private set; } = @"
delete from {0}
where {1}='{2}'";

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

        public Type DataBaseType { get; private set; }
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

        public Type DataOperatorType { get; private set; } = typeof(Daemon.EntityFramework.Core.Demo.DataOperator);
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

        public Type EntityDBConvertType { get; private set; }
        public ExpressionAnalyze ExpressionAnalyze { get; set; } = new Daemon.EntityFramework.Core.Demo.ExpressionAnalyze();
        public Type GetPKAttrType { get { return PrimaryKeyAttribute.GetType(); } }

        public Action<string> OutputAction { get; set; } = sql => Console.WriteLine(new BasicFormatter().Format(sql));
        public bool OutputSql { get; private set; }
        public PrimaryKeyAttribute PrimaryKeyAttribute { get; set; } = new PrimaryKeyAttribute();

        public Type QueryProviderType { get; private set; } = typeof(Daemon.EntityFramework.Core.Demo.QueryProvider<>);
        public Type QueryType { get; private set; } = typeof(Daemon.EntityFramework.Core.Demo.Query<>);
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

        public Type DefExpressionVisitorType { get; private set; } = typeof(Daemon.EntityFramework.Core.Demo.DefExpressionVisitor);
        public DefExpressionVisitor GetDefExpressionVisitor()
        {
            return Activator.CreateInstance(this.DefExpressionVisitorType) as DefExpressionVisitor;
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
        public DefSettings OpenWriteLog()
        {
            this.OutputSql = true;
            return this;
        }

        public DefSettings OpenWriteLog(Action<string> action)
        {
            OutputAction = action;
            return this;
        }

        public DefSettings RegisterType<T>()
        {
            var type = typeof(T);
            if (typeof(DataBase).IsAssignableFrom(type))
            {
                this.DataBaseType = type;
            }
            else if (typeof(EntityDBConvert).IsAssignableFrom(type))
            {
                this.EntityDBConvertType = type;
            }
            else if (typeof(DataOperator).IsAssignableFrom(type))
            {
                this.DataOperatorType = type;
            }
            else if (type.GetInterfaces().Count(p => p == typeof(IQueryProvider)) > 0)
            {
                this.QueryProviderType = type;
            }
            else if (type.GetInterfaces().Count(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IQueryable<>)) > 0)
            {
                this.QueryType = type;
            }
            else if (typeof(T).IsSubclassOf(typeof(DefExpressionVisitor)))
            {
                this.DefExpressionVisitorType = typeof(T);
            }
            return this;
        }

        public DefSettings SetDeleteSqlTemp(string temp)
        {
            this.DeleteSqlTemp = temp;
            return this;
        }
    }
}
