using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Core;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SqlServer;
using FlitBit.Emit;

namespace FlitBit.Data.DataModel
{
  /// <summary>
  /// Builds SQL commands over a data model.
  /// </summary>
  /// <typeparam name="TDataModel">data model's type</typeparam>
  /// <typeparam name="TIdentityKey"></typeparam>
  /// <typeparam name="TDbConnection">db connection type</typeparam>
  /// <typeparam name="TJoin"></typeparam>
  public abstract class DataModelJoinCommandBuilder<TDataModel, TIdentityKey, TDbConnection, TJoin> : DataModelJoinCommandBuilder<TDataModel, TIdentityKey>,
    IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin>
  {
    Constraints _constraints;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="queryKey">the query's key</param>
    /// <param name="sqlWriter">a writer</param>
    protected DataModelJoinCommandBuilder(IDataModelBinder<TDataModel, TIdentityKey> binder,  string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
      : base(binder, queryKey, sqlWriter)
    {
    }

    /// <summary>
    /// Indicates whether the join has been defined.
    /// </summary>
    public bool HasJoin { get { return _constraints != null; } }

    /// <summary>
    /// Specifies join conditions for the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <param name="predicate">a predicate expression defining the joinery between the types</param>
    /// <returns></returns>
    public IDataModelJoinCommandBuilder<TDataModel, TDbConnection, TJoin> Join(
      Expression<Func<TDataModel, TJoin, bool>> predicate)
    {
      ExplicitJoinTypes.Clear();
      var cns = new Constraints();
      var lambda = (LambdaExpression)predicate;
      var i = 0;
      cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));
      foreach (var arg in cns.Arguments)
      {
        ExplicitJoinTypes.Add(arg.RuntimeType);
      }
      _constraints = PrepareTranslateExpression(cns, lambda.Body);
      return this;
    }

    /// <summary>
    /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
    /// </summary>
    /// <param name="predicate">a predicate expression</param>
    /// <returns></returns>
    public IDataModelQueryCommand<TDataModel, TDbConnection, TJoin, TParam> Where<TParam>(
      Expression<Func<TDataModel, TJoin, TParam, bool>> predicate)
    {
      Contract.Requires<InvalidOperationException>(HasJoin);
      var cns = _constraints;
      var lambda = (LambdaExpression)predicate;
      var i = 0;
      cns.Arguments = lambda.Parameters.Skip(2).Select(p => new ParameterValueReference(p.Name, i++, p.Type));

      return ConstructCommandOnConstraints<TParam>(
        PrepareTranslateExpression(cns, lambda.Body)
        );
    }

