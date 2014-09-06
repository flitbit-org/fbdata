using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlitBit.Core.Collections;
using FlitBit.Data.DataModel;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data.Tests.Patterns
{
  public class NullableDateTimeInsertCommand :
    SingleUpdateQueryCommand<ITestNullableDateTime, ITestNullableDateTimeDataModel>

  {
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="offsets"></param>
    public NullableDateTimeInsertCommand(DynamicSql sql, int[] offsets)
      : base(sql, offsets) {}

    protected override void BindCommand(SqlCommand cmd, DynamicSql sql, ITestNullableDateTimeDataModel model,
      BitVector dirty,
      int[] offsets
      )
    {
      SqlParameter parameter;
      List<string> values = new List<string>();
      List<string> parms = new List<string>();

      if (dirty[offsets[2]])
      {
        values.Add("[TouchedDate]");
        parms.Add("@ITestNullableDateTime_TouchedDate");
        parameter = new SqlParameter
        {
          ParameterName = "@ITestNullableDateTime_TouchedDate",
          SqlDbType = SqlDbType.DateTime
        };
        cmd.Parameters.Add(parameter);
        DateTime? termName = model.TouchedDate;
        if (termName.HasValue)
        {
          parameter.Value = termName.Value;
        }
        else
        {
          parameter.Value = DBNull.Value;
        }
      }

      cmd.CommandText = string.Format(sql.Text, string.Join(",\r\n\t", values), string.Join(",\r\n\t", parms));
    }
  }
}