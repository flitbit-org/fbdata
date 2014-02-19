using System;

namespace FlitBit.Data.Expressions
{
  public class SqlConstantExpression : SqlColumnTranslatedExpression
  {
    public SqlConstantExpression(Type type, object value)
      : base(SqlExpressionKind.Constant, Convert.ToString(value), type)
    {
      this.Value = value;
    }

    public object Value { get; private set; }

    public override void Write(SqlWriter writer)
    {
      if (this.Column != null)
      {
        writer.Append(this.Column.Emitter.PrepareConstantValueForSql(this.Value));
        return;
      }
      base.Write(writer);
    }
  }
}