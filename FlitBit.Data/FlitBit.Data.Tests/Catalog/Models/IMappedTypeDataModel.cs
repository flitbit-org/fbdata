using System;
using System.Collections.Generic;
using FlitBit.Core.Collections;
using FlitBit.Data.Catalog;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.Tests.Catalog.Models
{
	[Serializable]
	public sealed class IMappedTypeDataModel : IMappedType, IEquatable<IMappedType>, IDataModel
	{
		// Fields
		static readonly int CHashCodeSeed = typeof(IMappedTypeDataModel).AssemblyQualifiedName.GetHashCode();
		static readonly List<string> __fieldMap;

		IMappedTypeDataModelData _data;

		static IMappedTypeDataModel()
		{
			__fieldMap = new List<string>(13);
			__fieldMap.AddRange(
												 new string[]
													 {
														 "ID",
														 "RuntimeType",
														 "MappedBaseType",
														 "Catalog",
														 "Schema",
														 "MappedTable",
														 "ReadObjectName",
														 "Strategy",
														 "OriginalVersion",
														 "LatestVersion",
														 "DateCreated",
														 "DateUpdated"
													 });
		}

		// Methods
		public IMappedTypeDataModel() { this._data = IMappedTypeDataModelData.Create(); }
		public BitVector GetDirtyFlags() { return _data.DirtyFlags; }

		public object Clone()
		{
			var clone = new IMappedTypeDataModel();
			clone._data = this._data.Copy();
			return clone;
		}

		public bool IsDirty(string name)
		{
			var i = __fieldMap.IndexOf(name);
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException("member", String.Concat("Property not defined: ", name, "."));
			}
			return _data.DirtyFlags[i];
		}

		public bool Equals(IMappedType other)
		{
			var model = other as IMappedTypeDataModel;
			return ((other != null) && this._data.Equals(model._data));
		}

		// Properties
		public string Catalog
		{
			get { return this._data.IMappedType_Catalog; }
			set { this._data.WriteIMappedType_Catalog(value); }
		}

		public DateTime DateCreated
		{
			get { return this._data.IMappedType_DateCreated; }
		}

		public DateTime DateUpdated
		{
			get { return this._data.IMappedType_DateUpdated; }
		}

		public int ID
		{
			get { return this._data.IMappedType_ID; }
		}

		public string LatestVersion
		{
			get { return this._data.IMappedType_LatestVersion; }
			set { this._data.WriteIMappedType_LatestVersion(value); }
		}

		public IMappedType MappedBaseType
		{
			get { return this._data.IMappedType_MappedBaseType.Model; }
			set
			{
				this._data.WriteIMappedType_MappedBaseType(
																									 DataModel<IMappedType>.ReferenceFactory.MakeFromReferent(value));
			}
		}

		public string MappedTable
		{
			get { return this._data.IMappedType_MappedTable; }
			set { this._data.WriteIMappedType_MappedTable(value); }
		}

		public string OriginalVersion
		{
			get { return this._data.IMappedType_OriginalVersion; }
			set { this._data.WriteIMappedType_OriginalVersion(value); }
		}

		public string ReadObjectName
		{
			get { return this._data.IMappedType_ReadObjectName; }
			set { this._data.WriteIMappedType_ReadObjectName(value); }
		}

		public IEnumerable<IMappedType> RegisteredSubtypes
		{
			get { return this._data.IMappedType_RegisteredSubtypes; }
		}

		public Type RuntimeType
		{
			get { return this._data.IMappedType_RuntimeType; }
			set { this._data.WriteIMappedType_RuntimeType(value); }
		}

		public string Schema
		{
			get { return this._data.IMappedType_Schema; }
			set { this._data.WriteIMappedType_Schema(value); }
		}

		public MappingStrategy Strategy
		{
			get { return this._data.IMappedType_Strategy; }
			set { this._data.WriteIMappedType_Strategy(value); }
		}

		public override bool Equals(object obj) { return obj is IMappedType && this.Equals((IMappedType) obj); }

		public override int GetHashCode()
		{
			var prime = 0xf3e9b;
			var res = CHashCodeSeed*prime;
			return (res ^ (this._data.GetHashCode()*prime));
		}
	}
}