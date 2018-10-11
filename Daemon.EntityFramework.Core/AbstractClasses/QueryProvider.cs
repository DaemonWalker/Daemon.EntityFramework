using Daemon.EntityFramework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            return DefSettings.GetQuery<TElement>(this, expression);
        }

        public virtual object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public virtual TResult Execute<TResult>(Expression expression)
        {
            var methodStack = new Stack<MethodCallExpression>();
            MethodCallExpression methodCall = null;
            if (expression is MethodCallExpression)
            {
                methodStack.Push(expression as MethodCallExpression);
            }
            Dictionary<string, LambdaExpression> whereParms = new Dictionary<string, LambdaExpression>();
            LambdaExpression orderbyParm = null;
            ConstantExpression takeParm = null;
            bool isCount = false;
            bool isFirst = true;
            Dictionary<string, List<KeyValuePair<string, string>>> joinOnDict = new Dictionary<string, List<KeyValuePair<string, string>>>();
            Dictionary<string, KeyValuePair<string, string>> joinSelectDict = null;
            Type joinSelectType = null;
            while (methodStack.Count > 0)
            {
                methodCall = methodStack.Pop();
                Expression method = methodCall.Arguments[0];
                if (methodCall.Method.Name == "Join")
                {
                    var tempMethod = methodCall.Arguments[1] as MethodCallExpression;
                    if (tempMethod != null)
                    {
                        methodStack.Push(tempMethod);
                    }
                    var kvLeft = this.JoinOnAnalyze(methodCall.Arguments[2] as UnaryExpression);
                    var kvRight = this.JoinOnAnalyze(methodCall.Arguments[3] as UnaryExpression);
                    var t = this.JoinSelectAnalyze(methodCall.Arguments[4] as UnaryExpression);
                    joinSelectDict = t.Item1;
                    joinSelectType = t.Item2;
                    joinOnDict.Add(kvLeft.Key, kvLeft.Value);
                    joinOnDict.Add(kvRight.Key, kvRight.Value);
                }
                else if (methodCall.Arguments.Count == 1)
                {
                    if (methodCall.Method.Name == "Count")
                    {
                        isCount = true;
                        //AddToWhereDict(whereParms, methodCall.Method.DeclaringType.Name,
                        //    Expression.Equal(Expression.Constant(1), Expression.Constant(1)));
                        //whereParm = Expression.Lambda(Expression.Constant(true));
                    }
                    else if (methodCall.Method.Name == "First")
                    {
                        isFirst = true;
                        takeParm = ConstExpOperate(takeParm, Expression.Constant(1));
                    }
                }
                else if (methodCall.Arguments.Count == 2)
                {
                    Expression lambda = methodCall.Arguments[1];
                    Expression right = null;
                    string type = string.Empty;
                    if (lambda is UnaryExpression)
                    {
                        right = (lambda as UnaryExpression).Operand as LambdaExpression;
                        var parmType = ((lambda as UnaryExpression).Operand as LambdaExpression).Parameters[0].Type;
                        if (parmType.IsAnonymousType())
                        {
                            type = "AnonymousType";
                        }
                        else
                        {
                            type = parmType.Name;
                        }
                    }
                    else if (lambda is ConstantExpression)
                    {
                        right = lambda as ConstantExpression;
                    }
                    if (methodCall.Method.Name == "Where")
                    {
                        AddToWhereDict(whereParms, type, right);
                    }
                    else if (methodCall.Method.Name == "Count")
                    {
                        isCount = true;
                        AddToWhereDict(whereParms, type, right);
                    }
                    else if (methodCall.Method.Name == "Take")
                    {
                        takeParm = ConstExpOperate(takeParm, right);
                    }
                    else if (methodCall.Method.Name == "OrderBy")
                    {
                        orderbyParm = LambdaExpOperate(orderbyParm, right);
                    }
                    else if (methodCall.Method.Name == "First")
                    {
                        isFirst = true;
                        AddToWhereDict(whereParms, type, right);
                        takeParm = ConstExpOperate(takeParm, Expression.Constant(1));
                    }
                }
                if (method is MethodCallExpression)
                {
                    methodStack.Push(method as MethodCallExpression);
                }
            }

            var convert = DefSettings.EntityDBConvert;
            object result = null;
            if (joinSelectDict != null)
            {
                var method = convert.GetType().GetMethod("Join").MakeGenericMethod(joinSelectType);
                result = method.Invoke(convert, new object[] { joinOnDict, joinSelectDict, whereParms, null });
            }
            else if (orderbyParm != null)
            {
                var list = convert.Select<T>(whereParms.First().Value, orderbyParm, takeParm);
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
                result = convert.Count<T>(whereParms.FirstOrDefault().Value);
            }
            else
            {
                result = convert.Select<T>(whereParms.FirstOrDefault().Value, orderbyParm, takeParm);
            }
            return (TResult)result;
        }

        protected virtual LambdaExpression LambdaExpOperate(LambdaExpression lambda, Expression right)
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
        protected virtual ConstantExpression ConstExpOperate(ConstantExpression constExp, Expression right)
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
        protected virtual KeyValuePair<string, List<KeyValuePair<string, string>>> JoinOnAnalyze(UnaryExpression unaryExpression)
        {
            var operand = unaryExpression.Operand as LambdaExpression;
            Type type = null;
            var list = new List<KeyValuePair<string, string>>();

            if (operand.Body is NewExpression)
            {
                var body = (unaryExpression.Operand as LambdaExpression).Body as NewExpression;
                type = body.Arguments.Select(p =>
                {
                    if (p is MemberExpression)
                    {
                        var me = p as MemberExpression;
                        return me.Member.DeclaringType;
                    }
                    else if (p is MethodCallExpression)
                    {
                        var obj = (p as MethodCallExpression).Object as MemberExpression;
                        return obj.Expression.Type;
                    }
                    else
                    {
                        return null;
                    }
                }).First(p => p != null);
                for (int i = 0; i < body.Members.Count; i++)
                {
                    if (body.Arguments[i] is MemberExpression)
                    {
                        list.Add(new KeyValuePair<string, string>(
                            body.Members[i].Name,
                            (body.Arguments[i] as MemberExpression).Member.Name));
                    }
                    else
                    {
                        list.Add(new KeyValuePair<string, string>(
                            body.Members[i].Name,
                            ((body.Arguments[i] as MethodCallExpression).Object as MemberExpression).Member.Name));
                    }
                }
            }
            var kv = new KeyValuePair<string, List<KeyValuePair<string, string>>>(type.Name, list);
            return kv;

        }
        protected virtual Tuple<Dictionary<string, KeyValuePair<string, string>>, Type> JoinSelectAnalyze(UnaryExpression unaryExpression)
        {
            var operand = unaryExpression.Operand as LambdaExpression;
            var dict = new Dictionary<string, KeyValuePair<string, string>>();
            Type type = null;
            if (operand.Body is MemberInitExpression)
            {
                var body = operand.Body as MemberInitExpression;
                var prop = body.Bindings.First().GetType().GetProperty("Expression", BindingFlags.Public | BindingFlags.Instance);
                type = body.Type;
                foreach (MemberBinding bind in body.Bindings)
                {
                    var newType = bind.Member.Name;
                    var exp = (prop.GetValue(bind) as MemberExpression);
                    dict.Add(newType,
                        new KeyValuePair<string, string>(
                            exp.Member.DeclaringType.Name,
                            exp.Member.Name));
                }
            }
            else if (operand.Body is NewExpression)
            {
                var body = operand.Body as NewExpression;
                type = body.Type;
                for (int i = 0; i < body.Members.Count; i++)
                {
                    var member = body.Members[i];
                    var arg = body.Arguments[i] as MemberExpression;
                    dict.Add(member.Name,
                        new KeyValuePair<string, string>(
                            arg.Member.DeclaringType.Name,
                            arg.Member.Name));
                    //var 
                    //dict.Add();
                }
                //foreach (MemberBinding bind in body.Bindings)
                //{
                //    var newType = bind.Member.Name;
                //    var exp = (prop.GetValue(bind) as MemberExpression);
                //    dict.Add(newType,
                //        new KeyValuePair<string, string>(
                //            exp.Member.DeclaringType.Name,
                //            exp.Member.Name));
                //}
            }
            return new Tuple<Dictionary<string, KeyValuePair<string, string>>, Type>(dict, type);
        }
        protected virtual void AddToWhereDict(Dictionary<string, LambdaExpression> dict, string type, Expression right)
        {
            if (dict.ContainsKey(type) == false)
            {
                dict.Add(type, LambdaExpOperate(null, right));
            }
            else
            {
                dict[type] = LambdaExpOperate(dict[type], right);
            }
        }
    }
}
