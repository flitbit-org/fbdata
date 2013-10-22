using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FlitBit.Data.DataModel;
using System.Data.SqlClient;

namespace FlitBit.Data.SqlServer
{
	internal class SqlDataModelCommandBuilder<TDataModel, TImpl, TCriteria> : IDataModelCommandBuilder<TDataModel, SqlConnection, TCriteria>
	{
		public IDataModelQueryManyCommand<TDataModel, SqlConnection, TCriteria> Where(Expression<Func<TDataModel, TCriteria, bool>> expression)
		{
			var sql = prepareTranslateExpression((expression.NodeType == ExpressionType.Lambda) ? expression.Body : expression);
			return null;
		}

		private string prepareTranslateExpression(Expression expression)
		{
			if (expression is BinaryExpression)
			{
				var binary = expression as BinaryExpression;
				if (binary.Left != binary.Right)
				{
					
				}
			}
			throw new NotImplementedException();
		}
	}
}
