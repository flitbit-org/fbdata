using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Meta;
using FlitBit.Emit;

namespace FlitBit.Data
{
	/// <summary>
	///   Used by the framework to emit optimized IL for database IO on behalf of data models.
	/// </summary>
	public abstract class MappedDbTypeEmitter
	{
		protected MappedDbTypeEmitter(DbType dbType, Type type)
		{
			this.DbType = dbType;
			this.RuntimeType = type;
			this.SpecializedSqlTypeName = dbType.ToString()
																					.ToUpperInvariant();
			this.NameDelimiterBegin = '[';
			this.NameDelimiterEnd = ']';
			this.NamePartSeperator = '.';
			this.LengthDelimiterBegin = '(';
			this.LengthDelimiterEnd = ')';
			this.PrecisionScaleSeparator = ',';
			this.TreatMissingLengthAsMaximum = false;
			this.LengthMaximum = String.Empty;
		}

		/// <summary>
		///   Gets the mapping's common DbType.
		/// </summary>
		public DbType DbType { get; protected set; }

		/// <summary>
		///   Indicates whether a length is required.
		/// </summary>
		public bool IsLengthRequired { get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.Length); } }

		/// <summary>
		///   Indicates whether a precision is required.
		/// </summary>
		public bool IsPrecisionRequired { get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.Precision); } }

		/// <summary>
		///   Indicates whether scale is optional.
		/// </summary>
		public bool IsScaleOptional { get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.OptionalScale); } }

		/// <summary>
		///   Indicates whether a scale is required.
		/// </summary>
		public bool IsScaleRequired { get { return !IsScaleOptional && LengthRequirements.HasFlag(DbTypeLengthRequirements.Scale); } }

		/// <summary>
		///   Gets the character used to begin distinguishing the DbType's length.
		/// </summary>
		public char LengthDelimiterBegin { get; protected set; }

		/// <summary>
		///   Gets the character used to end distinguishing the DbType's length.
		/// </summary>
		public char LengthDelimiterEnd { get; protected set; }

		public string LengthMaximum { get; protected set; }

		/// <summary>
		///   Gets length requirements for the mapped DbType.
		/// </summary>
		public DbTypeLengthRequirements LengthRequirements { get; protected set; }

		public char NameDelimiterBegin { get; protected set; }
		public char NameDelimiterEnd { get; protected set; }
		public char NamePartSeperator { get; protected set; }

		/// <summary>
		///   Gets the character used to separate a DbType's precision from it's scale.
		/// </summary>
		public char PrecisionScaleSeparator { get; protected set; }

		/// <summary>
		///   Gets the runtime type associated with the mapped DbType.
		/// </summary>
		public Type RuntimeType { get; private set; }

		/// <summary>
		///   Gets the specialized SQL type name, appropriate for constructing DDL &amp; DML.
		/// </summary>
		public string SpecializedSqlTypeName { get; protected set; }

		public bool TreatMissingLengthAsMaximum { get; protected set; }

		/// <summary>
		///   Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details">mapping detail for the column.</param>
		public abstract void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex,
			DbTypeDetails details);

		public void DescribeColumn(StringBuilder buffer, ColumnMapping mapping)
		{
			var details = mapping.DbTypeDetails;
			buffer.Append(' ')
						.Append(NameDelimiterBegin)
						.Append(mapping.TargetName)
						.Append(NameDelimiterEnd)
						.Append(' ')
						.Append(SpecializedSqlTypeName);
			if (IsLengthRequired || IsPrecisionRequired)
			{
				if (details.Length.HasValue)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(details.Length);
					if (IsScaleRequired && details.Scale.HasValue)
					{
						buffer.Append(PrecisionScaleSeparator)
									.Append(details.Scale);
					}
					buffer.Append(LengthDelimiterEnd);
				}
				else if (this.TreatMissingLengthAsMaximum)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(LengthMaximum)
								.Append(LengthDelimiterEnd);
				}
			}
			if (mapping.IsTimestampOnInsert)
			{
				buffer.Append(" ON INSERT");
			}
			else if (mapping.IsTimestampOnUpdate)
			{
				buffer.Append(" ON UPDATE");
			}
			else
			{
				if (!mapping.IsNullable)
				{
					buffer.Append(" NOT");
				}
				buffer.Append(" NULL");
			}
			if (mapping.IsIdentity)
			{
				buffer.Append(" PRIMARY KEY");
			}
			if (mapping.IsAlternateKey)
			{
				buffer.Append(" ALTER KEY");
			}
			if (mapping.IsReference && mapping.ReferenceTargetMember != null)
			{
				var foreign = Mappings.AccessMappingFor(mapping.ReferenceTargetMember.DeclaringType);
				var foreignCol = foreign.Columns.First(c => c.Member == mapping.ReferenceTargetMember);
				buffer.Append(" REFERENCES ")
							.Append(mapping.ReferenceTargetMember.DeclaringType)
							.Append('.')
							.Append(mapping.ReferenceTargetMember.Name);
			}
		}

		public virtual DbTypeDetails GetDbTypeDetails(ColumnMapping column)
		{
			Debug.Assert(column.Member.DeclaringType != null, "column.Member.DeclaringType != null");
			var bindingName = String.Concat(column.Member.DeclaringType.Name, "_", column.Member.Name);
			var len = (column.VariableLength != 0) ? column.VariableLength : (int?) null;
			return new DbTypeDetails(column.Member.Name, bindingName, len, null);
		}

		public virtual object EmitColumnDDL<TModel>(StringBuilder buffer, int ordinal, Mapping<TModel> mapping,
			ColumnMapping<TModel> col)
		{
			var tableConstraints = new List<string>();
			var details = col.DbTypeDetails;
			if (ordinal > 0)
			{
				buffer.Append(',');
			}
			buffer.Append(Environment.NewLine)
						.Append("\t")
						.Append(NameDelimiterBegin)
						.Append(col.TargetName)
						.Append(NameDelimiterEnd)
						.Append(' ')
						.Append(SpecializedSqlTypeName);
			if (IsLengthRequired || IsPrecisionRequired)
			{
				if (details.Length.HasValue)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(details.Length);
					if (IsScaleRequired && details.Scale.HasValue)
					{
						buffer.Append(PrecisionScaleSeparator)
									.Append(details.Scale);
					}
					buffer.Append(LengthDelimiterEnd);
				}
				else if (this.TreatMissingLengthAsMaximum)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(LengthMaximum)
								.Append(LengthDelimiterEnd);
				}
			}
			if (col.IsSynthetic)
			{
				EmitColumnInitializationDDL(buffer, mapping, col);
			}

			if (!col.IsNullable)
			{
				buffer.Append(" NOT");
			}
			buffer.Append(" NULL");
			EmitColumnConstraintsDDL(buffer, mapping, col, tableConstraints);
			return (tableConstraints.Count > 0) ? tableConstraints : null;
		}

		public virtual void EmitColumnConstraintsDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping,
			ColumnMapping<TModel> col, List<string> tableConstraints)
		{
			if (col.IsIdentity && mapping.Identity.Columns.Count() == 1)
			{
				buffer.Append(Environment.NewLine)
							.Append("\t\tCONSTRAINT PK_")
							.Append(mapping.TargetSchema)
							.Append(mapping.TargetObject)
							.Append(" PRIMARY KEY");
			}
			if (col.IsAlternateKey)
			{
				buffer.Append(Environment.NewLine)
							.Append("\t\tCONSTRAINT AK_")
							.Append(mapping.TargetSchema)
							.Append(mapping.TargetObject)
							.Append('_')
							.Append(col.TargetName)
							.Append(" UNIQUE");
			}
			if (col.RuntimeType == typeof(DateTime))
			{
				if (col.IsTimestampOnInsert || col.IsTimestampOnUpdate)
				{
					buffer.Append(Environment.NewLine)
								.Append("\t\tCONSTRAINT DF_")
								.Append(mapping.TargetSchema)
								.Append(mapping.TargetObject)
								.Append('_')
								.Append(col.TargetName)
								.Append(" DEFAULT (GETUTCDATE())");
				}
				if (col.IsTimestampOnUpdate)
				{
					var timestampOnInsertCol = mapping.Columns.FirstOrDefault(c => c.IsTimestampOnInsert);
					if (timestampOnInsertCol != null)
					{
						buffer.Append(',')
									.Append(Environment.NewLine)
									.Append("\t\tCONSTRAINT CK_")
									.Append(mapping.TargetSchema)
									.Append(mapping.TargetObject)
									.Append('_')
									.Append(col.TargetName)
									.Append(" CHECK (")
									.Append(NameDelimiterBegin)
									.Append(col.TargetName)
									.Append(NameDelimiterEnd)
									.Append(" >= ")
									.Append(NameDelimiterBegin)
									.Append(timestampOnInsertCol.TargetName)
									.Append(NameDelimiterEnd)
									.Append(")");
					}
				}
			}
			if (col.IsReference && col.ReferenceTargetMember != null)
			{
				var foreign = Mappings.AccessMappingFor(col.ReferenceTargetMember.DeclaringType);
				var foreignCol = foreign.Columns.First(c => c.Member == col.ReferenceTargetMember);
				buffer.Append(Environment.NewLine)
							.Append("\t\tCONSTRAINT FK_")
							.Append(mapping.TargetSchema)
							.Append(mapping.TargetObject)
							.Append('_')
							.Append(col.TargetName)
							.Append(Environment.NewLine)
							.Append("\t\t\tFOREIGN KEY REFERENCES ")
							.Append(foreign.DbObjectReference)
							.Append('(')
							.Append(NameDelimiterBegin)
							.Append(foreignCol.TargetName)
							.Append(NameDelimiterEnd)
							.Append(')');
				if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnUpdateCascade))
				{
					buffer.Append(Environment.NewLine)
								.Append("\t\t\tON UPDATE CASCADE");
				}
				if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnDeleteCascade))
				{
					buffer.Append(Environment.NewLine)
								.Append("\t\t\tON DELETE CASCADE");
				}
				else if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnDeleteSetNull))
				{
					buffer.Append(Environment.NewLine)
								.Append("\t\t\tON DELETE SET NULL");
				}
				else if (col.ReferenceBehaviors.HasFlag(ReferenceBehaviors.OnDeleteSetDefault))
				{
					buffer.Append(Environment.NewLine)
								.Append("\t\t\tON DELETE SET DEFAULT");
				}
			}
		}

		public virtual void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping,
			ColumnMapping<TModel> col)
		{}


		public virtual void EmitTableConstraintDDL<TModel>(StringBuilder buffer, Mapping<TModel> mapping,
			ColumnMapping<TModel> col, object handback)
		{}

		public virtual void EmitColumnDDLForHierarchy<TModel>(StringBuilder buffer, int ordinal, Mapping<TModel> mapping,
			IMapping baseMapping, ColumnMapping col)
		{
			var details = col.DbTypeDetails;
			if (ordinal > 0)
			{
				buffer.Append(',');
			}
			buffer.Append(Environment.NewLine)
						.Append("\t")
						.Append(NameDelimiterBegin)
						.Append(col.TargetName)
						.Append(NameDelimiterEnd)
						.Append(' ')
						.Append(SpecializedSqlTypeName);
			if (IsLengthRequired || IsPrecisionRequired)
			{
				if (details.Length.HasValue)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(details.Length);
					if (IsScaleRequired && details.Scale.HasValue)
					{
						buffer.Append(PrecisionScaleSeparator)
									.Append(details.Scale);
					}
					buffer.Append(LengthDelimiterEnd);
				}
				else if (this.TreatMissingLengthAsMaximum)
				{
					buffer.Append(LengthDelimiterBegin)
								.Append(LengthMaximum)
								.Append(LengthDelimiterEnd);
				}
			}

			if (!col.IsNullable)
			{
				buffer.Append(" NOT");
			}
			buffer.Append(" NULL")
						.Append(Environment.NewLine)
						.Append("\t\tCONSTRAINT PK_")
						.Append(mapping.TargetSchema)
						.Append(mapping.TargetObject)
						.Append(" PRIMARY KEY")
						.Append(Environment.NewLine)
						.Append("\t\tCONSTRAINT FK_")
						.Append(mapping.TargetSchema)
						.Append(mapping.TargetObject)
						.Append('_')
						.Append(col.TargetName)
						.Append(Environment.NewLine)
						.Append("\t\t\tFOREIGN KEY REFERENCES ")
						.Append(baseMapping.DbObjectReference)
						.Append('(')
						.Append(NameDelimiterBegin)
						.Append(col.TargetName)
						.Append(NameDelimiterEnd)
						.Append(')')
						.Append(Environment.NewLine)
						.Append("\t\t\tON UPDATE CASCADE")
						.Append(Environment.NewLine)
						.Append("\t\t\tON DELETE CASCADE");
		}

	}

	internal abstract class MappedDbTypeEmitter<T> : MappedDbTypeEmitter
	{
		protected MappedDbTypeEmitter(DbType dbType)
			: base(dbType, typeof(T))
		{ }
	}

	internal abstract class MappedDbTypeEmitter<T, TDbType> : MappedDbTypeEmitter<T>
		where TDbType : struct
	{
		protected MappedDbTypeEmitter(DbType dbType, TDbType specializedDbType)
			: base(dbType)
		{
			this.SpecializedDbType = specializedDbType;
			this.SpecializedSqlTypeName = specializedDbType.ToString()
																										.ToUpperInvariant();
		}

		/// <summary>
		///   Gets the mapping's specialized DbType.
		/// </summary>
		public TDbType SpecializedDbType { get; private set; }
	}
}