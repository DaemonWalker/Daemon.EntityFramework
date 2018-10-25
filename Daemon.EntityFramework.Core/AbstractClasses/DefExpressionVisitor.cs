using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Daemon.EntityFramework.Core.AbstractClasses
{
    abstract public class DefExpressionVisitor : ExpressionVisitor
    {
        protected virtual bool IsMemberInMemory(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)expression;
                if (member.Expression.NodeType == ExpressionType.Constant)
                {
                    return true;
                }
                else
                {
                    return IsMemberInMemory(member.Expression);
                }
            }
            return false;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (IsMemberInMemory(node) == false)
            {
                return base.VisitMember(node);
            }

            if (node.Type == typeof(string))
            {
                var item = Expression.Lambda<Func<string>>(node);
                var value = item.Compile()();
                return Expression.Constant(value, typeof(string));
            }
            else if (node.Type == typeof(int))
            {
                var item = Expression.Lambda<Func<int>>(node);
                var value = item.Compile()();
                return Expression.Constant(value, typeof(int));
            }
            return base.VisitMember(node);
        }
    }
}
