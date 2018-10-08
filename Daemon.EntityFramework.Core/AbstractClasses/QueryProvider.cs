using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class QueryProvider<T> : IQueryProvider
    {
        public DefSettings DefSettings { get; set; }
        DataOperator DataOperator
        {
            get
            {
                return this.DefSettings.DataOperator;
            }
        }
        public virtual IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return DefSettings.GetQuery<TElement>();
        }

        public virtual object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual TResult Execute<TResult>(Expression expression)
        {
            MethodCallExpression methodCall = (expression as MethodCallExpression);
            LambdaExpression whereParm = null;
            LambdaExpression orderbyParm = null;
            ConstantExpression takeParm = null;
            bool isCount = false;
            bool isFirst = true;
            while (methodCall != null)
            {
                Expression method = methodCall.Arguments[0];
                if (methodCall.Arguments.Count == 1)
                {
                    if (methodCall.Method.Name == "Count")
                    {
                        isCount = true;
                        whereParm = Expression.Lambda(Expression.Constant(true));
                    }
                    else if (methodCall.Method.Name == "First")
                    {
                        isFirst = true;
                        takeParm = constExpOperate(takeParm, Expression.Constant(1));
                    }
                }
                else if (methodCall.Arguments.Count == 2)
                {
                    Expression lambda = methodCall.Arguments[1];
                    Expression right = null;
                    if (lambda is UnaryExpression)
                    {
                        right = (lambda as UnaryExpression).Operand as LambdaExpression;
                    }
                    else if (lambda is ConstantExpression)
                    {
                        right = lambda as ConstantExpression;
                    }
                    if (methodCall.Method.Name == "Where")
                    {
                        whereParm = lambdaExpOperate(whereParm, right);
                    }
                    else if (methodCall.Method.Name == "Count")
                    {
                        isCount = true;
                        whereParm = lambdaExpOperate(whereParm, right);
                    }
                    else if (methodCall.Method.Name == "Take")
                    {
                        takeParm = constExpOperate(takeParm, right);
                    }
                    else if (methodCall.Method.Name == "OrderBy")
                    {
                        orderbyParm = lambdaExpOperate(orderbyParm, right);
                    }
                    else if (methodCall.Method.Name == "First")
                    {
                        isFirst = true;
                        whereParm = lambdaExpOperate(whereParm, right);
                        takeParm = constExpOperate(takeParm, Expression.Constant(1));
                    }
                }
                methodCall = method as MethodCallExpression;
            }

            var convert = DefSettings.EntityDBConvert;
            object result = null;
            if (orderbyParm != null)
            {
                var list = convert.Select<T>(whereParm, orderbyParm, takeParm);
                if (isFirst)
                {
                    result = list.First();
                }
                else
                {
                    result = list;
                }
            }
            else if (isCount)
            {
                result = convert.Count<T>(whereParm);
            }
            //if(result.GetType().GetInterface())
            return (TResult)result;
        }

        protected virtual LambdaExpression lambdaExpOperate(LambdaExpression lambda, Expression right)
        {
            var labExp = right as LambdaExpression;
            if (lambda == null)
            {
                lambda = Expression.Lambda(labExp.Body, labExp.Parameters);
            }
            else
            {
                Expression left = (lambda as LambdaExpression).Body;
                Expression temp = Expression.AndAlso(labExp.Body, left);
                lambda = Expression.Lambda(temp, lambda.Parameters);
            }
            return lambda;
        }
        protected virtual ConstantExpression constExpOperate(ConstantExpression constExp, Expression right)
        {
            var constRight = right as ConstantExpression;
            if (constExp == null)
            {
                constExp = right as ConstantExpression;
                return constExp;
            }
            else
            {
                throw new InvalidOperationException("Too Many Take Called!");
            }
        }
    }
}
