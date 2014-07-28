#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Linq.Expressions;

namespace FlitBit.Data.Expressions
{
  public class SqlComparisonExpression : SqlBinaryExpression
  {
    public SqlComparisonExpression(ExpressionType comparison, SqlExpression lhs, SqlExpression rhs)
      : base(SqlExpressionKind.Comparison, lhs, rhs) { this.ComparisonType = comparison; }

    public ExpressionType ComparisonType { get; private set; }

    public override void Write(SqlWriter writer)
    {
      var op = default(string);
      switch (this.ComparisonType)
      {
        case ExpressionType.Equal:
          if (this.Left.Kind == SqlExpressionKind.Null)
          {
            if (this.Right.Kind == SqlExpressionKind.Null)
            {
              writer.Append("(1 = 1)");
              // dumb, but writer expects a condition and writing such an expression is likewise.
            }
            else
            {
              writer.Append("(");
              this.Right.Write(writer);
              writer.Append(" IS NULL)");
            }
            return;
          }
          if (this.Right.Kind == SqlExpressionKind.Null || 
            (this.Right.Kind == SqlExpressionKind.Constant && 
            ((SqlConstantExpression) this.Right).Value == null))
          {
            writer.Append("(");
            this.Left.Write(writer);
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
          if (this.Left.Kind == SqlExpressionKind.Null)
          {
            if (this.Right.Kind == SqlExpressionKind.Null)
            {
              writer.Append("(1 = 0)");
              // dumb, but writer expects a condition and writing such an expression is likewise.
            }
            else
            {
              writer.Append("(");
              this.Right.Write(writer);
              writer.Append(" IS NOT NULL)");
            }
            return;
          }
          if (this.Right.Kind == SqlExpressionKind.Null ||
            (this.Right.Kind == SqlExpressionKind.Constant &&
            ((SqlConstantExpression)this.Right).Value == null))
          {
            writer.Append("(");
            this.Left.Write(writer);
            writer.Append(" IS NOT NULL)");
            return;
          }
          op = " <> ";
          break;
      }
      writer.Append("(");
      this.Left.Write(writer);
      writer.Append(op);
      this.Right.Write(writer);
      writer.Append(')');
    }
  }
}