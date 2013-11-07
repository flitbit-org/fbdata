using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
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
	public sealed class IMappedTypeDataModel : INotifyPropertyChanged, IMappedType, IEquatable<IMappedTypeDataModel>,
		IEquatable<IMappedType>, IDataModel
	{
		// Fields
		private static readonly string[] __fieldMap =
		{
			"Catalog", "DateCreated", "DateUpdated", "ID", "LatestVersion",
			"MappedBaseType", "MappedTable", "OriginalVersion", "ReadObjectName", "RuntimeType", "Schema", "Strategy"
		};

		private static readonly int CHashCodeSeed = typeof (IMappedTypeDataModel).AssemblyQualifiedName.GetHashCode();

		/* private scope */
		private BitVector DirtyFlags = new BitVector(13);
		private bool? IMappedType_Active_field;
		private string IMappedType_Catalog_field;
		private DateTime IMappedType_DateCreated_field;
		private DateTime IMappedType_DateUpdated_field;
		private int IMappedType_ID_field;
		private string IMappedType_LatestVersion_field;

		private DataModelReference<IMappedType, int> IMappedType_MappedBaseType_field =
			new DataModelReference<IMappedType, int>();

		private string IMappedType_MappedTable_field;
		private string IMappedType_OriginalVersion_field;
		private string IMappedType_ReadObjectName_field;
		private ObservableCollection<IMappedType> IMappedType_RegisteredSubtypes_field;
		private Type IMappedType_RuntimeType_field;
		private string IMappedType_Schema_field;
		private MappingStrategy IMappedType_Strategy_field;
		[NonSerialized] private PropertyChangedEventHandler _propertyChanged;

		// Methods
		public IMappedTypeDataModel()
		{
			RegisteredSubtypes = null;
		}

		public bool? Active
		{
			get { return IMappedType_Active_field; }
			set
			{
				if (IMappedType_Active_field != value)
				{
					IMappedType_Active_field = value;
					DirtyFlags[10] = true;
					HandlePropertyChanged("Active");
				}
			}
		}

		public object Clone()
		{
			var model = (IMappedTypeDataModel) base.MemberwiseClone();
			model.DirtyFlags = DirtyFlags.Copy();
			model._propertyChanged = null;
			model.RegisteredSubtypes = IMappedType_RegisteredSubtypes_field;
			return model;
		}

		public BitVector GetDirtyFlags()
		{
			return (BitVector) DirtyFlags.Clone();
		}

		public TIdentityKey GetReferentID<TIdentityKey>(string member)
		{
			if (string.Equals("MappedBaseType", member))
			{
				return (TIdentityKey) (object) IMappedType_MappedBaseType_field.IdentityKey;
			}
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			throw new ArgumentOutOfRangeException("member", "IMappedType does not reference: " + member + ".");
		}

		/* private scope */

		public bool IsDirty(string member)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			int index = Array.IndexOf(__fieldMap, member);
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("member", "IMappedType does not define property: `" + member + "`.");
			}
			return DirtyFlags[index];
		}

		public void LoadFromDataReader(DbDataReader reader, int[] offsets)
		{
			int ordinal = offsets[0];
			IMappedType_Catalog_field = reader.GetString(ordinal);
			ordinal = offsets[1];
			IMappedType_DateCreated_field = reader.GetDateTime(ordinal);
			ordinal = offsets[2];
			IMappedType_DateUpdated_field = reader.GetDateTime(ordinal);
			ordinal = offsets[3];
			IMappedType_ID_field = reader.GetInt32(ordinal);
			ordinal = offsets[4];
			IMappedType_LatestVersion_field = reader.GetString(ordinal);
			ordinal = offsets[5];
			IMappedType_MappedBaseType_field = reader.IsDBNull(ordinal)
				? new DataModelReference<IMappedType, int>()
				: new DataModelReference<IMappedType, int>(reader.GetInt32(ordinal));
			ordinal = offsets[6];
			IMappedType_MappedTable_field = reader.GetString(ordinal);
			ordinal = offsets[7];
			IMappedType_OriginalVersion_field = reader.GetString(ordinal);
			ordinal = offsets[8];
			IMappedType_ReadObjectName_field = reader.GetString(ordinal);
			ordinal = offsets[9];
			IMappedType_RuntimeType_field = Type.GetType(reader.GetString(ordinal));
			ordinal = offsets[10];
			IMappedType_Schema_field = reader.GetString(ordinal);
			ordinal = offsets[11];
			IMappedType_Strategy_field = ((MappingStrategy) reader.GetInt32(ordinal));
			IMappedType_Active_field = reader.IsDBNull(ordinal) ? default(bool?) : reader.GetBoolean(ordinal);
			DirtyFlags = new BitVector(13);
		}

		public void ResetDirtyFlags()
		{
			DirtyFlags = new BitVector(13);
		}

		public void SetReferentID<TIdentityKey>(string member, TIdentityKey id)
		{
			if (string.Equals("MappedBaseType", member))
			{
				if (!IMappedType_MappedBaseType_field.IdentityEquals(id))
				{
					IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>((int) (object) id);
					DirtyFlags[5] = true;
					HandlePropertyChanged("MappedBaseType");
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
			throw new NotImplementedException();
		}

		public bool Equals(IMappedType other)
		{
			return ((other is IMappedTypeDataModel) && Equals((IMappedTypeDataModel) other));
		}

		public bool Equals(IMappedTypeDataModel other)
		{
			return ((((((DirtyFlags == other.DirtyFlags) && (IMappedType_Catalog_field == other.IMappedType_Catalog_field)) &&
			           ((IMappedType_DateCreated_field == other.IMappedType_DateCreated_field) &&
			            (IMappedType_DateUpdated_field == other.IMappedType_DateUpdated_field))) &&
			          (((IMappedType_ID_field == other.IMappedType_ID_field) &&
			            (IMappedType_LatestVersion_field == other.IMappedType_LatestVersion_field)) &&
			           (EqualityComparer<DataModelReference<IMappedType, int>>.Default.Equals(IMappedType_MappedBaseType_field,
				           other.IMappedType_MappedBaseType_field) &&
			            (IMappedType_MappedTable_field == other.IMappedType_MappedTable_field)))) &&
			         ((((IMappedType_OriginalVersion_field == other.IMappedType_OriginalVersion_field) &&
			            (IMappedType_ReadObjectName_field == other.IMappedType_ReadObjectName_field)) &&
			           (IMappedType_RegisteredSubtypes_field.SequenceEqual(other.IMappedType_RegisteredSubtypes_field) &&
			            (IMappedType_RuntimeType_field == other.IMappedType_RuntimeType_field))) &&
			          (IMappedType_Schema_field == other.IMappedType_Schema_field))) &&
			        (IMappedType_Strategy_field == other.IMappedType_Strategy_field));
		}

		// Properties
		public string Catalog
		{
			get { return IMappedType_Catalog_field; }
			set
			{
				if (IMappedType_Catalog_field == value)
				{
					return;
				}
				IMappedType_Catalog_field = value;
				DirtyFlags[0] = true;
				HandlePropertyChanged("Catalog");
			}
		}

		public DateTime DateCreated
		{
			get { return IMappedType_DateCreated_field; }
		}

		public DateTime DateUpdated
		{
			get { return IMappedType_DateUpdated_field; }
		}

		public int ID
		{
			get { return IMappedType_ID_field; }
		}

		public string LatestVersion
		{
			get { return IMappedType_LatestVersion_field; }
			set
			{
				if (IMappedType_LatestVersion_field != value)
				{
					IMappedType_LatestVersion_field = value;
					DirtyFlags[4] = true;
					HandlePropertyChanged("LatestVersion");
				}
			}
		}

		public IMappedType MappedBaseType
		{
			get { return IMappedType_MappedBaseType_field.Model; }
			set
			{
				if (!IMappedType_MappedBaseType_field.Equals(value))
				{
					IMappedType_MappedBaseType_field = new DataModelReference<IMappedType, int>(value);
					DirtyFlags[5] = true;
					HandlePropertyChanged("MappedBaseType");
				}
			}
		}

		public string MappedTable
		{
			get { return IMappedType_MappedTable_field; }
			set
			{
				if (!(IMappedType_MappedTable_field == value))
				{
					IMappedType_MappedTable_field = value;
					DirtyFlags[6] = true;
					HandlePropertyChanged("MappedTable");
				}
			}
		}

		public string OriginalVersion
		{
			get { return IMappedType_OriginalVersion_field; }
			set
			{
				if (!(IMappedType_OriginalVersion_field == value))
				{
					IMappedType_OriginalVersion_field = value;
					DirtyFlags[7] = true;
					HandlePropertyChanged("OriginalVersion");
				}
			}
		}

		public string ReadObjectName
		{
			get { return IMappedType_ReadObjectName_field; }
			set
			{
				if (!(IMappedType_ReadObjectName_field == value))
				{
					IMappedType_ReadObjectName_field = value;
					DirtyFlags[8] = true;
					HandlePropertyChanged("ReadObjectName");
				}
			}
		}

		public IList<IMappedType> RegisteredSubtypes
		{
			get { return IMappedType_RegisteredSubtypes_field; }
			private set
			{
				if (value != null)
				{
					IMappedType_RegisteredSubtypes_field = new ObservableCollection<IMappedType>(value);
				}
				else
				{
					IMappedType_RegisteredSubtypes_field = new ObservableCollection<IMappedType>();
				}
				IMappedType_RegisteredSubtypes_field.CollectionChanged += IMappedType_RegisteredSubtypes_field_CollectionChanged;
			}
		}

		public Type RuntimeType
		{
			get { return IMappedType_RuntimeType_field; }
			set
			{
				if (!(IMappedType_RuntimeType_field == value))
				{
					IMappedType_RuntimeType_field = value;
					DirtyFlags[9] = true;
					HandlePropertyChanged("RuntimeType");
				}
			}
		}

		public string Schema
		{
			get { return IMappedType_Schema_field; }
			set
			{
				if (!(IMappedType_Schema_field == value))
				{
					IMappedType_Schema_field = value;
					DirtyFlags[10] = true;
					HandlePropertyChanged("Schema");
				}
			}
		}

		public MappingStrategy Strategy
		{
			get { return IMappedType_Strategy_field; }
			set
			{
				if (IMappedType_Strategy_field != value)
				{
					IMappedType_Strategy_field = value;
					DirtyFlags[11] = true;
					HandlePropertyChanged("Strategy");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				PropertyChangedEventHandler handler2;
				PropertyChangedEventHandler handler = _propertyChanged;
				do
				{
					handler2 = handler;
					var handler3 = (PropertyChangedEventHandler) Delegate.Combine(handler2, value);
					handler = Interlocked.CompareExchange(ref _propertyChanged, handler3, handler2);
				} while (handler == handler2);
			}
			remove
			{
				PropertyChangedEventHandler handler2;
				PropertyChangedEventHandler handler = _propertyChanged;
				do
				{
					handler2 = handler;
					var handler3 = (PropertyChangedEventHandler) Delegate.Remove(handler2, value);
					handler = Interlocked.CompareExchange(ref _propertyChanged, handler3, handler2);
				} while (handler == handler2);
			}
		}

		private void IMappedType_RegisteredSubtypes_field_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			DirtyFlags[9] = true;
			HandlePropertyChanged("RegisteredSubtypes");
		}

		public override bool Equals(object obj)
		{
			return ((obj is IMappedTypeDataModel) && Equals((IMappedTypeDataModel) obj));
		}

		public override int GetHashCode()
		{
			int num = 0xf3e9b;
			int num2 = CHashCodeSeed*num;
			num2 ^= num*DirtyFlags.GetHashCode();
			if (IMappedType_Catalog_field != null)
			{
				num2 ^= num*IMappedType_Catalog_field.GetHashCode();
			}
			num2 ^= num*IMappedType_DateCreated_field.GetHashCode();
			num2 ^= num*IMappedType_DateUpdated_field.GetHashCode();
			num2 ^= num*IMappedType_ID_field;
			if (IMappedType_LatestVersion_field != null)
			{
				num2 ^= num*IMappedType_LatestVersion_field.GetHashCode();
			}
			num2 ^= num*IMappedType_MappedBaseType_field.GetHashCode();
			if (IMappedType_MappedTable_field != null)
			{
				num2 ^= num*IMappedType_MappedTable_field.GetHashCode();
			}
			if (IMappedType_OriginalVersion_field != null)
			{
				num2 ^= num*IMappedType_OriginalVersion_field.GetHashCode();
			}
			if (IMappedType_ReadObjectName_field != null)
			{
				num2 ^= num*IMappedType_ReadObjectName_field.GetHashCode();
			}
			num2 ^= num*IMappedType_RegisteredSubtypes_field.GetHashCode();
			num2 ^= num*IMappedType_RuntimeType_field.GetHashCode();
			if (IMappedType_Schema_field != null)
			{
				num2 ^= num*IMappedType_Schema_field.GetHashCode();
			}
			return (num2 ^ (num*(int) IMappedType_Strategy_field));
		}

		private void HandlePropertyChanged(string propName)
		{
			if (_propertyChanged != null)
			{
				_propertyChanged(this, new PropertyChangedEventArgs(propName));
			}
		}
	}
}