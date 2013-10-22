using System.Data;
using System.Data.SqlClient;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.SqlServer
{
	/// <summary>
	/// Abstract base; command intended to produce a single instance result.
	/// </summary>
	/// <typeparam name="TDataModel"></typeparam>
	/// <typeparam name="TImpl"></typeparam>
	/// <typeparam name="TParams"></typeparam>
	public abstract class SingleResultQueryCommand<TDataModel, TImpl, TParams> : IDataModelQuerySingleCommand<TDataModel, SqlConnection, TParams>
		where TImpl : TDataModel, IDataModel, new()
	{
		readonly string _commandText;
		readonly int[] _offsets;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="commandText">Initial command text.</param>
		/// <param name="offsets">column offsets within the results returned by the command</param>
		protected SingleResultQueryCommand(string commandText, int[] offsets)
		{
			_commandText = commandText;
			this._offsets = offsets;
		} 
		
		/// <summary>
		/// Executes the command with the specified parameters. 
		/// </summary>
		/// <param name="cx">A db context.</param>
		/// <param name="cn">A db connection.</param>
		/// <param name="parameters">the parameters for the command.</param>
		/// <returns>A single data model result.</returns>
		/// <exception cref="DuplicateObjectException">thrown when the command results in more than one data model.</exception>
		public TDataModel ExecuteSingle(IDbContext cx, SqlConnection cn, TParams parameters)
		{
			TImpl res = default(TImpl);
			using (var cmd = cn.CreateCommand(_commandText, CommandType.Text))
			{
				BindCommand((SqlCommand)cmd, parameters, this._offsets);
				cmd.Prepare();
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new TImpl();
						res.LoadFromDataReader(reader, this._offsets);
					}
					if (reader.Read()) throw new DuplicateObjectException();
				}
			}
			return res;
		}

		/// <summary>
		/// Implemented by specialized classes to bind the parameters to the command.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="parameters"></param>
		/// <param name="offsets"></param>
		protected abstract void BindCommand(SqlCommand cmd, TParams parameters, int[] offsets);
	}
}
