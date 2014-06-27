using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
  /// <summary>
  /// Builds an order by statement for the data model.
  /// </summary>
  /// <typeparam name="TDataModel"></typeparam>
  public class OrderByBuilder<TDataModel>
  {
    DataModelSqlExpression<TDataModel> _expression;
    OrderBy _orderBy = new OrderBy();

    public OrderByBuilder(DataModelSqlExpression<TDataModel> expression)
    {
      Contract.Requires<ArgumentNullException>(expression != null);
      _expression = expression;
    }

    public OrderByBuilder<TDataModel> OrderBy(Expression<Func<TDataModel, object>> expression)
    {
      return OrderBy(expression, SortOrderKind.Asc);
    }

    public OrderByBuilder<TDataModel> OrderBy(Expression<Func<TDataModel, object>> expression, SortOrderKind kind)
    {
      Contract.Requires<ArgumentNullException>(expression != null);

      var member = expression.GetMemberFromExpression();
      Contract.Assert(member != null, "Expression must reference a field or property member");

      var memberType = member.MemberType;
      Contract.Assert(memberType == MemberTypes.Field
                      || memberType == MemberTypes.Property, "Expression must reference a field or property member");

      var parms = new List<ParameterExpression>(expression.Parameters);
      _expression.DuplicateSelfParameter(parms[0]);
      var expr = _expression.HandleValueReferencePath(GetMemberExpression(expression));
      _orderBy.Add(expr, kind);
      return this;
    }

    public OrderByBuilder<TDataModel> ThenBy(Expression<Func<TDataModel, object>> expression)
    {
      return ThenBy(expression, SortOrderKind.Asc);
    }

    public OrderByBuilder<TDataModel> ThenBy(Expression<Func<TDataModel, object>> expression, SortOrderKind kind)
    {
      Contract.Requires<ArgumentNullException>(expression != null);

      var member = expression.GetMemberFromExpression();
      Contract.Assert(member != null, "Expression must reference a field or property member");

      var memberType = member.MemberType;
      Contract.Assert(memberType == MemberTypes.Field
                      || memberType == MemberTypes.Property, "Expression must reference a field or property member");

      var parms = new List<ParameterExpression>(expression.Parameters);
      _expression.DuplicateSelfParameter(parms[0]);
      var expr = _expression.HandleValueReferencePath(GetMemberExpression(expression));
      _orderBy.Add(expr, kind);
      return this;
    }

    MemberExpression GetMemberExpression(Expression<Func<TDataModel, object>> expression)
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

    public OrderBy ToOrderByStatement() { return _orderBy; }
  }
}