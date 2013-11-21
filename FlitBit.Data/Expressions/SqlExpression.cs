using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.Expressions
{
  public class DataModelSqlExpression<TDataModel>
  {
    private readonly string _selfRef;

    private readonly Dictionary<Expression, SqlExpression> _translations =
      new Dictionary<Expression, SqlExpression>();

    private readonly List<SqlJoinExpression> _joins = new List<SqlJoinExpression>();

    private SqlExpression _whereExpression;

    public DataModelSqlExpression(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel> binder, string selfRef)
    {
      Mapping = mapping;
      Binder = binder;
      _selfRef = selfRef;
      SelfReferenceColumn = mapping.GetPreferredReferenceColumn();
    }

    public IMapping<TDataModel> Mapping { get; private set; }

    public IDataModelBinder<TDataModel> Binder { get; private set; }
    
    private ColumnMapping SelfReferenceColumn { get; set; }

    public SqlParameterExpression SelfParameter { get; private set; }

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
      var expr = new SqlParameterExpression(SqlExpressionKind.Join, asAlias, parm.Type);
      var joinMapping = Mappings.AccessMappingFor(parm.Type);
      SqlExpression onExpression = null;
      if (inferOnExpression)
      {
        var dep = joinMapping.Dependencies.SingleOrDefault(d => d.Target == Mapping && d.Kind.HasFlag(DependencyKind.Direct));
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
          new SqlMemberAccessExpression(Mapping.QuoteObjectName(toCol.TargetName), toCol.RuntimeType, toCol, SelfParameter)
          );
      }
      _translations.Add(parm, expr);
      _joins.Add(new SqlJoinExpression(parm.Type, _joins.Count, joinMapping.DbObjectReference, asAlias, onExpression));
    }

    internal void AddValueParameter(ParameterExpression parm)
    {
      _translations.Add(parm,
        new SqlParameterExpression(SqlExpressionKind.Parameter,
          Mapping.GetDbProviderHelper().FormatParameterName(parm.Name), parm.Type));
    }

    private class ParamJoin
    {
      public SqlParameterExpression Param { get; set; }
      public SqlJoinExpression Join { get; set; }
    }

    internal void IngestExpresion(Expression expr)
    {
      var binary = expr as BinaryExpression;
      if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      var where = HandleBinaryExpression(binary);
      if (where != null && _joins.Count > 0)
      {
        var joinParms = _translations.Values.Where(t => t.Kind == SqlExpressionKind.Join);
        foreach (var ea in from j in _joins
                           join p in joinParms on j.AsAlias equals p.Text
                           select new ParamJoin {  Param = (SqlParameterExpression)p, Join = j })
        {
          where = OptimizeJoinConditionsFromWhere(ea, where);
          if (where == null) break;
        }
      }
      _whereExpression = where;
    }

    private SqlExpression OptimizeJoinConditionsFromWhere(ParamJoin ea, SqlExpression where)
    {
      var res = where;

      return res;
    }

    private SqlExpression HandleBinaryExpression(BinaryExpression binary)
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

    private SqlExpression ProcessOrElse(BinaryExpression binary)
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

    private SqlExpression ProcessAndAlso(BinaryExpression binary)
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

    private SqlExpression ProcessComparison(BinaryExpression binary)
    {
      SqlValueExpression lhs = FormatValueReference(binary.Left);
      SqlValueExpression rhs = FormatValueReference(binary.Right);
      ColumnMapping selfRef = SelfReferenceColumn;
      if (selfRef != null)
      {
        if (lhs.Kind == SqlExpressionKind.Self
            && (rhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression) rhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          lhs = new SqlMemberAccessExpression(
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, lhs);
        }
        if (rhs.Kind == SqlExpressionKind.Self
            && (lhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression) lhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          rhs = new SqlMemberAccessExpression(
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, rhs);
        }
      }
      return new SqlComparisonExpression(binary.NodeType, lhs, rhs);
    }

    private SqlValueExpression FormatValueReference(Expression expr)
    {
      if (expr.NodeType == ExpressionType.MemberAccess)
      {
        return HandleValueReferencePath(expr);
      }
      if (expr.NodeType == ExpressionType.Parameter)
      {
        SqlExpression res;
        _translations.TryGetValue(expr, out res);
        return (SqlValueExpression) res;
      }
      throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ",
        expr.NodeType));
    }

    private SqlValueExpression HandleValueReferencePath(Expression expr)
    {
      var stack = new Stack<MemberExpression>();
      Expression item = expr;
      while (item.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression) item);
        item = ((MemberExpression) item).Expression;
      }
      if (item.NodeType != ExpressionType.Parameter)
        throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ", expr.NodeType));

      // The inner-most is a parameter...
      SqlValueExpression inner = FormatValueReference(item);
      IMapping fromMapping = (inner.Type == typeof (TDataModel)) ? Mapping : Mappings.AccessMappingFor(inner.Type);
      while (stack.Count > 0)
      {
        MemberExpression it = stack.Pop();
        SqlExpression res;
        if (_translations.TryGetValue(it, out res))
        {
          inner = (SqlValueExpression) res;
          fromMapping = (inner.Type == typeof (TDataModel)) ? Mapping : Mappings.AccessMappingFor(inner.Type);
        }
        else
        {
          MemberInfo m = it.Member;
          if (Mappings.ExistsFor(it.Type))
          {
            Dependency toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
            if (toDep == null)
            {
              throw new NotSupportedException(String.Concat("A dependency path is not known from ",
                fromMapping.RuntimeType.GetReadableSimpleName(),
                " to ", m.DeclaringType.GetReadableSimpleName(), " via the property `", m.Name, "'."));
            }
            ColumnMapping toCol = fromMapping.Columns.Single(c => c.Member == m);
            string text = Mapping.QuoteObjectName(toCol.TargetName);
            inner = new SqlMemberAccessExpression(text, it.Type, toCol, inner);
            fromMapping = (it.Type == typeof (TDataModel)) ? Mapping : Mappings.AccessMappingFor(it.Type);
          }
          else
          {
            ColumnMapping toCol = fromMapping.Columns.Single(c => c.Member == m);
            if (toCol.IsIdentity
                && (inner is SqlMemberAccessExpression
                    && ((SqlMemberAccessExpression) inner).Column.ReferenceTargetMember == m))
            {
              // special case for identity references.
              _translations.Add(it, inner);
            }
            else
            {
              string text = Mapping.QuoteObjectName(toCol.TargetName);
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
    public SqlExpression(SqlExpressionKind nodeType)
    {
      Kind = nodeType;
    }

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

    public override string ToString()
    {
      return Text;
    }
  }

  public class SqlJoinExpression : SqlExpression
  {
    public SqlJoinExpression(Type joinType, int ordinal, string dbObjectReference, string asAlias, SqlExpression onExpression) : base(SqlExpressionKind.Join)
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
      {

      }
      OnExpression = expr;
    }

    public override void Write(SqlWriter writer)
    {
      if (OnExpression == null)
      {
        throw new InvalidExpressionException("Join expression does not have a joining constraint (SQL ON statement): " + DbObjectReference + " AS " + AsAlias + ". " +
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
    private readonly string _text;

    public SqlValueExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType)
    {
      Type = type;
      _text = text;
    }

    public Type Type { get; private set; }

    public override void Write(SqlWriter writer)
    {
      writer.Append(_text);
    }
  }

  public class SqlParameterExpression : SqlValueExpression
  {
    public SqlParameterExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    {
    }
  }


  public class SqlMemberAccessExpression : SqlValueExpression
  {
    public SqlMemberAccessExpression(string text, Type type,
      ColumnMapping col, SqlValueExpression expr)
      : base(SqlExpressionKind.MemberAccess, text, type)
    {
      Column = col;
      Expression = expr;
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

  public class SqlAndAlsoExpression : SqlExpression
  {
    public SqlAndAlsoExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.AndAlso)
    {
      Left = lhs;
      Right = rhs;
    }

    public SqlExpression Left { get; private set; }
    public SqlExpression Right { get; private set; }

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      Left.Write(writer);
      writer.Append(" AND ");
      Right.Write(writer);
      writer.Append(')');
    }
  }

  public class SqlOrElseExpression : SqlExpression
  {
    public SqlOrElseExpression(SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.OrElse)
    {
      Left = lhs;
      Right = rhs;
    }

    public SqlExpression Left { get; private set; }
    public SqlExpression Right { get; private set; }

    public override void Write(SqlWriter writer)
    {
      writer.Append("(");
      Left.Write(writer);
      writer.Append(" OR ");
      Right.Write(writer);
      writer.Append(')');
    }
  }


  public class SqlComparisonExpression : SqlExpression
  {
    public SqlComparisonExpression(ExpressionType comparison, SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.Comparison)
    {
      ComparisonType = comparison;
      Left = lhs;
      Right = rhs;
    }

    public ExpressionType ComparisonType { get; private set; }
    public SqlExpression Left { get; private set; }
    public SqlExpression Right { get; private set; }

    public override void Write(SqlWriter writer)
    {
      string op = default(string);
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