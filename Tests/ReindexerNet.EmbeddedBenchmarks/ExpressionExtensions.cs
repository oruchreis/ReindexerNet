using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNetBenchmark;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
    this Expression<Func<T, bool>> expr1,
    Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    public static Expression<Func<T, bool>> OrAlso<T>(
    this Expression<Func<T, bool>> expr1,
    Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(left, right), parameter);
    }



    private class ReplaceExpressionVisitor
        : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node);
        }
    }

    public static IReadOnlyList<MemberInfo> GetPropertySelectors(this LambdaExpression expression)
    {
        var visitor = new PropertyVisitor();
        visitor.Visit(expression.Body);
        visitor.Path.Reverse();
        return visitor.Path;
    }

    private class PropertyVisitor : ExpressionVisitor
    {
        internal readonly List<MemberInfo> Path = new List<MemberInfo>();

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!(node.Member is PropertyInfo))
            {
                throw new ArgumentException("The path can only contain properties", nameof(node));
            }

            this.Path.Add(node.Member);
            return base.VisitMember(node);
        }
    }

    public static string GetPropertyPath(this LambdaExpression expression)
    {
        return string.Join(".", expression.GetPropertySelectors().Select(p => p.Name));
    }

    public static string GetPropertyName(this LambdaExpression expression)
    {
        if (expression == null) return null;

        if (expression.Body is MemberExpression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }
        else
        {
            var op = ((UnaryExpression)expression.Body).Operand;
            return ((MemberExpression)op).Member.Name;
        }
    }

    public static MemberExpression GetMemberExpression(this LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression;
        }
        else if (expression.Body is UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                return unaryMemberExpression;
            }
        }

        return null;
    }
}
