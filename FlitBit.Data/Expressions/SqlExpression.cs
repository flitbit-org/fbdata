using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FlitBit.Core;
using FlitBit.Data.CodeContracts;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SqlServer;
using FlitBit.Emit;

namespace FlitBit.Data.Expressions
{
  public class DataModelSqlExpression<TDataModel>
  {
    readonly Dictionary<Expression, SqlExpression> _translations =
      new Dictionary<Expression, SqlExpression>();

    string _selfRef;

    public DataModelSqlExpression(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel> binder, string selfRef)
    {
      this.Mapping = mapping;
      this.Binder = binder;
      this._selfRef = selfRef;
      this.SelfReferenceColumn = mapping.GetPreferredReferenceColumn();
    }

    public IMapping<TDataModel> Mapping { get; private set; }

    public IDataModelBinder<TDataModel> Binder { get; private set; }


    internal void SelfParameter(ParameterExpression parm)
    {
      _translations.Add(parm, new SqlParameterExpression(SqlExpressionKind.Self, _selfRef, parm.Type));
    }

    internal void JoinParameter(ParameterExpression parm)
    {
      _translations.Add(parm,
        new SqlParameterExpression(SqlExpressionKind.Join, Mapping.QuoteObjectName(parm.Name), parm.Type));
    }

    internal void AddValueParameter(ParameterExpression parm)
    {
      _translations.Add(parm,
        new SqlParameterExpression(SqlExpressionKind.Parameter, Mapping.GetDbProviderHelper().FormatParameterName(parm.Name), parm.Type));
    }

    internal void IngestExpresion(Expression expr)
    {
      var binary = expr as BinaryExpression;
      if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      HandleBinaryExpression(binary);
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
      var selfRef = SelfReferenceColumn;
      if (selfRef != null)
      {
        if (lhs.Kind == SqlExpressionKind.Self
            && (rhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression)rhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          lhs = new SqlMemberAccessExpression(SqlExpressionKind.MemberAccess, 
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, lhs);
        }
        if (rhs.Kind == SqlExpressionKind.Self
            && (lhs is SqlMemberAccessExpression
                && ((SqlMemberAccessExpression)lhs).Column.ReferenceTargetMember
                == selfRef.Member))
        {
          rhs = new SqlMemberAccessExpression(SqlExpressionKind.MemberAccess,
            Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, rhs);
        }
      }
      return new SqlComparisonExpression(binary.NodeType, lhs, rhs);
    }

    ColumnMapping SelfReferenceColumn { get; set; }

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
        throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ", expr.NodeType));

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
            inner = new SqlMemberAccessExpression(SqlExpressionKind.MemberAccess, text, it.Type, toCol, inner);
            fromMapping = (it.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(it.Type);
          }
          else
          {
            var toCol = fromMapping.Columns.Single(c => c.Member == m);
            if (toCol.IsIdentity
                && (inner is SqlMemberAccessExpression
                    && ((SqlMemberAccessExpression)inner).Column.ReferenceTargetMember == m))
            { // special case for identity references.
              _translations.Add(it, inner);
            }
            else
            {
              var text = Mapping.QuoteObjectName(toCol.TargetName);
              inner = new SqlMemberAccessExpression(SqlExpressionKind.MemberAccess, text, it.Type, toCol, inner);
              _translations.Add(it, inner);
            }
          }
        }
      }
      return inner;
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
      this.Kind = nodeType;
    }
    public SqlExpressionKind Kind { get; private set; }

    public string Text
    {
      get
      {
        var writer = new SqlWriter();
        this.Write(writer);
        return writer.Text;
      }
    }

    public abstract void Write(SqlWriter writer);
  }

  public class SqlValueExpression : SqlExpression
  {
    string _text;

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
    public SqlMemberAccessExpression(SqlExpressionKind nodeType, string text, Type type,
      ColumnMapping col, SqlValueExpression expr)
      : base(nodeType, text, type)
    {
      Column = col;
      Expression = expr;
    }

    public ColumnMapping Column { get; set; }

    public override void Write(SqlWriter writer)
    {
      Expression.Write(writer);
      writer.Append('.');
      base.Write(writer);
    }

    public SqlValueExpression Expression { get; set; }
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
      var op = default(string);
      switch (ComparisonType)
      {
        case ExpressionType.Equal:
          if (Left.Kind == SqlExpressionKind.Null)
          {
            if (Right.Kind == SqlExpressionKind.Null)
            {
              writer.Append("(1 = 1)"); // dumb, but writer expects a condition and writing such an expression is likewise.
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
              writer.Append("(1 = 0)"); // dumb, but writer expects a condition and writing such an expression is likewise.
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
