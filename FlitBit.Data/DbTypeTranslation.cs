#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data;
using System.Text;

namespace FlitBit.Data
{
  public class DbTypeTranslation
  {
    public DbTypeTranslation(DbType dbType, int specializedDbType, Type runtimeType, bool isDefaultForRuntimeType,
      string providerSqlTypeName)
      : this(
        dbType, specializedDbType, runtimeType, isDefaultForRuntimeType, providerSqlTypeName,
        DbTypeLengthRequirements.None,
        null, null)
    {}

    public DbTypeTranslation(DbType dbType, int specializedDbType, Type runtimeType, bool isDefaultForRuntimeType,
      string providerSqlTypeName, DbTypeLengthRequirements lengthRequirements)
      : this(
        dbType, specializedDbType, runtimeType, isDefaultForRuntimeType, providerSqlTypeName, lengthRequirements, null,
        null
        )
    {}

    public DbTypeTranslation(DbType dbType, int specializedDbType, Type runtimeType, bool isDefaultForRuntimeType,
      string providerSqlTypeName, DbTypeLengthRequirements lengthRequirements, string defaultLength)
      : this(
        dbType, specializedDbType, runtimeType, isDefaultForRuntimeType, providerSqlTypeName, lengthRequirements,
        defaultLength, null)
    {}

    public DbTypeTranslation(DbType dbType,
      int specializedDbType,
      Type runtimeType,
      bool isDefaultForRuntimeType,
      string providerSqlTypeName,
      DbTypeLengthRequirements lengthRequirements,
      string defaultLength,
      string defaultScale)
    {
      this.DbType = dbType;
      this.SpecializedDbType = specializedDbType;
      this.RuntimeType = runtimeType;
      this.IsDefaultForRuntimeType = isDefaultForRuntimeType;
      this.ProviderSqlTypeName = providerSqlTypeName;
      this.LengthRequirements = lengthRequirements;
      this.DefaultLength = defaultLength;
      this.DefaultScale = defaultScale;
    }

    /// <summary>
    ///   Gets the DbType. This is the type whose translation is described.
    /// </summary>
    public DbType DbType { get; internal set; }

    /// <summary>
    ///   Length to be used when no length is given for a column that requires
    ///   a length.
    /// </summary>
    public string DefaultLength { get; internal set; }

    /// <summary>
    ///   Scale to be used when no scale is given for a column that requires
    ///   a scale.
    /// </summary>
    public string DefaultScale { get; internal set; }

    /// <summary>
    ///   Indicates whether this translation is used by default for the
    ///   indicated runtime type.
    /// </summary>
    public bool IsDefaultForRuntimeType { get; internal set; }

    /// <summary>
    ///   Indicates any length requirements.
    /// </summary>
    public DbTypeLengthRequirements LengthRequirements { get; internal set; }

    /// <summary>
    ///   Gets the name of the type as known by the provider (SQL type name).
    /// </summary>
    public string ProviderSqlTypeName { get; internal set; }

    /// <summary>
    ///   Gets the runtime type corresponding to the DbType.
    /// </summary>
    public Type RuntimeType { get; internal set; }

    /// <summary>
    ///   Gets the specialized type. This is the int equivalent of a provider's
    ///   db type (such as SqlDbType).
    /// </summary>
    public int SpecializedDbType { get; internal set; }

    public virtual bool MustWriteLength(int declaredLength, int declaredScale)
    {
      return (this.LengthRequirements & (DbTypeLengthRequirements.LengthSpecifierMask)) != DbTypeLengthRequirements.None;
    }

    public virtual void WriteLength(int declaredLength, int declaredScale, StringBuilder sql)
    {
      var lbracket = '(';
      var rbracket = ')';
      sql.Append(lbracket);
      if (this.LengthRequirements.HasFlag(DbTypeLengthRequirements.Length)
          || this.LengthRequirements.HasFlag(DbTypeLengthRequirements.Precision))
      {
        if (declaredLength == 0)
        {
          sql.Append(this.DefaultLength);
        }
        else
        {
          sql.Append(declaredLength);
        }
      }
      if (this.LengthRequirements.HasFlag(DbTypeLengthRequirements.Scale))
      {
        if (declaredScale == 0)
        {
          if (!this.LengthRequirements.HasFlag(DbTypeLengthRequirements.OptionalScale))
          {
            sql.Append(',')
               .Append(this.DefaultLength);
          }
        }
        else
        {
          sql.Append(',')
             .Append(declaredLength);
        }
      }
      sql.Append(rbracket);
    }
  }
}