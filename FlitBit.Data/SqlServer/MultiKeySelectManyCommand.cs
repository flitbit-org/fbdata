using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	public abstract class MultiKeySelectManyCommand<TModel, TImpl> : IDataModelQueryManyCommand<TModel, SqlConnection>
		where TImpl : TModel, IDataModel, new()
	{
		readonly string _selectAll;
		readonly string _selectPage;
		readonly int[] _offsets;

		protected MultiKeySelectManyCommand(Mapping<TModel> mapping)
		{
			var idCols = mapping.Identity.Columns.ToArray();
			var columns = mapping.Columns.ToArray();
			var len = columns.Length;
			this._offsets = new int[len];
			var columnList = new StringBuilder(80 * len);
			var i = 0;
			for (; i < len; ++i)
			{
				this._offsets[i] = i;
				if (i > 0)
				{
					columnList.Append(",").Append(Environment.NewLine).Append("       ");
				}
				columnList.Append(mapping.QuoteObjectNameForSQL(columns[i].TargetName)).Append(" AS ").Append(columns[i].Emitter.GetDbTypeDetails(columns[i]).BindingName);
			}
			var builder = new StringBuilder(400);
			builder.Append("SELECT ").Append(columnList);
			builder.Append(Environment.NewLine).Append("FROM ").Append(mapping.DbObjectReference);
			this._selectAll = builder.ToString();


			var orderBy = new StringBuilder("ORDER BY ", 200);
			var inverseOrderBy = new StringBuilder("ORDER BY ", 200);
			i = 0;
			foreach (var id in idCols)
			{
				if (i++ > 0)
				{
					orderBy.Append(", ");
					inverseOrderBy.Append(", ");
				}
				orderBy.Append(mapping.QuoteObjectNameForSQL(id.TargetName));
				inverseOrderBy.Append(mapping.QuoteObjectNameForSQL(id.TargetName)).Append(" DESC");
			}
			builder.Clear();
			builder.Append(@"DECLARE @endRow INT = @startRow + (@limit - 1)
;WITH Cols
AS
(
SELECT ").Append(columnList)
				.Append(',').Append(Environment.NewLine).Append("ROW_NUMBER() OVER(").Append(orderBy).Append(") as Seq")
				.Append(',').Append(Environment.NewLine).Append("ROW_NUMBER() OVER(").Append(inverseOrderBy).Append(") as Inv")
				.Append(Environment.NewLine).Append("FROM ").Append(mapping.DbObjectReference)
				.Append(Environment.NewLine).Append(@"
)
SELECT TOP (@limit) ").Append(columnList)
				.Append(@"       Inv + Seq - 1 AS TotalRows
FROM Cols
WHERE Seq >= @startRow
ORDER BY Seq");

			this._selectPage = builder.ToString();
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
			
			return new DataModelQueryResult<TModel>(behavior, res);
		}
	}
}