using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Threading;
using FlitBit.Core.Collections;
using FlitBit.Data.Meta;
using FlitBit.Data.SPI;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.Tests.Patterns
{
  [MapEntity(EntityBehaviors.MapAllProperties, MappingStrategy.OneClassOneTable)]
  public interface ITestNullableDateTime : IDataModel
  {
    [IdentityKey, MapColumn(ColumnBehaviors.Synthetic)]
    int ID { get; }
    [MapColumn(ColumnBehaviors.Nullable)]
    DateTime? TouchedDate { get; set; }
  }

 
    [Serializable]
    public sealed class ITestNullableDateTimeDataModel : INotifyPropertyChanged, ITestNullableDateTime, IEquatable<ITestNullableDateTimeDataModel>, IEquatable<ITestNullableDateTime>, IDataModel
    {
        private static readonly string[] __fieldMap = new string[] { "ID", "Name", "TermName", "Description", "Active" };
        /* private scope */ BitVector _dirtyFlags = new BitVector(5);
        private int ITestNullableDateTime_ID_field;
        private DateTime? ITestNullableDateTime_TouchedDate_field;
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChanged;
        private static readonly int CHashCodeSeed = typeof(ITestNullableDateTimeDataModel).AssemblyQualifiedName.GetHashCode();

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

        public object Clone()
        {
            ITestNullableDateTimeDataModel model = (ITestNullableDateTimeDataModel) base.MemberwiseClone();
            model._dirtyFlags = this._dirtyFlags.Copy();
            model._propertyChanged = null;
            return model;
        }

        public bool Equals(ITestNullableDateTime other)
        {
            return ((other is ITestNullableDateTimeDataModel) && this.Equals((ITestNullableDateTimeDataModel) other));
        }

        public bool Equals(ITestNullableDateTimeDataModel other)
        {
          return this._dirtyFlags == other._dirtyFlags
                     &&
          this.ITestNullableDateTime_ID_field == other.ITestNullableDateTime_ID_field;
            ;
        }

        public override bool Equals(object obj)
        {
            return ((obj is ITestNullableDateTimeDataModel) && this.Equals((ITestNullableDateTimeDataModel) obj));
        }

        public BitVector GetDirtyFlags()
        {
            return (BitVector) this._dirtyFlags.Clone();
        }

        public override int GetHashCode()
        {
            int num = 0xf3e9b;
            int num2 = CHashCodeSeed * num;
            num2 ^= num * this._dirtyFlags.GetHashCode();
            num2 ^= num * this.ITestNullableDateTime_ID_field;
          return num2;
        }

        public TIdentityKey GetReferentID<TIdentityKey>(string member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            throw new ArgumentOutOfRangeException("member", "ITestNullableDateTime does not reference: " + member + ".");
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
                throw new ArgumentOutOfRangeException("member", "ITestNullableDateTime does not define property: `" + member + "`.");
            }
            return this._dirtyFlags[index];
        }

        public void LoadFromDataReader(DbDataReader reader, int[] offsets)
        {
            int ordinal = offsets[0];
            this.ITestNullableDateTime_ID_field = reader.GetInt16(ordinal);
            ordinal = offsets[1];
          if (reader.IsDBNull(ordinal))
          {
            ITestNullableDateTime_TouchedDate_field = (DateTime?)null;
          }
          else this.ITestNullableDateTime_TouchedDate_field = new DateTime?(reader.GetDateTime(ordinal));
            
            this._dirtyFlags = new BitVector(5);
        }

        public void ResetDirtyFlags()
        {
            this._dirtyFlags = new BitVector(5);
        }

        public void SetReferentID<TIdentityKey>(string member, TIdentityKey id)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            throw new ArgumentOutOfRangeException("member", "ITestNullableDateTime does not reference: " + member + ".");
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }
  
        public int ID
        {
            get
            {
                return this.ITestNullableDateTime_ID_field;
            }
            set
            {
                if (this.ITestNullableDateTime_ID_field != value)
                {
                    this.ITestNullableDateTime_ID_field = value;
                    this._dirtyFlags[0] = true;
                    this.HandlePropertyChanged("ID");
                }
            }
        }

        public DateTime? TouchedDate
        {
            get
            {
                return this.ITestNullableDateTime_TouchedDate_field;
            }
            set
            {
                if (this.ITestNullableDateTime_TouchedDate_field != value)
                {
                  this.ITestNullableDateTime_TouchedDate_field = value;
                    this._dirtyFlags[2] = true;
                    this.HandlePropertyChanged("TermName");
                }
            }
        }
    }

}
