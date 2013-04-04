using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using FlitBit.Copy;
using FlitBit.Core;
using FlitBit.Core.Collections;
using FlitBit.Data.Catalog;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.Tests.Catalog.Models
{
	public class AllMappedTypeCommand : IDataModelQueryManyCommand<IMappedType, SqlConnection>
	{
		static readonly string SelectAll = @"
SELECT 
	[Catalog]
	, [DateCreated]
	, [DateUpdated]
	, [ID]
	, [LatestVersion]
	, [MappedBaseType]
	, [MappedTable]
	, [OriginalVersion]
	, [ReadObjectName]
	, [RuntimeType]
	, [Schema]
	, [Strategy]
FROM [OrmCatalog].[MappedType]
";
		static readonly string SelectTop = SelectAll.Replace("SELECT", "SELECT TOP(@query_limit)");
		static readonly string KeySetPriorCondition = @"
WHERE [ID] < @id";
		static readonly string KeySetNextCondition = @"
WHERE [ID] > @id";
		static readonly string KeySetOrderDescending = @"
ORDER BY [ID] DESC";
		static readonly string KeySetOrder = @"
ORDER BY [ID]";
		static readonly int[] ColumnOffsets = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public IDataModelQueryResult<IMappedType> ExecuteMany(IDbContext cx, SqlConnection cn, QueryBehavior behavior)
		{
			Tuple<int, int> currentPageCorrelationKey = null;
			var res = new List<IMappedType>();
			cn.EnsureConnectionIsOpen();
			var query = behavior.IsLimited ? SelectTop : SelectAll;
			if (behavior.IsPaging)
			{
				if (behavior.PageCorrelationKey != null)
				{
					currentPageCorrelationKey = (Tuple<int, int>)behavior.PageCorrelationKey;
					query = String.Concat(query, behavior.Backward ? KeySetPriorCondition : KeySetNextCondition);
				}
				query = String.Concat(query, behavior.Backward ? KeySetOrderDescending : KeySetOrder);
			}

			using (var cmd = cn.CreateCommand(query, CommandType.Text))
			{
				if (behavior.IsLimited)
				{
					var pageSizeParam = new SqlParameter("@query_limit", SqlDbType.Int);
					pageSizeParam.Value = behavior.PageSize;
					cmd.Parameters.Add(pageSizeParam);
				}
				if (currentPageCorrelationKey != null)
				{
					var idParam = new SqlParameter("@id", SqlDbType.Int);
					idParam.Value = behavior.Backward ? currentPageCorrelationKey.Item1 : currentPageCorrelationKey.Item2;
					cmd.Parameters.Add(idParam);
				}

				using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				{
					IMappedTypeDataModel model = null;

					if (behavior.IsPaging && reader.Read())
					{
						model = new IMappedTypeDataModel();
						model.LoadFromDataReader(reader, ColumnOffsets);
						currentPageCorrelationKey = Tuple.Create(model.ID, 0);
						res.Add(model);
					}
					while (reader.Read())
					{
						model = new IMappedTypeDataModel();
						model.LoadFromDataReader(reader, ColumnOffsets);
						res.Add(model);
					}
					if (behavior.IsPaging && res.Count > 1)
					{
						currentPageCorrelationKey = Tuple.Create(currentPageCorrelationKey.Item1, model.ID);
						behavior = new QueryBehavior(behavior.Behaviors, behavior.PageSize, behavior.Page, currentPageCorrelationKey,
																				false);
					}
				}
			}
			return new DataModelQueryResult<IMappedType>(behavior, res);
		}

	}

	public class CreateMappedTypeCommand : IDataModelQuerySingleCommand<IMappedType, SqlConnection, IMappedType>
	{
		static readonly string CreateFmt = @"
DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()
DECLARE @IMappedType_ID INT

INSERT INTO [OrmCatalog].[MappedType] ({0}
)
VALUES ({1}
)

SET @IMappedType_ID = SCOPE_IDENTITY()
SELECT 	
	[Catalog]
	, [DateCreated]
	, [DateUpdated]
	, [ID]
	, [LatestVersion]
	, [MappedBaseType]
	, [MappedTable]
	, [OriginalVersion]
	, [ReadObjectName]
	, [RuntimeType]
	, [Schema]
	, [Strategy]
FROM [OrmCatalog].[MappedType]
WHERE [ID] = @IMappedType_ID
";
		static readonly int[] ColumnOffsets = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public IMappedType ExecuteSingle(IDbContext cx, SqlConnection cn, IMappedType key)
		{
			if (key == null)
			{
				throw new InvalidOperationException("Cannot store a null model");
			}
			var model = key as IMappedTypeDataModel;
			if (model == null)
			{
				// delegate subclasses to their own binders... we're covariant!

				throw new InvalidOperationException("You must transform the instance to the mapped concrete type before saving.");
			}

			var dirty = model.GetDirtyFlags();
			if (dirty.TrueFlagCount == 0)
			{
				return model;
			}
			using (var cmd = cn.CreateCommand())
			{
				SqlParameter parm;
				var columns = new List<string>();
				var values = new List<string>();
				if (dirty[0])
				{
					columns.Add("[Catalog]");
					values.Add("@IMappedType_Catalog");
					parm = new SqlParameter("@IMappedType_Catalog", SqlDbType.NVarChar, 128);
					if (model.Catalog == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.Catalog);
					cmd.Parameters.Add(parm);
				}
				columns.Add("[DateCreated]");
				values.Add("@generated_timestamp");
				columns.Add("[DateUpdated]");
				values.Add("@generated_timestamp");
				if (dirty[4])
				{
					columns.Add("[LatestVersion]");
					values.Add("@IMappedType_LatestVersion");
					parm = new SqlParameter("@IMappedType_LatestVersion", SqlDbType.NVarChar, 40);
					if (model.LatestVersion == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.LatestVersion);
					cmd.Parameters.Add(parm);
				}
				if (dirty[5])
				{
					columns.Add("[MappedBaseType]");
					values.Add("@IMappedType_MappedBaseType");
					parm = new SqlParameter("@IMappedType_MappedBaseType", SqlDbType.Int);
					var mappedBaseType = model.GetReferentID<int>("MappedBaseType");
					if (EqualityComparer<int>.Default.Equals(mappedBaseType, default(int)))
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlInt32(mappedBaseType);
					cmd.Parameters.Add(parm);
				}
				if (dirty[6])
				{
					columns.Add("[MappedTable]");
					values.Add("@IMappedType_MappedTable");
					parm = new SqlParameter("@IMappedType_MappedTable", SqlDbType.NVarChar, 128);
					if (model.MappedTable == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.MappedTable);
					cmd.Parameters.Add(parm);
				}
				if (dirty[7])
				{
					columns.Add("[OriginalVersion]");
					values.Add("@IMappedType_OriginalVersion");
					parm = new SqlParameter("@IMappedType_OriginalVersion", SqlDbType.NVarChar, 40);
					if (model.OriginalVersion == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.OriginalVersion);
					cmd.Parameters.Add(parm);
				}
				if (dirty[8])
				{
					columns.Add("[ReadObjectName]");
					values.Add("@IMappedType_ReadObjectName");
					parm = new SqlParameter("@IMappedType_ReadObjectName", SqlDbType.NVarChar, 128);
					if (model.ReadObjectName == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.ReadObjectName);
					cmd.Parameters.Add(parm);
				}
				if (dirty[9])
				{
					columns.Add("[RuntimeType]");
					values.Add("@IMappedType_RuntimeType");
					parm = new SqlParameter("@IMappedType_RuntimeType", SqlDbType.NVarChar, 128);
					if (model.RuntimeType == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.RuntimeType.FullName);
					cmd.Parameters.Add(parm);
				}
				if (dirty[10])
				{
					columns.Add("[Schema]");
					values.Add("@IMappedType_Schema");
					parm = new SqlParameter("@IMappedType_Schema", SqlDbType.NVarChar, 128);
					if (model.Schema == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.Schema);
					cmd.Parameters.Add(parm);
				}
				if (dirty[11])
				{
					columns.Add("[Strategy]");
					values.Add("@IMappedType_Strategy");
					parm = new SqlParameter("@IMappedType_Strategy", SqlDbType.Int);
					parm.SqlValue = new SqlInt32((int)model.Strategy);
					cmd.Parameters.Add(parm);
				}
				cmd.CommandText = String.Format(CreateFmt, String.Join("\r\n\t, ", columns), String.Join("\r\n\t, ", values));
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						model.LoadFromDataReader(reader, ColumnOffsets);
					}
				}
			}
			return model;
		}
	}

	public class UpdateMappedTypeCommand : IDataModelQuerySingleCommand<IMappedType, SqlConnection, IMappedType>
	{
		static readonly string UpdateFmt = @"
DECLARE @generated_timestamp DATETIME2 = GETUTCDATE()

UPDATE [OrmCatalog].[MappedType] 
	SET {0}
WHERE [ID] = @IMappedType_ID

SELECT 	
	[Catalog]
	, [DateCreated]
	, [DateUpdated]
	, [ID]
	, [LatestVersion]
	, [MappedBaseType]
	, [MappedTable]
	, [OriginalVersion]
	, [ReadObjectName]
	, [RuntimeType]
	, [Schema]
	, [Strategy]
FROM [OrmCatalog].[MappedType]
WHERE [ID] = @IMappedType_ID
";
		static readonly int[] ColumnOffsets = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public IMappedType ExecuteSingle(IDbContext cx, SqlConnection cn, IMappedType key)
		{
			if (key == null)
			{
				throw new InvalidOperationException("Cannot store a null model");
			}
			var model = key as IMappedTypeDataModel;
			if (model == null)
			{
				// delegate subclasses to their own binders... we're covariant!

				throw new InvalidOperationException("You must transform the instance to the mapped concrete type before saving.");
			}

			var dirty = model.GetDirtyFlags();
			if (dirty.TrueFlagCount == 0)
			{
				return model;
			}
			using (var cmd = cn.CreateCommand())
			{
				var parm = new SqlParameter("@IMappedType_ID", SqlDbType.Int);
				parm.Value = new SqlInt32(model.ID);
				cmd.Parameters.Add(parm);

				var columns = new List<string>();
				if (dirty[0])
				{
					columns.Add("[Catalog] = @IMappedType_Catalog");
					parm = new SqlParameter("@IMappedType_Catalog", SqlDbType.NVarChar, 128);
					if (model.Catalog == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.Catalog);
					cmd.Parameters.Add(parm);
				}
				columns.Add("[DateUpdated] = @generated_timestamp");
				if (dirty[4])
				{
					columns.Add("[LatestVersion] = @IMappedType_LatestVersion");
					parm = new SqlParameter("@IMappedType_LatestVersion", SqlDbType.NVarChar, 40);
					if (model.LatestVersion == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.LatestVersion);
					cmd.Parameters.Add(parm);
				}
				if (dirty[5])
				{
					columns.Add("[MappedBaseType] = @IMappedType_MappedBaseType");
					parm = new SqlParameter("@IMappedType_MappedBaseType", SqlDbType.Int);
					var mappedBaseType = model.GetReferentID<int>("MappedBaseType");
					if (EqualityComparer<int>.Default.Equals(mappedBaseType, default(int)))
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlInt32(mappedBaseType);
					cmd.Parameters.Add(parm);
				}
				if (dirty[6])
				{
					columns.Add("[MappedTable] = @IMappedType_MappedTable");
					parm = new SqlParameter("@IMappedType_MappedTable", SqlDbType.NVarChar, 128);
					if (model.MappedTable == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.MappedTable);
					cmd.Parameters.Add(parm);
				}
				if (dirty[8])
				{
					columns.Add("[ReadObjectName] = @IMappedType_ReadObjectName");
					parm = new SqlParameter("@IMappedType_ReadObjectName", SqlDbType.NVarChar, 128);
					if (model.ReadObjectName == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.ReadObjectName);
					cmd.Parameters.Add(parm);
				}
				if (dirty[9])
				{
					columns.Add("[RuntimeType] = @IMappedType_RuntimeType");
					parm = new SqlParameter("@IMappedType_RuntimeType", SqlDbType.NVarChar, 128);
					if (model.RuntimeType == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.RuntimeType.FullName);
					cmd.Parameters.Add(parm);
				}
				if (dirty[10])
				{
					columns.Add("[Schema] = @IMappedType_Schema");
					parm = new SqlParameter("@IMappedType_Schema", SqlDbType.NVarChar, 128);
					if (model.Schema == null)
						parm.SqlValue = DBNull.Value;
					else parm.SqlValue = new SqlString(model.Schema);
					cmd.Parameters.Add(parm);
				}
				if (dirty[11])
				{
					columns.Add("[Strategy] = @IMappedType_Strategy");
					parm = new SqlParameter("@IMappedType_Strategy", SqlDbType.Int);
					parm.SqlValue = new SqlInt32((int)model.Strategy);
					cmd.Parameters.Add(parm);
				}
				cmd.CommandText = String.Format(UpdateFmt, String.Join("\r\n\t, ", columns));
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						model.LoadFromDataReader(reader, ColumnOffsets);
					}
				}
			}
			return model;
		}
	}

	public class ReadMappedTypeByIdCommand : IDataModelQuerySingleCommand<IMappedType, SqlConnection, int>
	{
		static readonly string ReadById = @"
SELECT [Catalog]
	, [DateCreated]
	, [DateUpdated]
	, [ID]
	, [LatestVersion]
	, [MappedBaseType]
	, [MappedTable]
	, [OriginalVersion]
	, [ReadObjectName]
	, [RuntimeType]
	, [Schema]
	, [Strategy]
FROM [OrmCatalog].[MappedType]
WHERE [ID] = @ID
";
		static readonly int[] ColumnOffsets = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public IMappedType ExecuteSingle(IDbContext cx, SqlConnection cn, int id)
		{
			IMappedTypeDataModel res = default(IMappedTypeDataModel);
			using (var cmd = cn.CreateCommand())
			{
				var columns = new List<string>();
				var parm = new SqlParameter("@ID", SqlDbType.Int);
				parm.Value = new SqlInt32(id);
				cmd.Parameters.Add(parm);

				cmd.CommandText = String.Format(ReadById);
				cmd.Prepare();
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new IMappedTypeDataModel();
						res.LoadFromDataReader(reader, ColumnOffsets);
					}
				}
			}
			return res;
		}
	}

	public class ReadMappedTypeByRuntimeTypeCommand : IDataModelQuerySingleCommand<IMappedType, SqlConnection, Type>
	{
		static readonly string ReadBy = @"
SELECT [Catalog]
	, [DateCreated]
	, [DateUpdated]
	, [ID]
	, [LatestVersion]
	, [MappedBaseType]
	, [MappedTable]
	, [OriginalVersion]
	, [ReadObjectName]
	, [RuntimeType]
	, [Schema]
	, [Strategy]
FROM [OrmCatalog].[MappedType]
WHERE [RuntimeType] = @IMappedType_RuntimeType
";
		static readonly int[] ColumnOffsets = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public IMappedType ExecuteSingle(IDbContext cx, SqlConnection cn, Type key)
		{
			IMappedTypeDataModel res = default(IMappedTypeDataModel);
			using (var cmd = cn.CreateCommand())
			{
				var columns = new List<string>();
				var parm = new SqlParameter("@IMappedType_RuntimeType", SqlDbType.NVarChar, 128);
				parm.Value = (key == null) ? (object)DBNull.Value : new SqlString(key.FullName);
				cmd.Parameters.Add(parm);

				cmd.CommandText = String.Format(ReadBy);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						res = new IMappedTypeDataModel();
						res.LoadFromDataReader(reader, ColumnOffsets);
					}
				}
			}
			return res;
		}
	}

	[Serializable]
	public sealed class IMappedTypeDataModel : INotifyPropertyChanged, IMappedType, IEquatable<IMappedTypeDataModel>, IEquatable<IMappedType>, IDataModel
	{
		// Fields
		private static readonly string[] __fieldMap = new string[] { "Catalog", "DateCreated", "DateUpdated", "ID", "LatestVersion", "MappedBaseType", "MappedTable", "OriginalVersion", "ReadObjectName", "RuntimeType", "Schema", "Strategy" };
		/* private scope */
		BitVector DirtyFlags = new BitVector(13);
		private string IMappedType_Catalog_field;
		private DateTime IMappedType_DateCreated_field;
		private DateTime IMappedType_DateUpdated_field;
		private int IMappedType_ID_field;
		private string IMappedType_LatestVersion_field;
		private DataModelReference<IMappedType, int> IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>(null);
		private string IMappedType_MappedTable_field;
		private string IMappedType_OriginalVersion_field;
		private string IMappedType_ReadObjectName_field;
		private ObservableCollection<IMappedType> IMappedType_RegisteredSubtypes_field;
		private Type IMappedType_RuntimeType_field;
		private string IMappedType_Schema_field;
		private MappingStrategy IMappedType_Strategy_field;
		[NonSerialized]
		private PropertyChangedEventHandler _propertyChanged;
		private static readonly int CHashCodeSeed = typeof(IMappedTypeDataModel).AssemblyQualifiedName.GetHashCode();

		// Events
		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				PropertyChangedEventHandler handler2;
				PropertyChangedEventHandler handler = this._propertyChanged;
				do
				{
					handler2 = handler;
					PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Combine(handler2, value);
					handler = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, handler3, handler2);
				}
				while (handler == handler2);
			}
			remove
			{
				PropertyChangedEventHandler handler2;
				PropertyChangedEventHandler handler = this._propertyChanged;
				do
				{
					handler2 = handler;
					PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Remove(handler2, value);
					handler = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, handler3, handler2);
				}
				while (handler == handler2);
			}
		}

		// Methods
		public IMappedTypeDataModel()
		{
			this.RegisteredSubtypes = null;
		}

		private void IMappedType_RegisteredSubtypes_field_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.DirtyFlags[9] = true;
			this.HandlePropertyChanged("RegisteredSubtypes");
		}

		public object Clone()
		{
			IMappedTypeDataModel model = (IMappedTypeDataModel)base.MemberwiseClone();
			model.DirtyFlags = this.DirtyFlags.Copy();
			model._propertyChanged = null;
			model.RegisteredSubtypes = this.IMappedType_RegisteredSubtypes_field;
			return model;
		}

		public bool Equals(IMappedType other)
		{
			return ((other is IMappedTypeDataModel) && this.Equals((IMappedTypeDataModel)other));
		}

		public bool Equals(IMappedTypeDataModel other)
		{
			return ((((((this.DirtyFlags == other.DirtyFlags) && (this.IMappedType_Catalog_field == other.IMappedType_Catalog_field)) && ((this.IMappedType_DateCreated_field == other.IMappedType_DateCreated_field) && (this.IMappedType_DateUpdated_field == other.IMappedType_DateUpdated_field))) && (((this.IMappedType_ID_field == other.IMappedType_ID_field) && (this.IMappedType_LatestVersion_field == other.IMappedType_LatestVersion_field)) && (EqualityComparer<DataModelReference<IMappedType, int>>.Default.Equals(this.IMappedType_MappedBaseType_field, other.IMappedType_MappedBaseType_field) && (this.IMappedType_MappedTable_field == other.IMappedType_MappedTable_field)))) && ((((this.IMappedType_OriginalVersion_field == other.IMappedType_OriginalVersion_field) && (this.IMappedType_ReadObjectName_field == other.IMappedType_ReadObjectName_field)) && (this.IMappedType_RegisteredSubtypes_field.SequenceEqual<IMappedType>(other.IMappedType_RegisteredSubtypes_field) && (this.IMappedType_RuntimeType_field == other.IMappedType_RuntimeType_field))) && (this.IMappedType_Schema_field == other.IMappedType_Schema_field))) && (this.IMappedType_Strategy_field == other.IMappedType_Strategy_field));
		}

		public override bool Equals(object obj)
		{
			return ((obj is IMappedTypeDataModel) && this.Equals((IMappedTypeDataModel)obj));
		}

		public BitVector GetDirtyFlags()
		{
			return (BitVector)this.DirtyFlags.Clone();
		}

		public override int GetHashCode()
		{
			int num = 0xf3e9b;
			int num2 = CHashCodeSeed * num;
			num2 ^= num * this.DirtyFlags.GetHashCode();
			if (this.IMappedType_Catalog_field != null)
			{
				num2 ^= num * this.IMappedType_Catalog_field.GetHashCode();
			}
			num2 ^= num * this.IMappedType_DateCreated_field.GetHashCode();
			num2 ^= num * this.IMappedType_DateUpdated_field.GetHashCode();
			num2 ^= num * this.IMappedType_ID_field;
			if (this.IMappedType_LatestVersion_field != null)
			{
				num2 ^= num * this.IMappedType_LatestVersion_field.GetHashCode();
			}
			num2 ^= num * this.IMappedType_MappedBaseType_field.GetHashCode();
			if (this.IMappedType_MappedTable_field != null)
			{
				num2 ^= num * this.IMappedType_MappedTable_field.GetHashCode();
			}
			if (this.IMappedType_OriginalVersion_field != null)
			{
				num2 ^= num * this.IMappedType_OriginalVersion_field.GetHashCode();
			}
			if (this.IMappedType_ReadObjectName_field != null)
			{
				num2 ^= num * this.IMappedType_ReadObjectName_field.GetHashCode();
			}
			num2 ^= num * this.IMappedType_RegisteredSubtypes_field.GetHashCode();
			num2 ^= num * this.IMappedType_RuntimeType_field.GetHashCode();
			if (this.IMappedType_Schema_field != null)
			{
				num2 ^= num * this.IMappedType_Schema_field.GetHashCode();
			}
			return (num2 ^ (num * (int)this.IMappedType_Strategy_field));
		}

		public TIdentityKey GetReferentID<TIdentityKey>(string member)
		{
			if (string.Equals("MappedBaseType", member))
			{
				return (TIdentityKey)(object)this.IMappedType_MappedBaseType_field.IdentityKey;
			}
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			throw new ArgumentOutOfRangeException("member", "IMappedType does not reference: " + member + ".");
		}

		/* private scope */
		void HandlePropertyChanged(string propName)
		{
			if (this._propertyChanged != null)
			{
				this._propertyChanged(this, new PropertyChangedEventArgs(propName));
			}
		}

		public bool IsDirty(string member)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			int index = Array.IndexOf<string>(__fieldMap, member);
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("member", "IMappedType does not define property: `" + member + "`.");
			}
			return this.DirtyFlags[index];
		}

		public void LoadFromDataReader(DbDataReader reader, int[] offsets)
		{
			int ordinal = offsets[0];
			this.IMappedType_Catalog_field = reader.GetString(ordinal);
			ordinal = offsets[1];
			this.IMappedType_DateCreated_field = reader.GetDateTime(ordinal);
			ordinal = offsets[2];
			this.IMappedType_DateUpdated_field = reader.GetDateTime(ordinal);
			ordinal = offsets[3];
			this.IMappedType_ID_field = reader.GetInt32(ordinal);
			ordinal = offsets[4];
			this.IMappedType_LatestVersion_field = reader.GetString(ordinal);
			ordinal = offsets[5];
			this.IMappedType_MappedBaseType_field = reader.IsDBNull(ordinal)
				? new DataModelReference<IMappedType, int>(null)
				: new DataModelReference<IMappedType, int>(reader.GetInt32(ordinal));
			ordinal = offsets[6];
			this.IMappedType_MappedTable_field = reader.GetString(ordinal);
			ordinal = offsets[7];
			this.IMappedType_OriginalVersion_field = reader.GetString(ordinal);
			ordinal = offsets[8];
			this.IMappedType_ReadObjectName_field = reader.GetString(ordinal);
			ordinal = offsets[9];
			this.IMappedType_RuntimeType_field = Type.GetType(reader.GetString(ordinal));
			ordinal = offsets[10];
			this.IMappedType_Schema_field = reader.GetString(ordinal);
			ordinal = offsets[11];
			this.IMappedType_Strategy_field = ((MappingStrategy)reader.GetInt32(ordinal));
			this.DirtyFlags = new BitVector(13);
		}

		public void ResetDirtyFlags()
		{
			this.DirtyFlags = new BitVector(13);
		}

		public void SetReferentID<TIdentityKey>(string member, TIdentityKey id)
		{
			if (string.Equals("MappedBaseType", member))
			{
				if (!this.IMappedType_MappedBaseType_field.IdentityEquals(id))
				{
					this.IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>((int)(object)id);
					this.DirtyFlags[5] = true;
					this.HandlePropertyChanged("MappedBaseType");
				}
			}
			else
			{
				if (member == null)
				{
					throw new ArgumentNullException("member");
				}
				throw new ArgumentOutOfRangeException("member", "IMappedType does not reference: " + member + ".");
			}
		}

		// Properties
		public string Catalog
		{
			get
			{
				return this.IMappedType_Catalog_field;
			}
			set
			{
				if (this.IMappedType_Catalog_field == value)
				{
					return;
				}
				if (value == null) throw new ValidationException(CultureInfo.CurrentUICulture.IetfLanguageTag, "Catalog must have a value.");
				if (value.Length > 128) throw new ValidationException("Catalog name cannot exceed 128 characters.");
				this.IMappedType_Catalog_field = value;
				this.DirtyFlags[0] = true;
				this.HandlePropertyChanged("Catalog");
			}
		}

		public DateTime DateCreated
		{
			get
			{
				return this.IMappedType_DateCreated_field;
			}
		}

		public DateTime DateUpdated
		{
			get
			{
				return this.IMappedType_DateUpdated_field;
			}
		}

		public int ID
		{
			get
			{
				return this.IMappedType_ID_field;
			}
		}

		public string LatestVersion
		{
			get
			{
				return this.IMappedType_LatestVersion_field;
			}
			set
			{
				if (this.IMappedType_LatestVersion_field != value)
				{
					this.IMappedType_LatestVersion_field = value;
					this.DirtyFlags[4] = true;
					this.HandlePropertyChanged("LatestVersion");
				}
			}
		}

		public IMappedType MappedBaseType
		{
			get
			{
				return this.IMappedType_MappedBaseType_field.Model;
			}
			set
			{
				if (!this.IMappedType_MappedBaseType_field.Equals(value))
				{
					this.IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>(value);
					this.DirtyFlags[5] = true;
					this.HandlePropertyChanged("MappedBaseType");
				}
			}
		}

		public string MappedTable
		{
			get
			{
				return this.IMappedType_MappedTable_field;
			}
			set
			{
				if (!(this.IMappedType_MappedTable_field == value))
				{
					this.IMappedType_MappedTable_field = value;
					this.DirtyFlags[6] = true;
					this.HandlePropertyChanged("MappedTable");
				}
			}
		}

		public string OriginalVersion
		{
			get
			{
				return this.IMappedType_OriginalVersion_field;
			}
			set
			{
				if (!(this.IMappedType_OriginalVersion_field == value))
				{
					this.IMappedType_OriginalVersion_field = value;
					this.DirtyFlags[7] = true;
					this.HandlePropertyChanged("OriginalVersion");
				}
			}
		}

		public string ReadObjectName
		{
			get
			{
				return this.IMappedType_ReadObjectName_field;
			}
			set
			{
				if (!(this.IMappedType_ReadObjectName_field == value))
				{
					this.IMappedType_ReadObjectName_field = value;
					this.DirtyFlags[8] = true;
					this.HandlePropertyChanged("ReadObjectName");
				}
			}
		}

		public IList<IMappedType> RegisteredSubtypes
		{
			get
			{
				return this.IMappedType_RegisteredSubtypes_field;
			}
			private set
			{
				if (value != null)
				{
					this.IMappedType_RegisteredSubtypes_field = new ObservableCollection<IMappedType>(value);
				}
				else
				{
					this.IMappedType_RegisteredSubtypes_field = new ObservableCollection<IMappedType>();
				}
				this.IMappedType_RegisteredSubtypes_field.CollectionChanged += new NotifyCollectionChangedEventHandler(this.IMappedType_RegisteredSubtypes_field_CollectionChanged);
			}
		}

		public Type RuntimeType
		{
			get
			{
				return this.IMappedType_RuntimeType_field;
			}
			set
			{
				if (!(this.IMappedType_RuntimeType_field == value))
				{
					this.IMappedType_RuntimeType_field = value;
					this.DirtyFlags[9] = true;
					this.HandlePropertyChanged("RuntimeType");
				}
			}
		}

		public string Schema
		{
			get
			{
				return this.IMappedType_Schema_field;
			}
			set
			{
				if (!(this.IMappedType_Schema_field == value))
				{
					this.IMappedType_Schema_field = value;
					this.DirtyFlags[10] = true;
					this.HandlePropertyChanged("Schema");
				}
			}
		}

		public MappingStrategy Strategy
		{
			get
			{
				return this.IMappedType_Strategy_field;
			}
			set
			{
				if (this.IMappedType_Strategy_field != value)
				{
					this.IMappedType_Strategy_field = value;
					this.DirtyFlags[11] = true;
					this.HandlePropertyChanged("Strategy");
				}
			}
		}

		public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
			throw new NotImplementedException();
		}
	}

}