    /// <summary>
    /// Builds a query command with the specified constraints.
    /// </summary>
    /// <param name="constraints"></param>
    /// <returns></returns>
    protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TJoin, TParam> ConstructCommandOnConstraints<TParam>(
      Constraints constraints);
  }

  /// <summary>
  /// Builds SQL commands over a data model.
  /// </summary>
  /// <typeparam name="TDataModel">data model's type</typeparam>
  /// <typeparam name="TIdentityKey">the data model's identity key type</typeparam>
  public class DataModelJoinCommandBuilder<TDataModel, TIdentityKey>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly DataModelSqlWriter<TDataModel> _sqlWriter;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    readonly List<Type> _explicitJoinTypes = new List<Type>(); 

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="queryKey">the query's key.</param>
    /// <param name="sqlWriter">a writer</param>
    protected DataModelJoinCommandBuilder(IDataModelBinder<TDataModel, TIdentityKey> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
    {
      Contract.Requires<ArgumentNullException>(queryKey != null);
      Contract.Requires<ArgumentException>(queryKey.Length > 0);
      Contract.Requires<ArgumentNullException>(sqlWriter != null);
      this.QueryKey = queryKey;
      this._sqlWriter = sqlWriter;
      this.Binder = binder;
      this.Mapping = binder.Mapping;
    }

    /// <summary>
    /// Data model types that are explicitly joined.
    /// </summary>
    protected List<Type> ExplicitJoinTypes { get { return _explicitJoinTypes; } }

    /// <summary>
    /// The data model's binder.
    /// </summary>
    public IDataModelBinder<TDataModel, TIdentityKey> Binder { get; set; }

    /// <summary>
    /// Gets the data model's mapping.
    /// </summary>
    public IMapping<TDataModel> Mapping { get; private set; }

    /// <summary>
    /// Gets the builder's sql writer.
    /// </summary>
    public DataModelSqlWriter<TDataModel> Writer { get { return this._sqlWriter; } }

    /// <summary>
    /// The query's key.
    /// </summary>
    public string QueryKey { get; private set; }

    /// <summary>
    /// Evaluates the provided expression and builds query constraints.
    /// </summary>
    /// <param name="cns"></param>
    /// <param name="expr"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual Constraints PrepareTranslateExpression(Constraints cns, Expression expr)
    {
      cns.Writer.Append(this._sqlWriter.Select.Text);

      var binary = expr as BinaryExpression;
      if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      cns.Conditions = HandleBinaryExpression(binary, cns, false);
      this._sqlWriter.PrepareFromAndWhereStatement(this._sqlWriter.SelfRef, false, cns, cns.Writer);
      return cns;
    }

    protected virtual Constraints PrepareTranslateJoinExpression(Constraints cns, Expression expr)
    {
      var binary = expr as BinaryExpression;
      if (binary == null) throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      cns.Conditions = HandleBinaryExpression(binary, cns, true);
      return cns;
    }

    Condition HandleBinaryExpression(BinaryExpression binary, Constraints cns, bool joining)
    {
      if (binary.NodeType == ExpressionType.AndAlso)
      {
        return ProcessAndAlso(binary, cns, joining);
      }
      if (binary.NodeType == ExpressionType.OrElse)
      {
        return ProcessOrElse(binary, cns, joining);
      }
      if (binary.NodeType == ExpressionType.Equal)
      {
        return ProcessComparison(binary, cns, joining);
      }
      if (binary.NodeType == ExpressionType.NotEqual)
      {
        return ProcessComparison(binary, cns, joining);
      }
      return default(Condition);
    }

    Condition ProcessOrElse(BinaryExpression binary, Constraints cns, bool joining)
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

      return HandleBinaryExpression(left, cns, joining).Or(HandleBinaryExpression(right, cns, joining));
    }

    Condition ProcessAndAlso(BinaryExpression binary, Constraints cns, bool joining)
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

      return HandleBinaryExpression(left, cns, joining).And(HandleBinaryExpression(right, cns, joining));
    }

    ComparisonCondition ProcessComparison(BinaryExpression binary, Constraints cns, bool joining)
    {
      var ltype = GetTypeOfValue(binary.Left);
      var rtype = GetTypeOfValue(binary.Right);
      var lhs = FormatValueReference(binary.Left, cns, joining);
      var rhs = FormatValueReference(binary.Right, cns, joining);
      // Ensure that bound values are pinned to a column comparand.
      var lcol = lhs.Column;
      var rcol = rhs.Column;
      if (lcol == null)
      {
        if (rcol != null)
        {
          lhs.AssociateColumn(rcol);
        }
      }
      else if (rcol == null)
      {
        rhs.AssociateColumn(lcol);
      }
      return new ComparisonCondition(binary.NodeType, lhs, rhs);
    }

    private Type GetTypeOfValue(Expression expr)
    {
      switch (expr.NodeType)
      {
        case ExpressionType.Constant:
          return ((ConstantExpression)expr).Type;
        case ExpressionType.MemberAccess:
          return (((MemberExpression)expr).Member).GetTypeOfValue();
        case ExpressionType.Parameter:
          return ((ParameterExpression)expr).Type;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    ValueReference FormatValueReference(Expression expr, Constraints cns, bool joining)
    {
      var mapping = this.Mapping;
      var helper = mapping.GetDbProviderHelper();
      ValueReference res;
      if (expr.NodeType == ExpressionType.MemberAccess)
      {
        var m = ((MemberExpression)expr).Member;
        if (IsMemberOfSelf(expr))
        {
          var j = AddJoinPaths(expr, cns.Joins);
          if (j == null)
          {
            var col = mapping.Columns.Single(c => c.Member == m);
            res = new ValueReference(ValueReferenceKind.Self)
            {
              Value = String.Concat(mapping.QuoteObjectName("self"), '.',
                mapping.QuoteObjectName(col.TargetName))
            };
            res.AssociateColumn(col);
          }
          else
          {
            var foreignColumn = j.Mapping.Columns.Single(c => c.Member == m);
            res = new MemberValueReference(ValueReferenceKind.Join, m)
            {
              Join = j,
              Value =
                String.Concat(mapping.QuoteObjectName(Convert.ToString(j.Ordinal)), ".",
                  mapping.QuoteObjectName(foreignColumn.TargetName))
            };
            res.AssociateColumn(foreignColumn);
          }
        }
        else if (IsExplicitJoin(expr))
        {
          var j = AddFreeJoinPaths(expr, cns.Joins);
          if (j == null)
          {
            
            var col = mapping.Columns.Single(c => c.Member == m);
            res = new ValueReference(ValueReferenceKind.Self)
            {
              Value = String.Concat(mapping.QuoteObjectName("self"), '.',
                mapping.QuoteObjectName(col.TargetName))
            };
            res.AssociateColumn(col);
          }
          else
          {
            var foreignColumn = j.Mapping.Columns.Single(c => c.Member == m);
            res = new MemberValueReference(ValueReferenceKind.Join, m)
            {
              Join = j,
              Value =
                String.Concat(mapping.QuoteObjectName(Convert.ToString(j.Ordinal)), ".",
                  mapping.QuoteObjectName(foreignColumn.TargetName))
            };
            res.AssociateColumn(foreignColumn);
          }
        }
        else
        {
          var p = AddParameter(expr, cns);
          res = new ValueReference(ValueReferenceKind.Param)
          {
            Parameter = p,
            Value = helper.FormatParameterName(p.Name)
          };
        }
      }
      else if (expr.NodeType == ExpressionType.Constant)
      {
        var c = (ConstantExpression)expr;
        if (c.Type.IsClass && c.Value == null)
        {
          res = new ValueReference(ValueReferenceKind.Null);
        }
        else
        {
          var emitter = helper.GetDbTypeEmitter(mapping, expr.Type);
          if (emitter == null)
          {
            throw new NotSupportedException(String.Concat("No emitter found for type: ", expr.Type));
          }
          res = new ValueReference(ValueReferenceKind.Constant)
          {
            Value = emitter.PrepareConstantValueForSql(c.Value)
          };
        }
      }
      else if (expr.NodeType == ExpressionType.Parameter)
      {
        var p = AddParameter(expr, cns);
        res = new ValueReference(ValueReferenceKind.Param)
        {
          Parameter = p,
          Value = helper.FormatParameterName(p.Name)
        };
      }
      else
      {
        throw new NotSupportedException(String.Concat("Expression not supported: ", expr.NodeType));
      }
      return res;
    }

    private bool IsExplicitJoin(Expression expr)
    {
      var it = expr;
      while (it.NodeType == ExpressionType.MemberAccess)
      {
        it = ((MemberExpression)it).Expression;
      }
      return it.NodeType == ExpressionType.Parameter
             && ExplicitJoinTypes.Contains(it.Type);
    }
    
    Parameter AddParameter(Expression expr, Constraints cns)
    {
      var stack = new Stack<MemberExpression>();
      var parms = cns.Parameters;
      Expression it = expr;
      while (it.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression)it);
        it = ((MemberExpression)it).Expression;
      }
      var parmExpr = it as ParameterExpression;
      if (parmExpr == null)
        throw new NotSupportedException(String.Concat("Expression not supported as a parameter source: ", it.NodeType));

      var arg = cns.Arguments.Single(a => a.Name == parmExpr.Name);
      var name = stack.Aggregate(parmExpr.Name,
        (current, m) => String.Concat(current, '_', m.Member.Name));
      Parameter p;
      if (!parms.TryGetValue(name, out p))
      {
        parms.Add(name, p = new Parameter
        {
          Name = name,
          Argument = arg,
          Members = stack.Select(m => m.Member).ToArray()
        });
      }
      return p;
    }

    Join AddJoinPaths(Expression expr, Dictionary<string, Join> joins)
    {
      var stack = new Stack<MemberExpression>();
      var item = ((MemberExpression)expr).Expression;
      while (item.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression)item);
        item = ((MemberExpression)item).Expression;
      }
      if (item.NodeType != ExpressionType.Parameter
          || item.Type != typeof(TDataModel))
        throw new NotSupportedException();
      var key = "";
      var outer = default(Join);
      IMapping fromMapping = this.Mapping;
      foreach (var it in stack)
      {
        var m = it.Member;
        key = (key.Length > 0) ? String.Concat(key, '.', m.Name) : m.Name;
        Join j;
        if (!joins.TryGetValue(key, out j))
        {
          var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
          if (toDep == null)
          {
            throw new NotSupportedException(String.Concat("A dependency path is not known from ",
              fromMapping.RuntimeType.GetReadableSimpleName(),
              " to ", m.DeclaringType.GetReadableSimpleName(), " via the property `", m.Name, "'."));
          }
          joins.Add(key,
            j = new Join
            {
              Ordinal = joins.Count,
              Key = key,
              Path = outer,
              Mapping = toDep.Target,
              Member = m
            });
        }
        fromMapping = j.Mapping;
        outer = j;
      }

      return outer;
    }

    Join AddFreeJoinPaths(Expression expr, Dictionary<string, Join> joins)
    {
      var stack = new Stack<MemberExpression>();
      var item = expr;
      while (item.NodeType == ExpressionType.MemberAccess)
      {
        stack.Push((MemberExpression)item);
        item = ((MemberExpression)item).Expression;
      }
      if (item.NodeType != ExpressionType.Parameter
          || !ExplicitJoinTypes.Contains(item.Type))
        throw new NotSupportedException();
      var key = item.Type.Name;
      var outer = default(Join);
      IMapping fromMapping = Mappings.AccessMappingFor(item.Type);
      foreach (var it in stack)
      {
        var m = it.Member;
        key = (key.Length > 0) ? String.Concat(key, '.', m.Name) : m.Name;
        Join j;
        if (!joins.TryGetValue(key, out j))
        {
          var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
          if (toDep == null)
          {
            throw new NotSupportedException(String.Concat("A dependency path is not known from ",
              fromMapping.RuntimeType.GetReadableSimpleName(),
              " to ", m.DeclaringType.GetReadableSimpleName(), " via the property `", m.Name, "'."));
          }
          joins.Add(key,
            j = new Join
            {
              Ordinal = joins.Count,
              Key = key,
              Path = outer,
              Mapping = toDep.Target,
              Member = m
            });
        }
        fromMapping = j.Mapping;
        outer = j;
      }

      return outer;
    }

    bool IsMemberOfSelf(Expression expr)
    {
      var it = expr;
      while (it.NodeType == ExpressionType.MemberAccess)
      {
        it = ((MemberExpression)it).Expression;
      }
      return it.NodeType == ExpressionType.Parameter
             && it.Type == typeof(TDataModel);
    }
  }
}