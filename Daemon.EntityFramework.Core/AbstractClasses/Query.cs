using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class Query<TEntity> : IQueryable<TEntity>, IOrderedQueryable<TEntity>
    {
        public DefSettings DefSettings { get; set; }
        protected IQueryProvider _queryProvider;
        protected IQueryProvider queryProvider
        {
            set
            {
                this._queryProvider = value;
            }
            get
            {
                if (this._queryProvider == null)
                {
                    this._queryProvider = this.DefSettings.GetQueryProvider<TEntity>();
                }
                return this._queryProvider;
            }
        }
        private Expression expression;
        public Query(IQueryProvider queryProvider, Expression expression)
        {
            this.queryProvider = queryProvider;
            this.expression = expression;
        }

        public Query()
        {
            this.expression = Expression.Constant(this);
        }

        public Type ElementType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        public Expression Expression
        {
            get
            {
                return this.expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return this.queryProvider;
            }
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            var result = this.queryProvider.Execute<List<TEntity>>(expression);
            if (result == null)
            {
                yield break;
            }
            foreach (var item in result)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Provider.Execute(this.expression)).GetEnumerator();
        }
    }
}
