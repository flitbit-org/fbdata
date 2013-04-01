using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using FlitBit.Core;
using FlitBit.Core.Collections;
using FlitBit.Data.DataModel;
using FlitBit.Data.SPI;

namespace FlitBit.Data.Tests.Meta.Models
{
	[Serializable]
public sealed class IPersonDataModel : INotifyPropertyChanged, IParty, IPerson, IEquatable<IPersonDataModel>, IEquatable<IPerson>, IDataModel
{
		// Fields
		private static readonly string[] __fieldMap = new string[] { "ID", "Kind", "Name", "DateCreated", "DateUpdated", "FirstName", "LastName", "MiddleNames", "ScreenName", "EmailAddress", "VerificationState" };
		/* private scope */ BitVector DirtyFlags = new BitVector(11);
		private DateTime IParty_DateCreated_field;
		private DateTime IParty_DateUpdated_field;
		private int IParty_ID_field;
		private DataModelReference<IPartyKind, string> IParty_Kind_field = new DataModelReference<IPartyKind, string>((IPartyKind)null);
		private string IParty_Name_field;
		private string IPerson_EmailAddress_field;
		private string IPerson_FirstName_field;
		private string IPerson_LastName_field;
		private string IPerson_MiddleNames_field;
		private string IPerson_ScreenName_field;
		private EmailVerificationStates IPerson_VerificationState_field;
		[NonSerialized]
		private PropertyChangedEventHandler _propertyChanged;
		private static readonly int CHashCodeSeed = typeof(IPersonDataModel).AssemblyQualifiedName.GetHashCode();

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

		// Methods
		public object Clone()
		{
				IPersonDataModel model = (IPersonDataModel) base.MemberwiseClone();
				model.DirtyFlags = this.DirtyFlags.Copy();
				model._propertyChanged = null;
				return model;
		}

		public bool Equals(IPerson other)
		{
				return ((other is IPersonDataModel) && this.Equals((IPersonDataModel) other));
		}

		public bool Equals(IPersonDataModel other)
		{
				return ((((((this.DirtyFlags == other.DirtyFlags) && (this.IParty_ID_field == other.IParty_ID_field)) && (EqualityComparer<DataModelReference<IPartyKind, string>>.Default.Equals(this.IParty_Kind_field, other.IParty_Kind_field) && (this.IParty_Name_field == other.IParty_Name_field))) && (((this.IParty_DateCreated_field == other.IParty_DateCreated_field) && (this.IParty_DateUpdated_field == other.IParty_DateUpdated_field)) && ((this.IPerson_FirstName_field == other.IPerson_FirstName_field) && (this.IPerson_LastName_field == other.IPerson_LastName_field)))) && (((this.IPerson_MiddleNames_field == other.IPerson_MiddleNames_field) && (this.IPerson_ScreenName_field == other.IPerson_ScreenName_field)) && (this.IPerson_EmailAddress_field == other.IPerson_EmailAddress_field))) && (this.IPerson_VerificationState_field == other.IPerson_VerificationState_field));
		}

		public override bool Equals(object obj)
		{
				return ((obj is IPersonDataModel) && this.Equals((IPersonDataModel) obj));
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
				num2 ^= num * this.IParty_ID_field;
				num2 ^= num * this.IParty_Kind_field.GetHashCode();
				if (this.IParty_Name_field != null)
				{
						num2 ^= num * this.IParty_Name_field.GetHashCode();
				}
				num2 ^= num * this.IParty_DateCreated_field.GetHashCode();
				num2 ^= num * this.IParty_DateUpdated_field.GetHashCode();
				if (this.IPerson_FirstName_field != null)
				{
						num2 ^= num * this.IPerson_FirstName_field.GetHashCode();
				}
				if (this.IPerson_LastName_field != null)
				{
						num2 ^= num * this.IPerson_LastName_field.GetHashCode();
				}
				if (this.IPerson_MiddleNames_field != null)
				{
						num2 ^= num * this.IPerson_MiddleNames_field.GetHashCode();
				}
				if (this.IPerson_ScreenName_field != null)
				{
						num2 ^= num * this.IPerson_ScreenName_field.GetHashCode();
				}
				if (this.IPerson_EmailAddress_field != null)
				{
						num2 ^= num * this.IPerson_EmailAddress_field.GetHashCode();
				}
				return (num2 ^ (num * (int)this.IPerson_VerificationState_field));
		}

		public TIdentityKey GetReferentID<TIdentityKey>(string member)
		{
				if (string.Equals("Kind", member))
				{
						return (TIdentityKey) (object) this.IParty_Kind_field.IdentityKey;
				}
				if (member == null)
				{
						throw new ArgumentNullException("member");
				}
				throw new ArgumentOutOfRangeException("member", "IPerson does not reference: " + member + ".");
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
						throw new ArgumentOutOfRangeException("member", "IPerson does not define property: `" + member + "`.");
				}
				return this.DirtyFlags[index];
		}
				
