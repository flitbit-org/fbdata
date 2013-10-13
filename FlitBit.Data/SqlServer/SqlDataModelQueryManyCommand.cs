using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	public abstract class SqlDataModelQueryManyCommand<TModel, TImpl, TCorrelationKey> : IDataModelQueryManyCommand<TModel, SqlConnection>
		where TImpl : TModel, IDataModel, new()
		where TCorrelationKey : class
	{
		readonly string _selectAll;
		readonly string _selectTop;
		readonly string _selectPage;
		readonly int[] _columnOffsets;

		protected SqlDataModelQueryManyCommand(Mapping<TModel> mapping)
		{
			var idCols = mapping.Identity.Columns.ToArray();
			var columns = mapping.Columns.ToArray();
			var len = columns.Length;
			_columnOffsets = new int[len];
			var columnList = new StringBuilder(80 * len);
			for (var i = 0; i < len; ++i)
			{
				_columnOffsets[i] = i;
				if (i > 0)
				{
					columnList.Append(",").Append(Environment.NewLine).Append("       ");
				}
				columnList.Append(mapping.QuoteObjectNameForSQL(columns[i].TargetName)).Append(" AS ").Append(columns[i].Emitter.GetDbTypeDetails(columns[i]).BindingName);
			}
			var builder = new StringBuilder(400);
			builder.Append("SELECT ").Append(columnList);
			builder.Append(Environment.NewLine).Append("FROM ").Append(mapping.DbObjectReference);
			_selectAll = builder.ToString();
			_selectTop = _selectAll.Replace("SELECT ", "SELECT TOP(@limit) ");

			if (idCols.Length == 1)
			{
				builder.Clear();
				builder.Append(@"DECLARE @first_id int
DECLARE @totrows int

SELECT @totrows = SUM (row_count)
FROM sys.dm_db_partition_stats
WHERE object_id=OBJECT_ID('").Append(mapping.DbObjectReference).Append(@"')   
AND (index_id=0 or index_id=1);

SET ROWCOUNT @startRow
SELECT @first_id = ").Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName)).Append(@" 
	FROM ").Append(mapping.DbObjectReference).Append(@"
	ORDER BY ").Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName)).Append(@"

SET ROWCOUNT @pageSize
SELECT ").Append(columnList)
					.Append(',').Append(Environment.NewLine).Append("@totrows AS TotRows")
					.Append(Environment.NewLine).Append("FROM ").Append(mapping.DbObjectReference)
					.Append(Environment.NewLine)
					.Append("WHERE ")
					.Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName))
					.Append(@" >= @first_id
ORDER BY ").Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName)).Append(@"
SET ROWCOUNT 0");
			}
			else
			{

				builder.Clear();
				builder.Append(@"DECLARE @endRow INT = @startRow + (@pageSize - 1)
;WITH cols
AS
(
SELECT ").Append(columnList)
					.Append(Environment.NewLine).Append("FROM ").Append(mapping.DbObjectReference)
					.Append(Environment.NewLine)
					.Append("WHERE ")
					.Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName))
					.Append(@" >= @first_id
ORDER BY ").Append(mapping.QuoteObjectNameForSQL(idCols[0].TargetName)).Append(@"
SET ROWCOUNT 0");
			}
			_selectPage = builder.ToString();
		}

		/*
		 DECLARE @startRow INT = 160
DECLARE @pageSize INT = 30
DECLARE @endRow INT = @startRow + (@pageSize - 1)
;WITH cols
AS
(
SELECT [BusinessEntityID]
      ,[PersonType]
      ,[NameStyle]
      ,[Title]
      ,[FirstName]
      ,[MiddleName]
      ,[LastName]
      ,[Suffix]
      ,[EmailPromotion]
      ,[AdditionalContactInfo]
      ,[Demographics]
      ,[rowguid]
      ,[ModifiedDate]
	  ,
        ROW_NUMBER() OVER(ORDER BY [LastName] ASC, [FirstName] ASC, [MiddleName] ASC) AS seq, 
        ROW_NUMBER() OVER(ORDER BY [LastName] DESC, [FirstName] DESC, [MiddleName] DESC) AS totrows
  FROM [AdventureWorks2012].[Person].[Person]
)
SELECT TOP (@pageSize) [BusinessEntityID]
      ,[PersonType]
      ,[NameStyle]
      ,[Title]
      ,[FirstName]
      ,[MiddleName]
      ,[LastName]
      ,[Suffix]
      ,[EmailPromotion]
      ,[AdditionalContactInfo]
      ,[Demographics]
      ,[rowguid]
      ,[ModifiedDate], 
	  totrows + seq -1 as TotalRows
FROM cols
WHERE seq >= @startRow
ORDER BY seq

		 */

		public IDataModelQueryResult<TModel> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior)
		{
			var paging = behavior.IsPaging;
			var limited = behavior.IsLimited;
			var backward = behavior.Backward;
			var outPage = behavior.Page;
			var res = new List<TModel>();
			var inKey = behavior.PageCorrelationKey as TCorrelationKey;
			TCorrelationKey outKey = null;
			QueryBehavior outbehavior = null;
			cn.EnsureConnectionIsOpen();
			var query = new StringBuilder(limited ? _selectTop : _selectAll);
			if (paging)
			{
				if (inKey != null)
				{
					query.Append(backward ? _keySetPriorCondition : _keySetNextCondition);
				}
				query.Append(backward ? _keySetOrderDescending : _keySetOrder);
				outPage = (backward) ? Math.Min(0, outPage - 1) : outPage + 1;
			}

			using (var cmd = cn.CreateCommand(query.ToString(), CommandType.Text))
			{
				if (limited)
				{
					var pageSizeParam = new SqlParameter("@limit", SqlDbType.Int) { Value = behavior.PageSize };
					cmd.Parameters.Add(pageSizeParam);
				}
				if (paging && inKey != null)
				{
					BindPageCorrelation((SqlCommand)cmd, backward, inKey);
				}

				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					TImpl model = default(TImpl);

					if (paging && reader.Read())
					{
						model = new TImpl();
						model.LoadFromDataReader(reader, _columnOffsets);
						outKey = MakeInitialCorrelationKey(model);
						res.Add(model);
					}
					while (reader.Read())
					{
						model = new TImpl();
						model.LoadFromDataReader(reader, _columnOffsets);
						res.Add(model);
					}
					if (paging && res.Count > 1)
					{
						if (outKey != null)
						{
							outbehavior = new QueryBehavior(behavior.Behaviors, behavior.PageSize, outPage, outKey, false);
						}
						else
						{
							outKey = UpdateCorrelationKey(inKey, backward, model);
							outbehavior = new QueryBehavior(behavior.Behaviors, behavior.PageSize, outPage, outKey, false);
						}
					}
				}
			}
			return new DataModelQueryResult<TModel>(outbehavior, res);
		}

		protected abstract TCorrelationKey UpdateCorrelationKey(TCorrelationKey inKey, bool backward, TImpl model);

		protected abstract TCorrelationKey MakeInitialCorrelationKey(TImpl model);

		protected abstract void BindPageCorrelation(SqlCommand cmd, bool backward, TCorrelationKey key);
	}

}
