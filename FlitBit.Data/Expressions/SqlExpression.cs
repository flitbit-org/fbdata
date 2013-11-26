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
    private int _joinOrd = 0;

    readonly Dictionary<Expression, SqlExpression> _translations =
      new Dictionary<Expression, SqlExpression>();

    readonly List<SqlParameterExpression> _params = new List<SqlParameterExpression>();
    readonly List<SqlJoinExpression> _explicitJoins = new List<SqlJoinExpression>();
    readonly List<SqlJoinExpression> _impliedJoins = new List<SqlJoinExpression>();


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

    public IList<SqlParameterExpression> ValueParameters { get { return _params.AsReadOnly(); } }

    public IList<SqlJoinExpression> ExplicitJoins { get { return _explicitJoins.AsReadOnly(); } }

    internal void AddSelfParameter(ParameterExpression parm)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter == null);

      SelfParameter = new SqlParameterExpression(SqlExpressionKind.Self, _selfRef, parm.Type);
      _translations.Add(parm, SelfParameter);
    }

    internal void DuplicateSelfParameter(ParameterExpression parm)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter != null);
      _translations.Add(parm, SelfParameter);
    }

    internal void AddJoinParameter(ParameterExpression parm, bool inferOnExpression)
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
      var ord = _joinOrd++;
      expr.Join = new SqlJoinExpression(parm.Type, ord, joinMapping.DbObjectReference, asAlias, onExpression);
      _explicitJoins.Add(expr.Join);
      expr.Join.ReferenceExpression = expr;
    }

    internal void DuplicateJoinParameter(int ord, ParameterExpression parm)
    {
      Contract.Requires<ArgumentOutOfRangeException>(ExplicitJoins.Count > ord && ord >= 0);

      var join = _explicitJoins[ord];
      var existing =
        _translations.Values.First(
          it => it.Kind == SqlExpressionKind.Join && ((SqlJoinParameterExpression) it).Join == join);
      _translations.Add(parm, existing);
    }

    internal SqlParameterExpression AddValueParameter(ParameterExpression parm)
    {
      var res = new SqlParameterExpression(SqlExpressionKind.Parameter,
        Mapping.GetDbProviderHelper().FormatParameterName(parm.Name), parm.Type);
      _translations.Add(parm, res);
      _params.Add(res);
      return res;
    }

    internal void DuplicateValueParameter(int ord, ParameterExpression parm)
    {
      Contract.Requires<ArgumentOutOfRangeException>(ValueParameters.Count > ord && ord >= 0);
      Contract.Requires<ArgumentOutOfRangeException>(ValueParameters[ord].Type == parm.Type);

      _translations.Add(parm, _params[0]);
    }

    internal void IngestJoinExpression(ParameterExpression parm, Expression expr)
    {
      SqlExpression joinParm;
      if (!_translations.TryGetValue(parm, out joinParm) || joinParm.Kind != SqlExpressionKind.Join)
      {
        throw new ArgumentException("Parameter expression must identify a join already added to the SQL expression.", "parm");
      }
      var binary = expr as BinaryExpression;
      if (binary == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      }
      var onStatement = HandleBinaryExpression(binary);
      ((SqlJoinParameterExpression)joinParm).Join.AddExpression(onStatement);
    }

    internal void IngestExpression(Expression expr)
    {
      var binary = expr as BinaryExpression;
      if (binary == null)
      {
        throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      }
      var where = HandleBinaryExpression(binary);
      if (where != null
          && _joinOrd > 0)
      {
        foreach (var ea in _explicitJoins.Concat(_impliedJoins).OrderBy(j => j.Ordinal))
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

    SqlExpression OptimizeJoinConditionsFromWhere(SqlJoinExpression @join, SqlExpression where)
    {
      var res = where;
      if (res.Kind == SqlExpressionKind.AndAlso
          || res.Kind == SqlExpressionKind.OrElse
          || res.Kind == SqlExpressionKind.Comparison)
      {
        res = RecursiveDescentTryOptimizeJoin(@join, (SqlBinaryExpression)res);
      }
      return res;
    }

    SqlExpression RecursiveDescentTryOptimizeJoin(SqlJoinExpression @join, SqlBinaryExpression binary)
    {
      var left = binary.Left;
      var right = binary.Right;
      if (binary.Kind == SqlExpressionKind.AndAlso)
      {
        if (RecursiveCanLift(@join, left))
        {
          if (RecursiveCanLift(@join, right))
          {
            @join.AddExpression(binary);
            return null;
          }
          @join.AddExpression(binary.Left);
          if (right is SqlBinaryExpression)
          {
            return RecursiveDescentTryOptimizeJoin(@join, (SqlBinaryExpression)right);
          }
          return binary.Right;
        }
        if (left is SqlBinaryExpression)
        {
          left = RecursiveDescentTryOptimizeJoin(@join, (SqlBinaryExpression)left);
        }
        if (RecursiveCanLift(@join, right))
        {
          @join.AddExpression(right);
          return left;
        }
        if (right is SqlBinaryExpression)
        {
          return new SqlAndAlsoExpression(left,
            RecursiveDescentTryOptimizeJoin(@join, (SqlBinaryExpression)right)
            );
        }
      }
      if (RecursiveCanLift(@join, binary))
      {
        @join.AddExpression(binary);
        return null;
      }
      return binary;
    }

    bool RecursiveCanLift(SqlJoinExpression join, SqlExpression expr)
    {
      if (expr.Kind == SqlExpressionKind.AndAlso
          || expr.Kind == SqlExpressionKind.OrElse
          || expr.Kind == SqlExpressionKind.Comparison)
      {
        return RecursiveCanLift(join, ((SqlBinaryExpression)expr).Left)
               && RecursiveCanLift(join, ((SqlBinaryExpression)expr).Right);
      }
      var value = expr as SqlValueExpression;
      if (value != null)
      {
        if (value.Source.Kind == SqlExpressionKind.Join || value.Source.Kind == SqlExpressionKind.JoinReference)
        {
          return (value.Source == join.ReferenceExpression || value.Source.Join.Ordinal < join.Ordinal);
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

      if (lhs is SqlColumnTranslatedExpression
        && ((SqlColumnTranslatedExpression)lhs).Column == null
        && rhs.Kind == SqlExpressionKind.MemberAccess)
      {
        ((SqlColumnTranslatedExpression)lhs).Column = ((SqlMemberAccessExpression)rhs).Column;
      }
      if (rhs is SqlColumnTranslatedExpression
        && ((SqlColumnTranslatedExpression)rhs).Column == null
        && lhs.Kind == SqlExpressionKind.MemberAccess)
      {
        ((SqlColumnTranslatedExpression)rhs).Column = ((SqlMemberAccessExpression)lhs).Column;
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
      if (expr.NodeType == ExpressionType.Constant)
      {
        SqlExpression res;
        if (!_translations.TryGetValue(expr, out res))
        {
          var value = ((ConstantExpression)expr).Value;
          if (expr.Type.IsClass
              || expr.Type.IsInterface)
          {
            res = new SqlNullExpression(expr.Type);
          }
          else
          {
            res = new SqlConstantExpression(expr.Type, value);
          }
          _translations.Add(expr, res);
        }
        return (SqlValueExpression)res; 
      }
      throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ",
        expr.NodeType));
    }

    private SqlValueExpression HandleValueReferencePath(Expression expr)
    {
      var stack = new Stack<MemberExpression>();
      var item = expr;
      while (item.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression) item);
        item = ((MemberExpression) item).Expression;
      }
      if (item.NodeType != ExpressionType.Parameter)
      {
        throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ", expr.NodeType));
      }

      // The inner-most is a parameter...
      var inner = FormatValueReference(item);
      if (inner.Kind == SqlExpressionKind.Self)
      {
        return MapExpressionFromSelf(inner, stack);
      }
      return MapExpressionFromJoin(inner, stack);
    }

    private SqlValueExpression MapExpressionFromSelf(SqlValueExpression first, Stack<MemberExpression> stack)
    {
      IMapping fromMapping = Mapping;
      var path = "self";
      var inner = first;
      while (stack.Count > 0)
      {
        var it = stack.Pop();
        var m = it.Member;
        SqlExpression res;
        if (_translations.TryGetValue(it, out res))
        {
          inner = (SqlValueExpression) res;
          fromMapping = (inner.Type == typeof (TDataModel)) ? Mapping : Mappings.AccessMappingFor(inner.Type);
        }
        else
        {
          var col = fromMapping.Columns.Single(c => c.Member == m);
          if (inner.Kind == SqlExpressionKind.MemberAccess && ((SqlMemberAccessExpression) inner).Column.IsReference)
          {
            inner = InferJoinForMember((SqlMemberAccessExpression) inner, path);
          }
          if (col.IsIdentity
              && (inner.Kind == SqlExpressionKind.MemberAccess
                  && ((SqlMemberAccessExpression) inner).Column.ReferenceTargetMember == m))
          {
            // special case for identity references.
            _translations.Add(it, inner);
          }
          else
          {
            inner = new SqlMemberAccessExpression(fromMapping.QuoteObjectName(col.TargetName), col.RuntimeType, col, inner);
            if (col.IsReference)
            {
              fromMapping = Mappings.AccessMappingFor(col.RuntimeType);
              path = path + "." + m.Name;
            }
            _translations.Add(it, inner);
          }
        }
      }
      return inner;
    }

    private SqlValueExpression InferJoinForMember(SqlMemberAccessExpression item, string path)
    {
      if (item.Join == null)
      {
        var captureName = Mapping.QuoteObjectName(path);
        var join = _impliedJoins.SingleOrDefault(j => j.AsAlias == captureName);
        if (join == null)
        {
          var col = item.Column;
          var foreignMapping = Mappings.AccessMappingFor(col.ReferenceTargetType);
          var foreignCol = foreignMapping.Columns.Single(c => c.Member == col.ReferenceTargetMember);
          join = new SqlJoinExpression(foreignMapping.RuntimeType, _joinOrd++, foreignMapping.DbObjectReference,
            captureName, null);
          join.AddExpression(
            new SqlComparisonExpression(ExpressionType.Equal,
              item,
              new SqlMemberAccessExpression(Mapping.QuoteObjectName(foreignCol.TargetName), foreignCol.RuntimeType,
                foreignCol,
                join.ReferenceExpression))
            );
          _impliedJoins.Add(join);
        }
        item.Join = join;
      }
      return item.Join.ReferenceExpression;
    }

    private SqlValueExpression MapExpressionFromJoin(SqlValueExpression inner, Stack<MemberExpression> stack)
    {
      var item = inner;
      var fromMapping = (item.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(item.Type);
      while (stack.Count > 0)
      {
        var it = stack.Pop();
        SqlExpression res;
        if (_translations.TryGetValue(it, out res))
        {
          item = (SqlValueExpression)res;
          fromMapping = (item.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(item.Type);
        }
        else
        {
          var m = it.Member;
          if (it.Type.IsClass || it.Type.IsInterface && Mappings.ExistsFor(it.Type))
          {
            var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
            if (toDep == null)
            {
              throw new NotSupportedException(String.Concat("A dependency path is not known from ",
                fromMapping.RuntimeType.GetReadableSimpleName(),
                " to ", it.Type.GetReadableSimpleName(), " via the property `", m.Name, "'."));
            }
            var toCol = fromMapping.Columns.Single(c => c.Member == m);
            var text = Mapping.QuoteObjectName(toCol.TargetName);
            item = new SqlMemberAccessExpression(text, it.Type, toCol, item);
            fromMapping = (it.Type == typeof(TDataModel)) ? Mapping : Mappings.AccessMappingFor(it.Type);
          }
          else
          {
            var toCol = fromMapping.Columns.Single(c => c.Member == m);
            if (toCol.IsIdentity
                && (item is SqlMemberAccessExpression
                    && ((SqlMemberAccessExpression)item).Column.ReferenceTargetMember == m))
            {
              // special case for identity references.
              _translations.Add(it, item);
            }
            else
            {
              var text = Mapping.QuoteObjectName(toCol.TargetName);
              item = new SqlMemberAccessExpression(text, it.Type, toCol, item);
              _translations.Add(it, item);
            }
          }
        }
      }
      return item;
    }

    public string Text
    {
      get
      {
        var writer = new SqlWriter();
        Write(writer);
        return writer.Text;
      }
    }

    public void Write(SqlWriter writer)
    {
      Contract.Requires<InvalidOperationException>(SelfParameter != null);

      foreach (var join in _explicitJoins.Concat(_impliedJoins).OrderBy(j => j.Ordinal))
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
    JoinReference = 3,
    Parameter = 4,
    Constant = 5,
    Null = 6,
    MemberAccess = 7,
    Comparison = 8,
    AndAlso = 9,
    OrElse = 10,
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
    private SqlValueExpression _reference;

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
      {
        OnExpression = new SqlAndAlsoExpression(OnExpression, expr);
      }
      else
      {
        OnExpression = expr;
      }
    }

    public SqlValueExpression ReferenceExpression
    {
      get
      {
        if (_reference == null)
        {
          _reference = new SqlValueExpression(SqlExpressionKind.JoinReference, AsAlias, Type);
        }
        return _reference;
      }
      internal set
      {
        _reference = value;
      }
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

    /// <summary>
    /// For values mapped onto joins, gets the join.
    /// </summary>
    public SqlJoinExpression Join { get; internal set; }

    public override void Write(SqlWriter writer) { writer.Append(_text); }
  }


  public class SqlColumnTranslatedExpression : SqlValueExpression
  {
    public SqlColumnTranslatedExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    { }

    public ColumnMapping Column { get; set; }
  }

  public class SqlConstantExpression : SqlColumnTranslatedExpression
  {
    public SqlConstantExpression(Type type, object value)
      : base(SqlExpressionKind.Constant, Convert.ToString(value), type)
    {
      Value = value;
    }

    public object Value { get; private set; }

    public override void Write(SqlWriter writer)
    {
      if (Column != null)
      {
        writer.Append(Column.Emitter.PrepareConstantValueForSql(Value));
        return;
      }
      base.Write(writer);
    }
  }

  public class SqlNullExpression : SqlColumnTranslatedExpression
  {
    public SqlNullExpression(Type type)
      : base(SqlExpressionKind.Null, "NULL", type)
    {
    }
  }

  public class SqlParameterExpression : SqlColumnTranslatedExpression
  {
    public SqlParameterExpression(SqlExpressionKind nodeType, string text, Type type)
      : base(nodeType, text, type)
    {}
  }

  public class SqlJoinParameterExpression : SqlParameterExpression
  {
    public SqlJoinParameterExpression(string text, Type type)
      : base(SqlExpressionKind.Join, text, type)
    {}

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