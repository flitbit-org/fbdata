#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data.Expressions
{
    internal static class DataModelSqlExpression
    {
        static int __idSeed;
        internal static int NextID { get { return Interlocked.Increment(ref __idSeed); } }
    }

    /// <summary>
    ///     An SQL expression builder/helper for target type; translates limited lambda expressions to SQL.
    /// </summary>
    /// <typeparam name="TDataModel">target type TDataModel</typeparam>
    public class DataModelSqlExpression<TDataModel>
    {
        readonly int _id = DataModelSqlExpression.NextID;

        readonly string _selfRef;
        int _joinOrd;

        readonly Dictionary<Expression, SqlExpression> _translations =
            new Dictionary<Expression, SqlExpression>();

        readonly List<SqlParameterExpression> _params = new List<SqlParameterExpression>();
        readonly List<SqlJoinExpression> _explicitJoins = new List<SqlJoinExpression>();
        readonly List<SqlJoinExpression> _impliedJoins = new List<SqlJoinExpression>();

        SqlExpression _whereExpression;
        Func<DataModelSqlExpression<TDataModel>, OrderByBuilder> _orderByFactory;
        Delegate _orderByAction;

        public DataModelSqlExpression(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel> binder, string selfRef)
            : this(mapping, binder, selfRef, null, null)
        {}

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="mapping">the target type's mapping</param>
        /// <param name="binder">the target type's binder</param>
        /// <param name="selfRef">a string reference to self used in the expression</param>
        public DataModelSqlExpression(IMapping<TDataModel> mapping, IDataModelBinder<TDataModel> binder, string selfRef,
            Func<DataModelSqlExpression<TDataModel>, OrderByBuilder> orderByFactory, Delegate orderByAction)
        {

            this.Mapping = mapping;
            this.Binder = binder;
            this._selfRef = selfRef;
            SetOrderBy(orderByFactory, orderByAction);
            this.SelfReferenceColumn = mapping.GetPreferredReferenceColumn();
        }

        /// <summary>
        ///     The query expression's unique id.
        /// </summary>
        public int ID { get { return _id; } }

        /// <summary>
        ///     The expression's object-relational mapping.
        /// </summary>
        public IMapping<TDataModel> Mapping { get; private set; }

        /// <summary>
        ///     The expression's object-relational binder.
        /// </summary>
        public IDataModelBinder<TDataModel> Binder { get; private set; }

        ColumnMapping SelfReferenceColumn { get; set; }

        /// <summary>
        ///     Gets the parameter expression refering to the current target object (self).
        /// </summary>
        public SqlParameterExpression SelfParameter { get; private set; }

        /// <summary>
        ///     Gets the parameter expressions bound to value parameters in the lambda.
        /// </summary>
        public IList<SqlParameterExpression> ValueParameters { get { return this._params.AsReadOnly(); } }

        /// <summary>
        ///     Gets join expressions generated from explicit lambda joins.
        /// </summary>
        public IList<SqlJoinExpression> ExplicitJoins { get { return this._explicitJoins.AsReadOnly(); } }

        /// <summary>
        ///     Gets the where expression.
        /// </summary>
        public SqlExpression WhereExpression { get { return this._whereExpression; } }

        internal void AddSelfParameter(ParameterExpression parm)
        {
            Contract.Requires<InvalidOperationException>(this.SelfParameter == null);

            this.SelfParameter = new SqlParameterExpression(SqlExpressionKind.Self, this._selfRef, parm.Type);
            this._translations.Add(parm, this.SelfParameter);
        }

        internal void DuplicateSelfParameter(ParameterExpression parm)
        {
            Contract.Requires<InvalidOperationException>(this.SelfParameter != null);
            this._translations.Add(parm, this.SelfParameter);
        }

        internal void AddJoinParameter(ParameterExpression parm, bool inferOnExpression)
        {
            Contract.Requires<InvalidOperationException>(this.SelfParameter != null);

            var asAlias = this.Mapping.QuoteObjectName(parm.Name);
            var expr = new SqlJoinParameterExpression(asAlias, parm.Type);
            var joinMapping = Mappings.AccessMappingFor(parm.Type);
            SqlExpression onExpression = null;
            if (inferOnExpression)
            {
                var dep =
                    joinMapping.Dependencies.SingleOrDefault(
                        d => d.Target == this.Mapping && d.Kind.HasFlag(DependencyKind.Direct));
                if (dep == null)
                {
                    throw new NotSupportedException(String.Concat("A direct dependency path is not known from ",
                        joinMapping.RuntimeType.GetReadableSimpleName(),
                        " to ", typeof(TDataModel).GetReadableSimpleName(), "."));
                }
                var fromCol = joinMapping.Columns.Single(c => c.Member == dep.Member);
                var toCol = this.Mapping.Columns.Single(c => c.Member == fromCol.ReferenceTargetMember);
                onExpression = new SqlComparisonExpression(ExpressionType.Equal,
                    new SqlMemberAccessExpression(this.Mapping.QuoteObjectName(fromCol.TargetName), toCol.RuntimeType,
                        fromCol,
                        expr),
                    new SqlMemberAccessExpression(this.Mapping.QuoteObjectName(toCol.TargetName), toCol.RuntimeType,
                        toCol,
                        this.SelfParameter)
                    );
            }
            this._translations.Add(parm, expr);
            var ord = this._joinOrd++;
            expr.Join = new SqlJoinExpression(parm.Type, ord, joinMapping.DbObjectReference, asAlias, onExpression);
            this._explicitJoins.Add(expr.Join);
            expr.Join.ReferenceExpression = expr;
        }

        internal void SetOrderBy(Func<DataModelSqlExpression<TDataModel>, OrderByBuilder> orderByFactory,
            Delegate orderByAction)
        {
            if (_orderByFactory != null)
            {
                throw new InvalidOperationException("OrderBy clause already exists and cannot be reset.");
            }
            if (orderByFactory != null
                && orderByAction == null)
            {
                throw new ArgumentNullException("orderByAction", "OrderBy requires both a factory and an action.");
            }
            _orderByFactory = orderByFactory;
            _orderByAction = orderByAction;
        }

        internal void DuplicateJoinParameter(int ord, ParameterExpression parm)
        {
            Contract.Requires<ArgumentOutOfRangeException>(this.ExplicitJoins.Count > ord && ord >= 0);

            var join = this._explicitJoins[ord];
            var existing =
                this._translations.Values.First(
                    it => it.Kind == SqlExpressionKind.Join && ((SqlJoinParameterExpression)it).Join == join);
            this._translations.Add(parm, existing);
        }

        internal SqlParameterExpression AddValueParameter(ParameterExpression parm)
        {
            var res = new SqlParameterExpression(SqlExpressionKind.Parameter,
                this.Mapping.GetDbProviderHelper().FormatParameterName(parm.Name), parm.Type);
            this._translations.Add(parm, res);
            this._params.Add(res);
            return res;
        }

        internal void DuplicateValueParameter(int ord, ParameterExpression parm)
        {
            Contract.Requires<ArgumentOutOfRangeException>(this.ValueParameters.Count > ord && ord >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(this.ValueParameters[ord].Type == parm.Type);

            this._translations.Add(parm, this._params[0]);
        }

        internal void IngestJoinExpression(ParameterExpression parm, Expression expr)
        {
            SqlExpression joinParm;
            if (!this._translations.TryGetValue(parm, out joinParm)
                || joinParm.Kind != SqlExpressionKind.Join)
            {
                throw new ArgumentException(
                    "Parameter expression must identify a join already added to the SQL expression.",
                    "parm");
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
                && this._joinOrd > 0)
            {
                foreach (var ea in this._explicitJoins.Concat(this._impliedJoins).OrderBy(j => j.Ordinal))
                {
                    where = OptimizeJoinConditionsFromWhere(ea, where);
                    if (where == null)
                    {
                        break;
                    }
                }
            }
            this._whereExpression = where;
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
                if (value.Source.Kind == SqlExpressionKind.Join
                    || value.Source.Kind == SqlExpressionKind.JoinReference)
                {
                    return (value.Source == join.ReferenceExpression
                            || (value.Source.Join != null && value.Source.Join.Ordinal < join.Ordinal));
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
            if (binary.NodeType == ExpressionType.GreaterThan)
            {
                return ProcessComparison(binary);
            }
            if (binary.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return ProcessComparison(binary);
            }
            if (binary.NodeType == ExpressionType.LessThan)
            {
                return ProcessComparison(binary);
            }
            if (binary.NodeType == ExpressionType.LessThanOrEqual)
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
            this._translations.Add(binary, res);
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
            this._translations.Add(binary, res);
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

            var selfRef = this.SelfReferenceColumn;
            if (selfRef != null)
            {
                if (lhs.Kind == SqlExpressionKind.Self
                    && (rhs is SqlMemberAccessExpression
                        && ((SqlMemberAccessExpression)rhs).Column.ReferenceTargetMember
                        == selfRef.Member))
                {
                    lhs = new SqlMemberAccessExpression(
                        this.Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, lhs);
                }
                if (rhs.Kind == SqlExpressionKind.Self
                    && (lhs is SqlMemberAccessExpression
                        && ((SqlMemberAccessExpression)lhs).Column.ReferenceTargetMember
                        == selfRef.Member))
                {
                    rhs = new SqlMemberAccessExpression(
                        this.Mapping.QuoteObjectName(selfRef.TargetName), selfRef.Member.GetTypeOfValue(), selfRef, rhs);
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
                this._translations.TryGetValue(expr, out res);
                return (SqlValueExpression)res;
            }
            if (expr.NodeType == ExpressionType.Constant)
            {
                SqlExpression res;
                if (!this._translations.TryGetValue(expr, out res))
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
                    this._translations.Add(expr, res);
                }
                return (SqlValueExpression)res;
            }
            if (expr.NodeType == ExpressionType.Convert)
            {
                var conv = (UnaryExpression)expr;
                if (conv.IsLifted
                    && conv.IsLiftedToNull)
                {
                    // lifted to a nullable, use the underlying...
                    return FormatValueReference(conv.Operand);
                }
            }
            throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ",
                expr.NodeType));
        }

        internal SqlValueExpression HandleValueReferencePath(Expression expr)
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
                throw new NotSupportedException(String.Concat("Expression not supported as a value expression: ",
                    expr.NodeType));
            }

            // The inner-most is a parameter...
            var inner = FormatValueReference(item);
            if (inner.Kind == SqlExpressionKind.Self)
            {
                return MapExpressionFromSelf(inner, stack);
            }
            return MapExpressionFromJoin(inner, stack);
        }

        SqlValueExpression MapExpressionFromSelf(SqlValueExpression first, Stack<MemberExpression> stack)
        {
            IMapping fromMapping = this.Mapping;
            var path = "self";
            var inner = first;
            while (stack.Count > 0)
            {
                var it = stack.Pop();
                var m = it.Member;
                SqlExpression res;
                if (this._translations.TryGetValue(it, out res))
                {
                    inner = (SqlValueExpression)res;
                    fromMapping = (inner.Type == typeof(TDataModel))
                                      ? this.Mapping
                                      : Mappings.AccessMappingFor(inner.Type);
                }
                else
                {
                    var col = fromMapping.Columns.Single(c => c.Member == m);
                    if (inner.Kind == SqlExpressionKind.MemberAccess
                        && ((SqlMemberAccessExpression)inner).Column.IsReference)
                    {
                        inner = InferJoinForMember((SqlMemberAccessExpression)inner, path);
                    }
                    if (col.IsIdentity
                        && (inner.Kind == SqlExpressionKind.MemberAccess
                            && ((SqlMemberAccessExpression)inner).Column.ReferenceTargetMember == m))
                    {
                        // special case for identity references.
                        this._translations.Add(it, inner);
                    }
                    else
                    {
                        inner = new SqlMemberAccessExpression(fromMapping.QuoteObjectName(col.TargetName),
                            col.RuntimeType, col,
                            inner);
                        if (col.IsReference)
                        {
                            fromMapping = Mappings.AccessMappingFor(col.RuntimeType);
                            path = path + "." + m.Name;
                        }
                        this._translations.Add(it, inner);
                    }
                }
            }
            return inner;
        }

        SqlValueExpression InferJoinForMember(SqlMemberAccessExpression item, string path)
        {
            if (item.Join == null)
            {
                var captureName = this.Mapping.QuoteObjectName(path);
                var join = this._impliedJoins.SingleOrDefault(j => j.AsAlias == captureName);
                if (join == null)
                {
                    var col = item.Column;
                    var foreignMapping = Mappings.AccessMappingFor(col.ReferenceTargetType);
                    var foreignCol = foreignMapping.Columns.Single(c => c.Member == col.ReferenceTargetMember);
                    join = new SqlJoinExpression(foreignMapping.RuntimeType, this._joinOrd++,
                        foreignMapping.DbObjectReference,
                        captureName, null);
                    join.AddExpression(
                        new SqlComparisonExpression(ExpressionType.Equal,
                            item,
                            new SqlMemberAccessExpression(this.Mapping.QuoteObjectName(foreignCol.TargetName),
                                foreignCol.RuntimeType,
                                foreignCol,
                                join.ReferenceExpression))
                        );
                    this._impliedJoins.Add(join);
                }
                item.Join = join;
            }
            return item.Join.ReferenceExpression;
        }

        SqlValueExpression MapExpressionFromJoin(SqlValueExpression inner, Stack<MemberExpression> stack)
        {
            var item = inner;
            var fromMapping = (item.Type == typeof(TDataModel)) ? this.Mapping : Mappings.AccessMappingFor(item.Type);
            while (stack.Count > 0)
            {
                var it = stack.Pop();
                SqlExpression res;
                if (this._translations.TryGetValue(it, out res))
                {
                    item = (SqlValueExpression)res;
                    fromMapping = (item.Type == typeof(TDataModel))
                                      ? this.Mapping
                                      : Mappings.AccessMappingFor(item.Type);
                }
                else
                {
                    var m = it.Member;
                    if (it.Type.IsClass
                        || it.Type.IsInterface && Mappings.ExistsFor(it.Type))
                    {
                        var toDep = fromMapping.Dependencies.SingleOrDefault(d => d.Member == m);
                        if (toDep == null)
                        {
                            throw new NotSupportedException(String.Concat("A dependency path is not known from ",
                                fromMapping.RuntimeType.GetReadableSimpleName(),
                                " to ", it.Type.GetReadableSimpleName(), " via the property `", m.Name, "'."));
                        }
                        var toCol = fromMapping.Columns.Single(c => c.Member == m);
                        var text = this.Mapping.QuoteObjectName(toCol.TargetName);
                        item = new SqlMemberAccessExpression(text, it.Type, toCol, item);
                        fromMapping = (it.Type == typeof(TDataModel))
                                          ? this.Mapping
                                          : Mappings.AccessMappingFor(it.Type);
                    }
                    else
                    {
                        var toCol = fromMapping.Columns.Single(c => c.Member == m);
                        if (toCol.IsIdentity
                            && (item is SqlMemberAccessExpression
                                && ((SqlMemberAccessExpression)item).Column.ReferenceTargetMember == m))
                        {
                            // special case for identity references.
                            this._translations.Add(it, item);
                        }
                        else
                        {
                            var text = this.Mapping.QuoteObjectName(toCol.TargetName);
                            item = new SqlMemberAccessExpression(text, it.Type, toCol, item);
                            this._translations.Add(it, item);
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
            Contract.Requires<InvalidOperationException>(this.SelfParameter != null);

            foreach (var join in this._explicitJoins.Concat(this._impliedJoins).OrderBy(j => j.Ordinal))
            {
                join.Write(writer);
            }
            if (this._whereExpression != null)
            {
                writer.NewLine().Append("WHERE ").Indent();
                this._whereExpression.Write(writer);
                writer.Outdent();
            }
        }

        public OrderBy OrderByStatement(OrderBy defa)
        {
            if (_orderByFactory != null)
            {
                var builder = _orderByFactory(this);
                builder.InvokeOrderByDelegate(_orderByAction);
                return builder.ToOrderByStatement();
            }
            return defa;
        }
    }
}