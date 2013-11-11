using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using FlitBit.Core.Collections;
using FlitBit.Data.Catalog;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;

namespace FlitBit.Data.Tests.Catalog.Models
{
	
    [Serializable]
    public sealed class IMappedTypeDataModel : INotifyPropertyChanged, IMappedType, IEquatable<IMappedTypeDataModel>, IEquatable<IMappedType>, IDataModel
    {
        private static readonly string[] __fieldMap = new string[] { "ID", "DateCreated", "DateUpdated", "Catalog", "LatestVersion", "MappedBaseType", "MappedTable", "OriginalVersion", "ReadObjectName", "RuntimeType", "Schema", "Strategy", "Active" };
        /* private scope */ BitVector DirtyFlags = new BitVector(14);
	    private object _sync = new object();
        private bool? IMappedType_Active_field;
        private string IMappedType_Catalog_field;
        private DateTime IMappedType_DateCreated_field;
        private DateTime IMappedType_DateUpdated_field;
        private int IMappedType_ID_field;
        private string IMappedType_LatestVersion_field;
        private DataModelReference<IMappedType, int> IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>();
        private string IMappedType_MappedTable_field;
        private string IMappedType_OriginalVersion_field;
        private string IMappedType_ReadObjectName_field;
				private DataModelCollectionReference<IMappedType, int> IMappedType_RegisteredSubtypes_field;
        private Type IMappedType_RuntimeType_field;
        private string IMappedType_Schema_field;
        private MappingStrategy IMappedType_Strategy_field;
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChanged;
        private static readonly int CHashCodeSeed = typeof(IMappedTypeDataModel).AssemblyQualifiedName.GetHashCode();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler handler2;
                PropertyChangedEventHandler handler = this._propertyChanged;
                do
                {
                    handler2 = handler;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler) Delegate.Combine(handler2, value);
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
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler) Delegate.Remove(handler2, value);
                    handler = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this._propertyChanged, handler3, handler2);
                }
                while (handler == handler2);
            }
        }

        public IMappedTypeDataModel()
        {
        }

	    private void IMappedType_RegisteredSubtypes_field_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.DirtyFlags[9] = true;
            this.HandlePropertyChanged("RegisteredSubtypes");
        }

        public object Clone()
        {
            IMappedTypeDataModel model = (IMappedTypeDataModel) base.MemberwiseClone();
            model.DirtyFlags = this.DirtyFlags.Copy();
            model._propertyChanged = null;
						if (IMappedType_RegisteredSubtypes_field != null)
	        model.IMappedType_RegisteredSubtypes_field = IMappedType_RegisteredSubtypes_field.Clone(model.IMappedType_RegisteredSubtypes_field_CollectionChanged);
            return model;
        }

        public bool Equals(IMappedType other)
        {
            return ((other is IMappedTypeDataModel) && this.Equals((IMappedTypeDataModel) other));
        }

	    public bool Equals(IMappedTypeDataModel other)
	    {
		    return ((((((this.DirtyFlags == other.DirtyFlags) && (this.IMappedType_ID_field == other.IMappedType_ID_field)) &&
		               ((this.IMappedType_DateCreated_field == other.IMappedType_DateCreated_field) &&
		                (this.IMappedType_DateUpdated_field == other.IMappedType_DateUpdated_field))) &&
		              (((this.IMappedType_Catalog_field == other.IMappedType_Catalog_field) &&
		                (this.IMappedType_LatestVersion_field == other.IMappedType_LatestVersion_field)) &&
		               (EqualityComparer<DataModelReference<IMappedType, int>>.Default.Equals(
			               this.IMappedType_MappedBaseType_field, other.IMappedType_MappedBaseType_field) &&
		                (this.IMappedType_MappedTable_field == other.IMappedType_MappedTable_field)))) &&
		             ((((this.IMappedType_OriginalVersion_field == other.IMappedType_OriginalVersion_field) &&
		                (this.IMappedType_ReadObjectName_field == other.IMappedType_ReadObjectName_field)) &&
		               (this.IMappedType_RegisteredSubtypes_field.Equals(other.IMappedType_RegisteredSubtypes_field) &&
		                (this.IMappedType_RuntimeType_field == other.IMappedType_RuntimeType_field))) &&
		              ((this.IMappedType_Schema_field == other.IMappedType_Schema_field) &&
		               (this.IMappedType_Strategy_field == other.IMappedType_Strategy_field)))) &&
		            Nullable.Equals<bool>(this.IMappedType_Active_field, other.IMappedType_Active_field));
	    }

	    public override bool Equals(object obj)
        {
            return ((obj is IMappedTypeDataModel) && this.Equals((IMappedTypeDataModel) obj));
        }

        public BitVector GetDirtyFlags()
        {
            return (BitVector) this.DirtyFlags.Clone();
        }

        public override int GetHashCode()
        {
            int num = 0xf3e9b;
            int num2 = CHashCodeSeed * num;
            num2 ^= num * this.DirtyFlags.GetHashCode();
            num2 ^= num * this.IMappedType_ID_field;
            num2 ^= num * this.IMappedType_DateCreated_field.GetHashCode();
            num2 ^= num * this.IMappedType_DateUpdated_field.GetHashCode();
            if (this.IMappedType_Catalog_field != null)
            {
                num2 ^= num * this.IMappedType_Catalog_field.GetHashCode();
            }
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
            num2 ^= num * Convert.ToInt32(this.IMappedType_Strategy_field);
            return (num2 ^ (num * this.IMappedType_Active_field.GetHashCode()));
        }

        public TIdentityKey GetReferentID<TIdentityKey>(string member)
        {
            if (string.Equals("MappedBaseType", member))
            {
                return (TIdentityKey) Convert.ChangeType(this.IMappedType_MappedBaseType_field.IdentityKey, typeof(TIdentityKey));
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            throw new ArgumentOutOfRangeException("member", "IMappedType does not reference: " + member + ".");
        }

        /* private scope */ void HandlePropertyChanged(string propName)
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
		    this.IMappedType_ID_field = reader.GetInt32(ordinal);
				ordinal = offsets[1];
		    this.IMappedType_DateCreated_field = reader.GetDateTime(ordinal);
		    ordinal = offsets[2];
		    this.IMappedType_DateUpdated_field = reader.GetDateTime(ordinal);
		    ordinal = offsets[3];
		    this.IMappedType_Catalog_field = reader.GetString(ordinal);
		    ordinal = offsets[4];
		    this.IMappedType_LatestVersion_field = reader.GetString(ordinal);
		    ordinal = offsets[5];
		    this.IMappedType_MappedBaseType_field = reader.IsDBNull(ordinal)
			    ? new DataModelReference<IMappedType, int>()
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
		    this.IMappedType_Strategy_field = (MappingStrategy) reader.GetInt32(ordinal);
		    ordinal = offsets[12];
		    this.IMappedType_Active_field = reader.IsDBNull(ordinal)
			    ? null
			    : ((bool?) new SqlBoolean(reader.GetBoolean(ordinal)));
		    this.DirtyFlags = new BitVector(14);
	    }

	    public void ResetDirtyFlags()
        {
            this.DirtyFlags = new BitVector(14);
        }

        public void SetReferentID<TIdentityKey>(string member, TIdentityKey id)
        {
            if (string.Equals("MappedBaseType", member))
            {
                if (!this.IMappedType_MappedBaseType_field.IdentityEquals(id))
                {
                    this.IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>(Convert.ToInt32(id));
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }

        public bool? Active
        {
            get
            {
                return this.IMappedType_Active_field;
            }
            set
            {
                if (!Nullable.Equals<bool>(this.IMappedType_Active_field, value))
                {
                    this.IMappedType_Active_field = value;
                    this.DirtyFlags[12] = true;
                    this.HandlePropertyChanged("Active");
                }
            }
        }

        public string Catalog
        {
            get
            {
                return this.IMappedType_Catalog_field;
            }
            set
            {
                if (this.IMappedType_Catalog_field != value)
                {
                    this.IMappedType_Catalog_field = value;
                    this.DirtyFlags[3] = true;
                    this.HandlePropertyChanged("Catalog");
                }
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
                if (this.IMappedType_MappedTable_field != value)
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
                if (this.IMappedType_OriginalVersion_field != value)
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
                if (this.IMappedType_ReadObjectName_field != value)
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
			    if (IMappedType_RegisteredSubtypes_field == null)
			    {
				    lock (_sync)
				    {
					    if (IMappedType_RegisteredSubtypes_field == null)
					    {
						    IMappedType_RegisteredSubtypes_field = new DataModelCollectionReference<IMappedType, int>("RegisteredSubtypes",
							    IMappedType_RegisteredSubtypes_field_CollectionChanged, ID);
					    }
				    }
			    }
					return IMappedType_RegisteredSubtypes_field.GetCollection();
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
                if (this.IMappedType_RuntimeType_field != value)
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
                if (this.IMappedType_Schema_field != value)
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
    }
}