		public void ResetDirtyFlags()
		{
				this.DirtyFlags = new BitVector(11);
		}

		public void SetReferentID<TIdentityKey>(string member, TIdentityKey id)
		{
				if (string.Equals("Kind", member))
				{
						if (!this.IParty_Kind_field.IdentityEquals(id))
						{
								this.IParty_Kind_field = new DataModelReference<IPartyKind, string>((string) (object)id);
								this.DirtyFlags[1] = true;
								this.HandlePropertyChanged("Kind");
						}
				}
				else
				{
						if (member == null)
						{
								throw new ArgumentNullException("member");
						}
						throw new ArgumentOutOfRangeException("member", "IPerson does not reference: " + member + ".");
				}
		}

		// Properties
		public DateTime DateCreated
		{
				get
				{
						return this.IParty_DateCreated_field;
				}
		}

		public DateTime DateUpdated
		{
				get
				{
						return this.IParty_DateUpdated_field;
				}
		}

		public string EmailAddress
		{
				get
				{
						return this.IPerson_EmailAddress_field;
				}
				set
				{
						if (!(this.IPerson_EmailAddress_field == value))
						{
								this.IPerson_EmailAddress_field = value;
								this.DirtyFlags[9] = true;
								this.HandlePropertyChanged("EmailAddress");
						}
				}
		}

		public string FirstName
		{
				get
				{
						return this.IPerson_FirstName_field;
				}
				set
				{
						if (!(this.IPerson_FirstName_field == value))
						{
								this.IPerson_FirstName_field = value;
								this.DirtyFlags[5] = true;
								this.HandlePropertyChanged("FirstName");
						}
				}
		}

		public int ID
		{
				get
				{
						return this.IParty_ID_field;
				}
		}

		public IPartyKind Kind
		{
				get
				{
						return this.IParty_Kind_field.Model;
				}
		}

		public string LastName
		{
				get
				{
						return this.IPerson_LastName_field;
				}
				set
				{
						if (!(this.IPerson_LastName_field == value))
						{
								this.IPerson_LastName_field = value;
								this.DirtyFlags[6] = true;
								this.HandlePropertyChanged("LastName");
						}
				}
		}

		public string MiddleNames
		{
				get
				{
						return this.IPerson_MiddleNames_field;
				}
				set
				{
						if (!(this.IPerson_MiddleNames_field == value))
						{
								this.IPerson_MiddleNames_field = value;
								this.DirtyFlags[7] = true;
								this.HandlePropertyChanged("MiddleNames");
						}
				}
		}

		public string Name
		{
				get
				{
						return this.IParty_Name_field;
				}
				set
				{
						if (!(this.IParty_Name_field == value))
						{
								this.IParty_Name_field = value;
								this.DirtyFlags[2] = true;
								this.HandlePropertyChanged("Name");
						}
				}
		}

		public string ScreenName
		{
				get
				{
						return this.IPerson_ScreenName_field;
				}
				set
				{
						if (!(this.IPerson_ScreenName_field == value))
						{
								this.IPerson_ScreenName_field = value;
								this.DirtyFlags[8] = true;
								this.HandlePropertyChanged("ScreenName");
						}
				}
		}

		public EmailVerificationStates VerificationState
		{
				get
				{
						return this.IPerson_VerificationState_field;
				}
				set
				{
						if (this.IPerson_VerificationState_field != value)
						{
								this.IPerson_VerificationState_field = value;
								this.DirtyFlags[10] = true;
								this.HandlePropertyChanged("VerificationState");
						}
				}
		}
		
		public void LoadFromDataReader(DbDataReader reader, int[] offsets)
		{
			int ordinal = offsets[0];
			this.IParty_ID_field = reader.IsDBNull(ordinal) ? new int() : reader.GetInt32(ordinal);
			ordinal = offsets[1];
			this.IParty_Kind_field = reader.IsDBNull(ordinal) ? new DataModelReference<IPartyKind, string>((IPartyKind)null) : new DataModelReference<IPartyKind, string>(reader.GetString(ordinal));
			ordinal = offsets[2];
			this.IParty_Name_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[3];
			this.IParty_DateCreated_field = reader.IsDBNull(ordinal) ? new DateTime() : reader.GetDateTime(ordinal);
			ordinal = offsets[4];
			this.IParty_DateUpdated_field = reader.IsDBNull(ordinal) ? new DateTime() : reader.GetDateTime(ordinal);
			ordinal = offsets[5];
			this.IPerson_FirstName_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[6];
			this.IPerson_LastName_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[7];
			this.IPerson_MiddleNames_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[8];
			this.IPerson_ScreenName_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[9];
			this.IPerson_EmailAddress_field = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			ordinal = offsets[10];
			this.IPerson_VerificationState_field = reader.IsDBNull(ordinal) ? new EmailVerificationStates() : ((EmailVerificationStates)reader.GetInt32(ordinal));
			this.DirtyFlags = new BitVector(11);
		}
	}

}