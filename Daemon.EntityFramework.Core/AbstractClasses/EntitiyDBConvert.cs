using System;
using System.Collections.Generic;
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
        public abstract int Count<T>(Expression where);

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public abstract List<T> Delete<T>(IEnumerable<T> ts);

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
        public abstract List<TResult> Join<TResult>(
            IDictionary<string, List<KeyValuePair<string, string>>> joinInfo,
            IDictionary<string, KeyValuePair<string, string>> selectInfo,
            IDictionary<string, LambdaExpression> whereInfo,
            LambdaExpression orderByInfo);

        /// <summary>
        /// 查询操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public abstract List<T> Select<T>(LambdaExpression where, LambdaExpression orderBy, ConstantExpression take);

        /// <summary>
        /// 求和操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public abstract object Sum<T>(Expression where, Expression prop);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public abstract List<T> Update<T>(IEnumerable<T> ts);
    }
}
