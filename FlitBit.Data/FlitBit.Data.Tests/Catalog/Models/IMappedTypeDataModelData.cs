using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlitBit.Core.Collections;
using FlitBit.Data.Catalog;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Tests.Catalog.Models
{
	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct IMappedTypeDataModelData : IEquatable<IMappedTypeDataModelData>
	{
		static readonly int CHashCodeSeed;	 		

		public BitVector DirtyFlags;
		public int IMappedType_ID;
		public Type IMappedType_RuntimeType;
		public IMappedType IMappedType_MappedBaseType;
		public string IMappedType_Catalog;
		public string IMappedType_Schema;
		public string IMappedType_MappedTable;
		public string IMappedType_ReadObjectName;
		public MappingStrategy IMappedType_Strategy;
		public string IMappedType_OriginalVersion;
		public string IMappedType_LatestVersion;
		public DateTime IMappedType_DateCreated;
		public DateTime IMappedType_DateUpdated;
		public IEnumerable<IMappedType> IMappedType_RegisteredSubtypes;
		
		public override bool Equals(object obj)
		{
			return (typeof(IMappedTypeDataModelData).IsInstanceOfType(obj) && this.Equals((IMappedTypeDataModelData)obj));
		}

		public override int GetHashCode()
		{
			int num = 0xf3e9b;
			int num2 = CHashCodeSeed * num;
			num2 ^= num * this.IMappedType_ID;
			num2 ^= num * this.IMappedType_RuntimeType.GetHashCode();
			num2 ^= num * this.IMappedType_MappedBaseType.GetHashCode();
			if (this.IMappedType_Catalog != null)
			{
				num2 ^= num * this.IMappedType_Catalog.GetHashCode();
			}
			if (this.IMappedType_Schema != null)
			{
				num2 ^= num * this.IMappedType_Schema.GetHashCode();
			}
			if (this.IMappedType_MappedTable != null)
			{
				num2 ^= num * this.IMappedType_MappedTable.GetHashCode();
			}
			if (this.IMappedType_ReadObjectName != null)
			{
				num2 ^= num * this.IMappedType_ReadObjectName.GetHashCode();
			}
			num2 ^= num * (int)this.IMappedType_Strategy;
			if (this.IMappedType_OriginalVersion != null)
			{
				num2 ^= num * this.IMappedType_OriginalVersion.GetHashCode();
			}
			if (this.IMappedType_LatestVersion != null)
			{
				num2 ^= num * this.IMappedType_LatestVersion.GetHashCode();
			}
			num2 ^= num * this.IMappedType_DateCreated.GetHashCode();
			num2 ^= num * this.IMappedType_DateUpdated.GetHashCode();
			return (num2 ^ (num * this.IMappedType_RegisteredSubtypes.GetHashCode()));
		}

		static IMappedTypeDataModelData()
		{
			CHashCodeSeed = typeof(IMappedTypeDataModelData).AssemblyQualifiedName.GetHashCode();
		}

		public bool WriteIMappedType_ID(int value)
		{
			if (this.IMappedType_ID != value)
			{
				this.IMappedType_ID = value;
				this.DirtyFlags[0] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_RuntimeType(Type value)
		{
			if (!(this.IMappedType_RuntimeType == value))
			{
				this.IMappedType_RuntimeType = value;
				this.DirtyFlags[1] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_MappedBaseType(IMappedType value)
		{
			if (!object.Equals(this.IMappedType_MappedBaseType, value))
			{
				this.IMappedType_MappedBaseType = value;
				this.DirtyFlags[2] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_Catalog(string value)
		{
			if (!(this.IMappedType_Catalog == value))
			{
				this.IMappedType_Catalog = value;
				this.DirtyFlags[3] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_Schema(string value)
		{
			if (!(this.IMappedType_Schema == value))
			{
				this.IMappedType_Schema = value;
				this.DirtyFlags[4] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_MappedTable(string value)
		{
			if (!(this.IMappedType_MappedTable == value))
			{
				this.IMappedType_MappedTable = value;
				this.DirtyFlags[5] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_ReadObjectName(string value)
		{
			if (!(this.IMappedType_ReadObjectName == value))
			{
				this.IMappedType_ReadObjectName = value;
				this.DirtyFlags[6] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_Strategy(MappingStrategy value)
		{
			if (this.IMappedType_Strategy != value)
			{
				this.IMappedType_Strategy = value;
				this.DirtyFlags[7] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_OriginalVersion(string value)
		{
			if (!(this.IMappedType_OriginalVersion == value))
			{
				this.IMappedType_OriginalVersion = value;
				this.DirtyFlags[8] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_LatestVersion(string value)
		{
			if (!(this.IMappedType_LatestVersion == value))
			{
				this.IMappedType_LatestVersion = value;
				this.DirtyFlags[9] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_DateCreated(DateTime value)
		{
			if (!(this.IMappedType_DateCreated == value))
			{
				this.IMappedType_DateCreated = value;
				this.DirtyFlags[10] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_DateUpdated(DateTime value)
		{
			if (!(this.IMappedType_DateUpdated == value))
			{
				this.IMappedType_DateUpdated = value;
				this.DirtyFlags[11] = true;
				return true;
			}
			return false;
		}

		public bool WriteIMappedType_RegisteredSubtypes(IEnumerable<IMappedType> value)
		{
			if (!object.Equals(this.IMappedType_RegisteredSubtypes, value))
			{
				this.IMappedType_RegisteredSubtypes = value;
				this.DirtyFlags[12] = true;
				return true;
			}
			return false;
		}

		public bool Equals(IMappedTypeDataModelData other)
		{
			return this.IMappedType_ID == other.IMappedType_ID
				&& this.IMappedType_RuntimeType == other.IMappedType_RuntimeType
				&& object.Equals(this.IMappedType_MappedBaseType, other.IMappedType_MappedBaseType)
				&& this.IMappedType_Catalog == other.IMappedType_Catalog
				&& this.IMappedType_Schema == other.IMappedType_Schema
				&& this.IMappedType_MappedTable == other.IMappedType_MappedTable
				&& this.IMappedType_ReadObjectName == other.IMappedType_ReadObjectName
				&& this.IMappedType_Strategy == other.IMappedType_Strategy
				&& this.IMappedType_OriginalVersion == other.IMappedType_OriginalVersion
				&& this.IMappedType_LatestVersion == other.IMappedType_LatestVersion
				&& this.IMappedType_DateCreated == other.IMappedType_DateCreated
				&& this.IMappedType_DateUpdated == other.IMappedType_DateUpdated
				&& object.Equals(this.IMappedType_RegisteredSubtypes, other.IMappedType_RegisteredSubtypes);
		}

		internal static IMappedTypeDataModelData Create()
		{
			return new IMappedTypeDataModelData { DirtyFlags = new BitVector(14) };
		}

		internal IMappedTypeDataModelData Copy()
		{
			IMappedTypeDataModelData res = this;
			res.DirtyFlags = this.DirtyFlags.Copy();				
			return res;
		}

		public static bool operator ==(IMappedTypeDataModelData rhs, IMappedTypeDataModelData data1)
		{
			return rhs.Equals(data1);
		}

		public static bool operator !=(IMappedTypeDataModelData rhs, IMappedTypeDataModelData data1)
		{
			return !rhs.Equals(data1);
		}
	}

}
