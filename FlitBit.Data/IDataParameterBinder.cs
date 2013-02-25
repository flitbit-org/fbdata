#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
	[ContractClass(typeof(CodeContracts.ContractsForIDataParameterBinder))]
	public interface IDataParameterBinder
	{
		bool ContainsParameter(string name);
		
		int IndexOfParameter(string name);

		string PrepareParameterName(string name);

		DbTypeTranslation TranslateRuntimeType(Type type);

		IEnumerable<ParameterBinding> Bindings { get; }

		IDataParameterBinder DefineParameter(string name, Type runtimeType);
		IDataParameterBinder DefineParameter(string name, Type runtimeType, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType);
		IDataParameterBinder DefineParameter(string name, DbType dbType, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int length);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int length, ParameterDirection direction);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale);
		IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction);
		IDataParameterBinder DefineParameter(Func<DbParamDefinition> specializeParam);

		IDataParameterBinder SetParameterValue(string name, bool value);
		IDataParameterBinder SetParameterValue(string name, byte[] value);
		IDataParameterBinder SetParameterValue(string name, byte value);
		IDataParameterBinder SetParameterValue(string name, DateTime value);
		IDataParameterBinder SetParameterValue(string name, decimal value);
		IDataParameterBinder SetParameterValue(string name, Double value);
		IDataParameterBinder SetParameterValue(string name, Guid value);
		IDataParameterBinder SetParameterValue(string name, Single value);

		IDataParameterBinder SetParameterValue(string name, SByte value);
		IDataParameterBinder SetParameterValue(string name, string value);
		IDataParameterBinder SetParameterValue(string name, Int16 value);
		IDataParameterBinder SetParameterValue(string name, Int32 value);
		IDataParameterBinder SetParameterValue(string name, Int64 value);

		IDataParameterBinder SetParameterValue(string name, UInt16 value);

		IDataParameterBinder SetParameterValue(string name, UInt32 value);

		IDataParameterBinder SetParameterValue(string name, UInt64 value);
		IDataParameterBinder SetParameterValue<T>(string name, T value);
		IDataParameterBinder SetParameterValueAsEnum<E>(string name, E value);

		IDataParameterBinder SetParameterDbNull(string name);

		bool PrepareDbCommand(DbCommand command);

		void Initialize(IEnumerable<ParameterBinding> bindings);
	}

	namespace CodeContracts
	{
		/// <summary>
		/// CodeContracts Class for IDbExecutable
		/// </summary>
		[ContractClassFor(typeof(IDataParameterBinder))]
		internal abstract class ContractsForIDataParameterBinder : IDataParameterBinder
		{
			public IDataParameterBinder DefineParameter(string name, Type runtimeType)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, Type runtimeType, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, Type runtimeType, int length, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentNullException>(runtimeType != null);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType, int length)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType, int length, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(length > 0);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(size > 0);
				Contract.Requires<ArgumentOutOfRangeException>(scale >= 0);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(string name, DbType dbType, int size, byte scale, ParameterDirection direction)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<InvalidOperationException>(IndexOfParameter(name) < 0, Resources.Chk_ParameterObstructed);
				Contract.Requires<ArgumentOutOfRangeException>(size > 0);
				Contract.Requires<ArgumentOutOfRangeException>(scale >= 0);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder DefineParameter(Func<DbParamDefinition> specializeParam)
			{
				Contract.Requires<ArgumentNullException>(specializeParam != null);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, bool value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, byte[] value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, byte value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, DateTime value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, decimal value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, double value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, Guid value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, float value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, sbyte value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, string value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, short value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, int value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, long value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, ushort value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, uint value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue(string name, ulong value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValue<T>(string name, T value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterValueAsEnum<E>(string name, E value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public IDataParameterBinder SetParameterDbNull(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);
				Contract.Ensures(Contract.Result<IDataParameterBinder>() != null);

				throw new NotImplementedException();
			}

			public int IndexOfParameter(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Ensures(Contract.Result<int>() >= -1);

				throw new NotImplementedException();
			}

			public void GetParameterValueAs<T>(string name, out T value)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Requires<ArgumentOutOfRangeException>(IndexOfParameter(name) >= 0, Resources.Chk_ParameterNotDefined);

				throw new NotImplementedException();
			}

			public bool ContainsParameter(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				throw new NotImplementedException();
			}

			public string PrepareParameterName(string name)
			{
				Contract.Requires<ArgumentNullException>(name != null);
				Contract.Ensures(Contract.Result<string>() != null);

				throw new NotImplementedException();
			}

			public DbTypeTranslation TranslateRuntimeType(Type type)
			{
				throw new NotImplementedException();
			}

			public bool PrepareDbCommand(DbCommand command)
			{
				Contract.Requires<ArgumentNullException>(command != null);

				throw new NotImplementedException();
			}


			public IEnumerable<ParameterBinding> Bindings
			{
				get { throw new NotImplementedException(); }
			}

			public void Initialize(IEnumerable<ParameterBinding> bindings)
			{
				throw new NotImplementedException();
			}
		}
	}
}
