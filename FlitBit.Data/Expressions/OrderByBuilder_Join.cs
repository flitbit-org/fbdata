using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
    /// <summary>
    ///     Builds an order by statement for the data model.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TJoin"></typeparam>
    public class OrderByBuilder<TDataModel, TJoin> : OrderByBuilder
    {
        readonly DataModelSqlExpression<TDataModel> _expression;

        MemberInfo GetMemberFromExpression<T, T1>(Expression<Func<T, T1, object>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var body = expression.Body as MemberExpression;
            if (body != null)
            {
                var memberExpression = body;
                return memberExpression.Member;
            }
            var unaryExpression = (UnaryExpression)expression.Body;

            var operand = unaryExpression.Operand as MemberExpression;
            if (operand != null)
            {
                var memberExpression = operand;
                return memberExpression.Member;
            }
            return null;
        }

        public OrderByBuilder(DataModelSqlExpression<TDataModel> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            _expression = expression;
        }

        public OrderByBuilder<TDataModel, TJoin> OrderBy(Expression<Func<TDataModel, TJoin, object>> expression)
        {
            return OrderBy(expression, SortOrderKind.Asc);
        }

        public OrderByBuilder<TDataModel, TJoin> OrderBy(Expression<Func<TDataModel, TJoin, object>> expression,
            SortOrderKind kind)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var member = GetMemberFromExpression(expression);
            Contract.Assert(member != null, "Expression must reference a field or property member");

            var memberType = member.MemberType;
            Contract.Assert(memberType == MemberTypes.Field
                            || memberType == MemberTypes.Property,
                "Expression must reference a field or property member");

            var parms = new List<ParameterExpression>(expression.Parameters);
            _expression.DuplicateSelfParameter(parms[0]);
            _expression.DuplicateJoinParameter(0, parms[1]);
            var expr = _expression.HandleValueReferencePath(GetMemberExpression(expression));
            OrderByExpr.Add(expr, kind);
            return this;
        }

        public OrderByBuilder<TDataModel, TJoin> ThenBy(Expression<Func<TDataModel, TJoin, object>> expression)
        {
            return ThenBy(expression, SortOrderKind.Asc);
        }

        public OrderByBuilder<TDataModel, TJoin> ThenBy(Expression<Func<TDataModel, TJoin, object>> expression,
            SortOrderKind kind)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var member = GetMemberFromExpression(expression);
            Contract.Assert(member != null, "Expression must reference a field or property member");

            var memberType = member.MemberType;
            Contract.Assert(memberType == MemberTypes.Field
                            || memberType == MemberTypes.Property,
                "Expression must reference a field or property member");

            var parms = new List<ParameterExpression>(expression.Parameters);
            _expression.DuplicateSelfParameter(parms[0]);
            _expression.DuplicateJoinParameter(0, parms[1]);
            var expr = _expression.HandleValueReferencePath(GetMemberExpression(expression));
            OrderByExpr.Add(expr, kind);
            return this;
        }

        MemberExpression GetMemberExpression(Expression<Func<TDataModel, TJoin, object>> expression)
        {
            var body = expression.Body as MemberExpression;
            if (body != null)
            {
                return body;
            }
            var unibody = (UnaryExpression)expression.Body;
            var operand = unibody.Operand as MemberExpression;
            if (operand != null)
            {
                return operand;
            }
            return null;
        }

        internal override void InvokeOrderByDelegate(Delegate delg)
        {
            var orderBy = (Action<OrderByBuilder<TDataModel, TJoin>, TDataModel, TJoin>)delg;
            orderBy(this, default(TDataModel), default(TJoin));
        }
    }
}