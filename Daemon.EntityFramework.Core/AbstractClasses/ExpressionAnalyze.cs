using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    public abstract class ExpressionAnalyze
    {
        public virtual string Where(Expression expression)
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

            throw new ArgumentException("Invaild Where Expression!");
        }

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


    }
}
