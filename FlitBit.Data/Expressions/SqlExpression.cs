using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.Expressions
{
  public class DataModelSqlExpression<TDataModel>
  {
    readonly string _selfRef;

    readonly Dictionary<Expression, SqlExpression> _translations =
      new Dictionary<Expression, SqlExpression>();

    readonly List<SqlParameterExpression> _params = new List<SqlParameterExpression>();
    readonly List<SqlJoinExpression> _joins = new List<SqlJoinExpression>();

    SqlExpression _whereExpression;

    public DataModelSqlExpression(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel> binder, string selfRef)
    {
      Mapping = mapping;
      Binder = binder;
      _selfRef = selfRef;
      SelfReferenceColumn = mapping.GetPreferredReferenceColumn();
    }

    public IMapping<TDataModel> Mapping { get; private set; }

    public IDataModelBinder<TDataModel> Binder { get; private set; }

    ColumnMapping SelfReferenceColumn { get; set; }

    public SqlParameterExpression SelfParameter { get; private set; }

    public IList<SqlParameterExpression> ValueParameters { get { return _params; } }

    internal void AddSelfParameter(ParameterExpression parm)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter == null);

      SelfParameter = new SqlParameterExpression(SqlExpressionKind.Self, _selfRef, parm.Type);
      _translations.Add(parm, SelfParameter);
    }

    internal void JoinParameter(ParameterExpression parm, bool inferOnExpression)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter != null);

      var asAlias = Mapping.QuoteObjectName(parm.Name);
      var expr = new SqlJoinParameterExpression(asAlias, parm.Type);
      var joinMapping = Mappings.AccessMappingFor(parm.Type);
      SqlExpression onExpression = null;
      if (inferOnExpression)
      {
        var dep =
          joinMapping.Dependencies.SingleOrDefault(d => d.Target == Mapping && d.Kind.HasFlag(DependencyKind.Direct));
        if (dep == null)
        {
          throw new NotSupportedException(String.Concat("A direct dependency path is not known from ",
            joinMapping.RuntimeType.GetReadableSimpleName(),
            " to ", typeof(TDataModel).GetReadableSimpleName(), "."));
        }
        var fromCol = joinMapping.Columns.Single(c => c.Member == dep.Member);
        var toCol = Mapping.Columns.Single(c => c.Member == fromCol.ReferenceTargetMember);
        onExpression = new SqlComparisonExpression(ExpressionType.Equal,
          new SqlMemberAccessExpression(Mapping.QuoteObjectName(fromCol.TargetName), toCol.RuntimeType, fromCol, expr),
          new SqlMemberAccessExpression(Mapping.QuoteObjectName(toCol.TargetName), toCol.RuntimeType, toCol,
            SelfParameter)
          );
      }
      _translations.Add(parm, expr);
      expr.Join = new SqlJoinExpression(parm.Type, _joins.Count, joinMapping.DbObjectReference, asAlias, onExpression);
      _joins.Add(expr.Join);
    }

    internal SqlParameterExpression AddValueParameter(ParameterExpression parm)
    {
      var res = new SqlParameterExpression(SqlExpressionKind.Parameter,
        Mapping.GetDbProviderHelper().FormatParameterName(parm.Name), parm.Type);
      _translations.Add(parm, res);
      _params.Add(res);
      return res;
    }

    internal void IngestExpresion(Expression expr)
    {
      var binary = expr as BinaryExpression;
      if (binary == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      }
      var where = HandleBinaryExpression(binary);
      if (where != null
          && _joins.Count > 0)
      {
        foreach (var ea in _translations.Values.Where(t => t.Kind == SqlExpressionKind.Join)
                                        .Cast<SqlJoinParameterExpression>()
                                        .OrderBy(p => p.Join.Ordinal))
        {
          where = OptimizeJoinConditionsFromWhere(ea, where);
          if (where == null)
          {
            break;
          }
        }
      }
      _whereExpression = where;
    }

    SqlExpression OptimizeJoinConditionsFromWhere(SqlJoinParameterExpression param, SqlExpression where)
    {
      var res = where;
      if (res.Kind == SqlExpressionKind.AndAlso
          || res.Kind == SqlExpressionKind.OrElse
          || res.Kind == SqlExpressionKind.Comparison)
      {
        res = RecursiveDescentTryOptimizeJoin(param, (SqlBinaryExpression)res);
      }
      return res;
    }

    SqlExpression RecursiveDescentTryOptimizeJoin(SqlJoinParameterExpression param, SqlBinaryExpression binary)
    {
      var left = binary.Left;
      var right = binary.Right;
      if (binary.Kind == SqlExpressionKind.AndAlso)
      {
        if (RecursiveCanLift(param, left))
        {
          if (RecursiveCanLift(param, right))
          {
            param.Join.AddExpression(binary);
            return null;
          }
          param.Join.AddExpression(binary.Left);
          if (right is SqlBinaryExpression)
          {
            return RecursiveDescentTryOptimizeJoin(param, (SqlBinaryExpression)right);
          }
          return binary.Right;
        }
        if (left is SqlBinaryExpression)
        {
          left = RecursiveDescentTryOptimizeJoin(param, (SqlBinaryExpression)left);
        }
        if (RecursiveCanLift(param, right))
        {
          param.Join.AddExpression(right);
          return left;
        }
        if (right is SqlBinaryExpression)
        {
          return new SqlAndAlsoExpression(left,
            RecursiveDescentTryOptimizeJoin(param, (SqlBinaryExpression)right)
            );
        }
      }
      if (RecursiveCanLift(param, binary))
      {
        param.Join.AddExpression(binary);
        return null;
      }
      return binary;
    }

    bool RecursiveCanLift(SqlJoinParameterExpression param, SqlExpression expr)
    {
      if (expr.Kind == SqlExpressionKind.AndAlso
          || expr.Kind == SqlExpressionKind.OrElse
          || expr.Kind == SqlExpressionKind.Comparison)
      {
        return RecursiveCanLift(param, ((SqlBinaryExpression)expr).Left)
               && RecursiveCanLift(param, ((SqlBinaryExpression)expr).Right);
      }
      var value = expr as SqlValueExpression;
      if (value != null)
      {
        if (value.Source.Kind == SqlExpressionKind.Join)
        {
          return (value.Source == param || ((SqlJoinParameterExpression)value.Source).Join.Ordinal < param.Join.Ordinal);
        }
        return true;
      }
      return false;
    }

    SqlExpression HandleBinaryExpression(BinaryExpression binary)
    {
      if (binary.NodeType == ExpressionType.AndAlso)
      {
        return ProcessAndAlso(binary);
      }
      if (binary.NodeType == ExpressionType.OrElse)
      {
        return ProcessOrElse(binary);
      }
      if (binary.NodeType == ExpressionType.Equal)
      {
        return ProcessComparison(binary);
      }
      if (binary.NodeType == ExpressionType.NotEqual)
      {
        return ProcessComparison(binary);
      }
      throw new NotSupportedException(String.Concat("Expression not supported in sql expressions: ",
        binary.NodeType));
    }

    SqlExpression ProcessOrElse(BinaryExpression binary)
    {
      var left = binary.Left as BinaryExpression;
      if (left == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
          binary.Left.NodeType));
      }

      var right = binary.Right as BinaryExpression;
      if (right == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
          binary.Right.NodeType));
      }

      var res = new SqlOrElseExpression(
        HandleBinaryExpression(left),
        HandleBinaryExpression(right)
        );
      _translations.Add(binary, res);
      return res;
    }

    SqlExpression ProcessAndAlso(BinaryExpression binary)
    {
      var left = binary.Left as BinaryExpression;
      if (left == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
          binary.Left.NodeType));
      }

      var right = binary.Right as BinaryExpression;
      if (right == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported in binary expression: ",
          binary.Right.NodeType));
      }

      var res = new SqlAndAlsoExpression(
        HandleBinaryExpression(left),
        HandleBinaryExpression(right)
        );
      _translations.Add(binary, res);
      return res;
    }

    SqlExpression ProcessComparison(BinaryExpression binary)
    {
      var lhs = FormatValueReference(binary.Left);
      var rhs = FormatValueReference(binary.Right);
      
      if (lhs.Kind == SqlExpressionKind.Parameter 
        && ((SqlParameterExpression)lhs).Column == null
        && rhs.Kind == SqlExpressionKind.MemberAccess)
      {
        ((SqlParameterExpression)lhs).Column = ((SqlMemberAccessExpression)rhs).Column;
      }
      if (rhs.Kind == SqlExpressionKind.Parameter
        && ((SqlParameterExpression)rhs).Column == null
        && lhs.Kind == SqlExpressionKind.MemberAccess)
      {
        ((SqlParameterExpression)rhs).Column = ((SqlMemberAccessExpression)lhs).Column;
      }

      var selfRef = SelfReferenceColumn;
      if (selfRef != null)
      {
        if (lhs.Kind == SqlExpressionKind.Self
            && (rhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression)rhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          lhs = new SqlMemberAccessExpression(
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, lhs);
        }
        if (rhs.Kind == SqlExpressionKind.Self
            && (lhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression)lhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          rhs = new SqlMemberAccessExpression(
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, rhs);
        }
      }

      return new SqlComparisonExpression(binary.NodeType, lhs, rhs);
    }

    SqlValueExpression FormatValueReference(Expression expr)
    {
      if (expr.NodeType == ExpressionType.MemberAccess)
      {
        return HandleValueReferencePath(expr);
      }
      if (expr.NodeType == ExpressionType.Parameter)
      {
        SqlExpression res;
        _translations.TryGetValue(expr, out res);
        return (SqlValueExpression)res;
      }
      throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ",
        expr.NodeType));
    }

    SqlValueExpression HandleValueReferencePath(Expression expr)
    {
      var stack = new Stack<MemberExpression>();
      var item = expr;
      while (item.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression)item);
        item = ((MemberExpression)item).Expression;
      }
      if (item.NodeType != ExpressionType.Parameter)
      {
        throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ", expr.NodeType));
      }

      // The inner-most is a parameter...
      var inner = FormatValueReference(item);
      var fromMapping = (inner.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(inner.Type);
      while (stack.Count > 0)
      {
        var it = stack.Pop();
        SqlExpression res;
        if (_translations.TryGetValue(it, out res))
        {
          inner = (SqlValueExpression)res;
          fromMapping = (inner.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(inner.Type);
        }
        else
        {
          var m = it.Member;
          if (Mappings.ExistsFor(it.Type))
          {
            var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
            if (toDep == null)
            {
              throw new NotSupportedException(String.Concat("A dependency path is not known from ",
                fromMapping.RuntimeType.GetReadableSimpleName(),
                " to ", m.DeclaringType.GetReadableSimpleName(), " via the property `", m.Name, "'."));
            }
            var toCol = fromMapping.Columns.Single(c => c.Member == m);
            var text = Mapping.QuoteObjectName(toCol.TargetName);
            inner = new SqlMemberAccessExpression(text, it.Type, toCol, inner);
            fromMapping = (it.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(it.Type);
          }
          else
          {
            var toCol = fromMapping.Columns.Single(c => c.Member == m);
            if (toCol.IsIdentity
                && (inner is SqlMemberAccessExpression
                    && ((SqlMemberAccessExpression)inner).Column.ReferenceTargetMember == m))
            {
              // special case for identity references.
              _translations.Add(it, inner);
            }
            else
            {
              var text = Mapping.QuoteObjectName(toCol.TargetName);
              inner = new SqlMemberAccessExpression(text, it.Type, toCol, inner);
              _translations.Add(it, inner);
            }
          }
        }
      }
      return inner;
    }

    public void Write(SqlWriter writer)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter != null);

      foreach (var join in _joins)
      {
        join.Write(writer);
      }
      if (_whereExpression != null)
      {
        writer.NewLine().Append("WHERE ").Indent();
        _whereExpression.Write(writer);
        writer.Outdent();
      }
    }
  }

  [Flags]
  public enum SqlExpressionKind
  {
    Unknown = 0,
    Self = 1,
    Join = 2,
    Parameter = 3,
    Constant = 4,
    Null = 5,
    MemberAccess = 6,
    Comparison = 7,
    AndAlso = 8,
    OrElse = 9,
  }

  public abstract class SqlExpression
  {
    public SqlExpression(SqlExpressionKind nodeType) { Kind = nodeType; }

    public SqlExpressionKind Kind { get; private set; }

    public string Text
    {
      get
      {
        var writer = new SqlWriter();
        Write(writer);
        return writer.Text;
      }
    }

    public abstract void Write(SqlWriter writer);

    public override string ToString() { return Text; }
  }

  public class SqlJoinExpression : SqlExpression
  {
    public SqlJoinExpression(Type joinType, int ordinal, string dbObjectReference, string asAlias,
      SqlExpression onExpression)
      : base(SqlExpressionKind.Join)
    {
      Ordinal = ordinal;
      Type = joinType;
      DbObjectReference = dbObjectReference;
      AsAlias = asAlias;
      OnExpression = onExpression;
    }

    public int Ordinal { get; internal set; }

    public Type Type { get; protected set; }

    public SqlExpression OnExpression { get; private set; }

    public string AsAlias { get; set; }

    public string DbObjectReference { get; set; }

    public void AddExpression(SqlExpression expr)
    {
      if (OnExpression != null)
      {}
      OnExpression = expr;
    }

    public override void Write(SqlWriter writer)
    {
      if (OnExpression == null)
      {
        throw new InvalidExpressionException("Join expression does not have a joining constraint (SQL ON statement): "
                                             + DbObjectReference + " AS " + AsAlias + ". " +
                                             "Expressions are evaluated as a binary tree, in order. Use parentheses to ensure the order is as intended and push join conditions as far left as possible to optimize join statements.");
      }
      writer.Indent()
            .NewLine().Append("JOIN ").Append(DbObjectReference).Append(" AS ").Append(AsAlias);
      writer.Indent()
            .NewLine().Append("ON ");
      OnExpression.Write(writer);
      writer
        .Outdent()
        .Outdent();
    }
  }

  public class SqlValueExpression : SqlExpression
  {
    readonly string _text;

    public SqlValueExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType)
    {
      Type = type;
      _text = text;
      Source = this;
    }

    public Type Type { get; private set; }

    public SqlValueExpression Source { get; protected set; }

    public override void Write(SqlWriter writer) { writer.Append(_text); }
  }

  public class SqlParameterExpression : SqlValueExpression
  {
    public SqlParameterExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    {}

    public ColumnMapping Column { get; set; }
  }

  public class SqlJoinParameterExpression : SqlParameterExpression
  {
    public SqlJoinParameterExpression(string text, Type type)
      : base(SqlExpressionKind.Join, text, type)
    {}

    public SqlJoinExpression Join { get; internal set; }
  }

  public class SqlMemberAccessExpression : SqlValueExpression
  {
    public SqlMemberAccessExpression(string text, Type type,
      ColumnMapping col, SqlValueExpression expr)
      : base(SqlExpressionKind.MemberAccess, text, type)
    {
      Column = col;
      Expression = expr;
      var item = expr;
      while (item.Kind == SqlExpressionKind.MemberAccess)
      {
        item = ((SqlMemberAccessExpression)item).Expression;
      }
      Source = item;
    }

    public ColumnMapping Column { get; set; }

    public SqlValueExpression Expression { get; set; }

    public override void Write(SqlWriter writer)
    {
      Expression.Write(writer);
      writer.Append('.');
      base.Write(writer);
    }
  }

  public abstract class SqlBinaryExpression : SqlExpression
  {
    protected SqlBinaryExpression(SqlExpressionKind kind, SqlExpression lhs, SqlExpression rhs)
      : base(kind)
    {
      Left = lhs;
      Right = rhs;
    }

    public SqlExpression Left { get; private set; }
    public SqlExpression Right { get; private set; }
  }

  public class SqlAndAlsoExpression : SqlBinaryExpression
  {
    public SqlAndAlsoExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.AndAlso, lhs, rhs)
    {}

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      Left.Write(writer);
      writer.Append(" AND ");
      Right.Write(writer);
      writer.Append(')');
    }
  }

  public class SqlOrElseExpression : SqlBinaryExpression
  {
    public SqlOrElseExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.OrElse, lhs, rhs)
    {}

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      Left.Write(writer);
      writer.Append(" OR ");
      Right.Write(writer);
      writer.Append(')');
    }
  }

  public class SqlComparisonExpression : SqlBinaryExpression
  {
    public SqlComparisonExpression(ExpressionType comparison, SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.Comparison, lhs, rhs)
    {
      ComparisonType = comparison;
    }

    public ExpressionType ComparisonType { get; private set; }

    public override void Write(SqlWriter writer)
    {
      var op = default(string);
      switch (ComparisonType)
      {
        case ExpressionType.Equal:
          if (Left.Kind == SqlExpressionKind.Null)
          {
            if (Right.Kind == SqlExpressionKind.Null)
            {
              writer.Append("(1 = 1)");
              // dumb, but writer expects a condition and writing such an expression is likewise.
            }
            else
            {
              writer.Append("(");
              Right.Write(writer);
              writer.Append(" IS NULL)");
            }
            return;
          }
          if (Right.Kind == SqlExpressionKind.Null)
          {
            writer.Append("(");
            Left.Write(writer);
            writer.Append(" IS NULL)");
            return;
          }
          op = " = ";
          break;
        case ExpressionType.GreaterThan:
          op = " > ";
          break;
        case ExpressionType.GreaterThanOrEqual:
          op = " >= ";
          break;
        case ExpressionType.LessThan:
          op = " < ";
          break;
        case ExpressionType.LessThanOrEqual:
          op = " <= ";
          break;
        case ExpressionType.NotEqual:
          if (Left.Kind == SqlExpressionKind.Null)
          {
            if (Right.Kind == SqlExpressionKind.Null)
            {
              writer.Append("(1 = 0)");
              // dumb, but writer expects a condition and writing such an expression is likewise.
            }
            else
            {
              writer.Append("(");
              Right.Write(writer);
              writer.Append(" IS NOT NULL)");
            }
            return;
          }
          if (Right.Kind == SqlExpressionKind.Null)
          {
            writer.Append("(");
            Left.Write(writer);
            writer.Append(" IS NOT NULL)");
            return;
          }
          op = " <> ";
          break;
      }
      writer.Append("(");
      Left.Write(writer);
      writer.Append(op);
      Right.Write(writer);
      writer.Append(')');
    }
  }
}