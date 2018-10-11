using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.Demo
{
    class Query<TEntity> : Daemon.EntityFramework.Core.AbstractClasses.Query<TEntity>
    {
        public Query() { }
        public Query(IQueryProvider queryProvider, Expression expression) :
            base(queryProvider, expression)
        { }
    }
}
