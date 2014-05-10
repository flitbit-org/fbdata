#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data;

namespace FlitBit.Data.Expressions
{
  public class SqlJoinExpression : SqlExpression
  {
    private SqlValueExpression _reference;

    public SqlJoinExpression(Type joinType, int ordinal, string dbObjectReference, string asAlias,
      SqlExpression onExpression)
      : base(SqlExpressionKind.Join)
    {
      this.Ordinal = ordinal;
      this.Type = joinType;
      this.DbObjectReference = dbObjectReference;
      this.AsAlias = asAlias;
      this.OnExpression = onExpression;
    }

    public int Ordinal { get; internal set; }

    public Type Type { get; protected set; }

    public SqlExpression OnExpression { get; private set; }

    public string AsAlias { get; set; }

    public string DbObjectReference { get; set; }

    public void AddExpression(SqlExpression expr)
    {
      if (this.OnExpression != null)
      {
        this.OnExpression = new SqlAndAlsoExpression(this.OnExpression, expr);
      }
      else
      {
        this.OnExpression = expr;
      }
    }

    public SqlValueExpression ReferenceExpression
    {
      get
      {
        if (this._reference == null)
        {
          this._reference = new SqlValueExpression(SqlExpressionKind.JoinReference, this.AsAlias, this.Type);
        }
        return this._reference;
      }
      internal set
      {
        this._reference = value;
      }
    }

    public override void Write(SqlWriter writer)
    {
      if (this.OnExpression == null)
      {
        throw new InvalidExpressionException("Join expression does not have a joining constraint (SQL ON statement): "
                                             + this.DbObjectReference + " AS " + this.AsAlias + ". " +
                                             "Expressions are evaluated as a binary tree, in order. Use parentheses to ensure the order is as intended and push join conditions as far left as possible to optimize join statements.");
      }
      writer.Indent()
            .NewLine().Append("JOIN ").Append(this.DbObjectReference).Append(" AS ").Append(this.AsAlias);
      writer.Indent()
            .NewLine().Append("ON ");
      this.OnExpression.Write(writer);
      writer
        .Outdent()
        .Outdent();
    }
  }
}