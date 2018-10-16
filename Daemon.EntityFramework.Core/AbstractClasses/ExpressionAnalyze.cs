using Daemon.EntityFramework.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class ExpressionAnalyze
    {
        /// <summary>
        /// 设置信息
        /// </summary>
        public DefSettings DefSettings { get; set; }

        /// <summary>
        /// orderby表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string OrderBy(Expression expression)
        {
            if (expression == null)
            {
                return string.Empty;
            }
            var bodyProp = expression.GetType().GetProperty("Body");
            if (bodyProp != null)
            {
                expression = bodyProp.GetValue(expression) as Expression;
            }
            if (expression is BinaryExpression)
            {
                var binExp = expression as BinaryExpression;
                var left = OrderBy(binExp.Left);
                var right = OrderBy(binExp.Right);
                return $"{left}  ,  {right}";
            }
            else if (expression is BlockExpression)
            {
                var blkExp = expression as BlockExpression;
                var sb = new StringBuilder();
                foreach (var exp in blkExp.Expressions)
                {
                    sb.AppendFormat(" {0},", this.OrderBy(exp));
                }
                sb.Length = sb.Length - 1;
                return sb.ToString();
            }
            else if (expression is MemberExpression)
            {
                var memExp = expression as MemberExpression;
                return $"{memExp.Member.DeclaringType.Name}.{ memExp.Member.Name}";
            }
            else if (expression is ConstantExpression)
            {
                var conExp = expression as ConstantExpression;
                if (conExp.Type == typeof(string))
                {
                    return $"'{conExp.Value.ToString()}'";
                }
                else if (conExp.Type == typeof(int))
                {
                    return conExp.Value.ToString();
                }
                else if (conExp.Type == typeof(bool))
                {
                    if (((bool)conExp.Value) == true)
                    {
                        return "(1=1)";
                    }
                    else
                    {
                        return "(1=0)";
                    }
                }
            }

            throw new ArgumentException("Invaild OrderBy Expression!");
        }

        /// <summary>
        /// where Lambda表达式分析器
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual string Where(Expression expression)
        {
            //如果什么都没有就返回这个
            if (expression == null)
            {
                return " 1 = 1 ";
            }

            //反正有body这个属性
            var bodyProp = expression.GetType().GetProperty("Body");
            if (bodyProp != null)
            {
                expression = bodyProp.GetValue(expression) as Expression;
            }
            //二元表达式翻译
            if (expression is BinaryExpression)
            {
                //二元表达式可以拆分成左子式、右子式和符号
                //分别对左右子式进行分析然后用符号链接
                var binExp = expression as BinaryExpression;
                var left = Where(binExp.Left);
                var right = Where(binExp.Right);
                var op = string.Empty;
                switch (binExp.NodeType)
                {
                    case ExpressionType.Equal:
                        op = "=";
                        break;
                    case ExpressionType.AndAlso:
                    case ExpressionType.And:
                        op = "and";
                        break;
                    case ExpressionType.OrElse:
                        op = "or";
                        break;
                    case ExpressionType.NotEqual:
                        op = "!=";
                        break;
                    case ExpressionType.GreaterThan:
                        op = ">";
                        break;
                    case ExpressionType.Add:
                        op = "+";
                        break;
                    case ExpressionType.Subtract:
                        op = "-";
                        break;
                    case ExpressionType.Multiply:
                        op = "*";
                        break;
                    case ExpressionType.Divide:
                        op = "/";
                        break;
                    case ExpressionType.LessThan:
                        op = "<";
                        break;
                    case ExpressionType.Modulo:
                        op = "%";
                        break;
                }
                return $"({left}  {op}  {right})";
            }
            //属性表达式
            else if (expression is MemberExpression)
            {
                //就是 Table.ColumnName
                var memExp = expression as MemberExpression;
                return $"{memExp.Member.DeclaringType.Name}.{ memExp.Member.Name}";
            }
            //常量表达式
            else if (expression is ConstantExpression)
            {
                var conExp = expression as ConstantExpression;
                //string就加单引号
                if (conExp.Type == typeof(string))
                {
                    return $"'{conExp.Value.ToString()}'";
                }
                else if (conExp.Type == typeof(int))
                {
                    return conExp.Value.ToString();
                }
                //bool值sql没有这玩意 暂时这么翻译
                else if (conExp.Type == typeof(bool))
                {
                    if (((bool)conExp.Value) == true)
                    {
                        return "(1=1)";
                    }
                    else
                    {
                        return "(1=0)";
                    }
                }
            }
            //剩下的情况再说。。。
            throw new ArgumentException("Invaild Where Expression!");
        }

        /// <summary>
        /// 同没有colMatchInfo参数方法，只是在匿名类是添加原表的对照关系
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="colMatchInfo"></param>
        /// <returns></returns>
        public virtual string Where(Expression expression,
            IEnumerable<Tuple<string, string, string>> colMatchInfo)
        {
            if (expression == null)
            {
                return " 1 = 1 ";
            }
            var bodyProp = expression.GetType().GetProperty("Body");
            if (bodyProp != null)
            {
                expression = bodyProp.GetValue(expression) as Expression;
            }
            if (expression is BinaryExpression)
            {
                var binExp = expression as BinaryExpression;
                var left = Where(binExp.Left, colMatchInfo);
                var right = Where(binExp.Right, colMatchInfo);
                var op = string.Empty;
                switch (binExp.NodeType)
                {
                    case ExpressionType.Equal:
                        op = "=";
                        break;
                    case ExpressionType.AndAlso:
                    case ExpressionType.And:
                        op = "and";
                        break;
                    case ExpressionType.OrElse:
                        op = "or";
                        break;
                    case ExpressionType.NotEqual:
                        op = "!=";
                        break;
                    case ExpressionType.GreaterThan:
                        op = ">";
                        break;
                    case ExpressionType.Add:
                        op = "+";
                        break;
                    case ExpressionType.Subtract:
                        op = "-";
                        break;
                    case ExpressionType.Multiply:
                        op = "*";
                        break;
                    case ExpressionType.Divide:
                        op = "/";
                        break;
                    case ExpressionType.LessThan:
                        op = "<";
                        break;
                    case ExpressionType.Modulo:
                        op = "%";
                        break;
                }
                return $"({left}  {op}  {right})";
            }
            else if (expression is MemberExpression)
            {
                //使用对照关系生成 Table.ColumnName
                var memExp = expression as MemberExpression;
                if (memExp.Expression is ParameterExpression)
                {
                    var info = colMatchInfo.First(p => p.Item1 == memExp.Member.Name);
                    return $"{info.Item3}.{info.Item2}";
                }
                else
                {
                    if (memExp.Member.Name == "Length")
                    {
                        return $" length({Where(memExp.Expression, colMatchInfo)}) ";
                    }
                }
            }
            else if (expression is ConstantExpression)
            {
                var conExp = expression as ConstantExpression;
                if (conExp.Type == typeof(string))
                {
                    return $"'{conExp.Value.ToString()}'";
                }
                else if (conExp.Type == typeof(int))
                {
                    return conExp.Value.ToString();
                }
                else if (conExp.Type == typeof(bool))
                {
                    if (((bool)conExp.Value) == true)
                    {
                        return "(1=1)";
                    }
                    else
                    {
                        return "(1=0)";
                    }
                }
            }
            else if (expression is MethodCallExpression)
            {
                var method = expression as MethodCallExpression;
                var methodName = method.Method.Name;
                var obj = (method.Object as MemberExpression);
                var propName = obj.Member.Name;
                var exp = string.Empty;
                if (obj.Member.DeclaringType.IsAnonymousType())
                {
                    var info = colMatchInfo.First(p => p.Item1 == propName);
                    exp = $"{info.Item3}.{info.Item2}";
                }
                else
                {
                    exp = $"{obj.Member.DeclaringType.Name}.{propName}";
                }
                if (methodName.ToLower() == "contains")
                {
                    var args = method.Arguments[0].ToString();
                    return $" {exp} like '%{args.Substring(1, args.Length - 2)}%'";
                }
                else if (methodName.ToLower() == "startswith")
                {
                    var args = method.Arguments[0].ToString();
                    return $" {exp} like '{args.Substring(1, args.Length - 2)}%'";
                }
            }
            throw new ArgumentException("Invaild Where Expression!");
        }
    }
}
