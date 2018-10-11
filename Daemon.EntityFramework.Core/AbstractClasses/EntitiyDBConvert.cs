using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class EntityDBConvert
    {
        protected DataOperator dataOperator { get { return DefSettings.DataOperator; } }
        public DefSettings DefSettings { get; set; }
        public abstract List<T> Insert<T>(IEnumerable<T> ts);
        public abstract List<T> Update<T>(IEnumerable<T> ts);
        public abstract List<T> Delete<T>(IEnumerable<T> ts);
        public abstract List<T> Select<T>(LambdaExpression where, LambdaExpression orderBy, ConstantExpression take);
        public abstract int Count<T>(Expression where);
        public abstract object Sum<T>(Expression where, Expression prop);
        public abstract List<TResult> Join<TResult>(
            IDictionary<string, List<KeyValuePair<string, string>>> joinInfo,
            IDictionary<string, KeyValuePair<string, string>> selectInfo,
            IDictionary<string, LambdaExpression> whereInfo,
            LambdaExpression orderByInfo);
    }
}
