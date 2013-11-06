using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Data.Expressions;
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
			DbType = dbType;
			RuntimeType = type;
			SpecializedSqlTypeName = dbType.ToString()
				.ToUpperInvariant();
			NameDelimiterBegin = '[';
			NameDelimiterEnd = ']';
			NamePartSeperator = '.';
			LengthDelimiterBegin = '(';
			LengthDelimiterEnd = ')';
			PrecisionScaleSeparator = ',';
			TreatMissingLengthAsMaximum = false;
			LengthMaximum = String.Empty;
		}

		/// <summary>
		///   Gets the mapping's common DbType.
		/// </summary>
		public DbType DbType { get; protected set; }

		/// <summary>
		///   Indicates whether a length is required.
		/// </summary>
		public bool IsLengthRequired
		{
			get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.Length); }
		}

		/// <summary>
		///   Indicates whether a precision is required.
		/// </summary>
		public bool IsPrecisionRequired
		{
			get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.Precision); }
		}

		/// <summary>
		///   Indicates whether scale is optional.
		/// </summary>
		public bool IsScaleOptional
		{
			get { return LengthRequirements.HasFlag(DbTypeLengthRequirements.OptionalScale); }
		}

		/// <summary>
		///   Indicates whether a scale is required.
		/// </summary>
		public bool IsScaleRequired
		{
			get { return !IsScaleOptional && LengthRequirements.HasFlag(DbTypeLengthRequirements.Scale); }
		}

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
		///   Gets the specialized DbType's value (as integer).
		/// </summary>
		public int SpecializedDbTypeValue { get; protected set; }

		/// <summary>
		///   Gets the specialized SQL type name, appropriate for constructing DDL &amp; DML.
		/// </summary>
		public string SpecializedSqlTypeName { get; protected set; }

		/// <summary>
		///   Indicates that when a length is not specified the max length is to be used.
		/// </summary>
		public bool TreatMissingLengthAsMaximum { get; protected set; }

		public bool IsQuoteRequired { get; protected set; }

		public bool CanConstantContainQuote { get; protected set; }

		public string QuoteChars { get; protected set; }

		public string DelimitedQuoteChars { get; protected set; }

		/// <summary>
		///   Emits MSIL that loads a value from a DbDataReader, translates it to the RuntimeType, and leaves the value on the
		///   stack.
		/// </summary>
		/// <param name="method">the method under construction.</param>
		/// <param name="reader">a reference to the reader.</param>
		/// <param name="columnIndex">a reference to the column's index within the reader.</param>
		/// <param name="details">mapping detail for the column.</param>
		public abstract void LoadValueFromDbReader(MethodBuilder method, IValueRef reader, IValueRef columnIndex,
			DbTypeDetails details);

		public void DescribeColumn(StringBuilder buffer, ColumnMapping mapping)
		{
			DbTypeDetails details = mapping.DbTypeDetails;
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
				else if (TreatMissingLengthAsMaximum)
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
				buffer.Append(" ALTERNATE KEY");
			}
			if (mapping.IsReference && mapping.ReferenceTargetMember != null)
			{
				IMapping foreign = Mappings.AccessMappingFor(mapping.ReferenceTargetMember.DeclaringType);
				ColumnMapping foreignCol = foreign.Columns.First(c => c.Member == mapping.ReferenceTargetMember);
				buffer.Append(" REFERENCES ")
					.Append(mapping.ReferenceTargetMember.DeclaringType)
					.Append('.')
					.Append(mapping.ReferenceTargetMember.Name);
			}
		}

		/// <summary>
		///   Declares a variable matching the columns definition, suitable for use in a SQL script.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="mapping"></param>
		/// <param name="helper"></param>
		/// <param name="overrideBindName">the variable's name or null to use the column's bind name.</param>
		public void DeclareScriptVariable(SqlWriter writer, ColumnMapping mapping, DbProviderHelper helper,
			string overrideBindName)
		{
			DbTypeDetails details = mapping.DbTypeDetails;
			string bindName = helper.FormatParameterName(overrideBindName ?? details.BindingName);
			writer.Append("DECLARE ")
				.Append(" ").Append(bindName ?? details.BindingName).Append(" ")
				.Append(SpecializedSqlTypeName);
			if (IsLengthRequired || IsPrecisionRequired)
			{
				if (details.Length.HasValue)
				{
					writer.Append(LengthDelimiterBegin)
						.Append(Convert.ToString(details.Length));
					if (IsScaleRequired && details.Scale.HasValue)
					{
						writer.Append(PrecisionScaleSeparator)
							.Append(Convert.ToString(details.Scale));
					}
					writer.Append(LengthDelimiterEnd);
				}
				else if (TreatMissingLengthAsMaximum)
				{
					writer.Append(LengthDelimiterBegin)
						.Append(LengthMaximum)
						.Append(LengthDelimiterEnd);
				}
			}
		}

		public virtual DbTypeDetails GetDbTypeDetails(ColumnMapping column)
		{
			Debug.Assert(column.Member.DeclaringType != null, "column.Member.DeclaringType != null");
			string bindingName = String.Concat(column.Member.DeclaringType.Name, "_", column.Member.Name);
			int? len = (column.VariableLength != 0) ? column.VariableLength : (int?) null;
			return new DbTypeDetails(column.Member.Name, bindingName, len, null);
		}

		public virtual object EmitColumnDDL<TModel>(StringBuilder buffer, int ordinal, IMapping<TModel> mapping,
			ColumnMapping<TModel> col)
		{
			var tableConstraints = new List<string>();
			DbTypeDetails details = col.DbTypeDetails;
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
				else if (TreatMissingLengthAsMaximum)
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

		public virtual void EmitColumnConstraintsDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping,
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
			if (col.RuntimeType == typeof (DateTime))
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
					ColumnMapping timestampOnInsertCol = mapping.Columns.FirstOrDefault(c => c.IsTimestampOnInsert);
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
				IMapping foreign = Mappings.AccessMappingFor(col.ReferenceTargetMember.DeclaringType);
				ColumnMapping foreignCol = foreign.Columns.First(c => c.Member == col.ReferenceTargetMember);
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

		public virtual void EmitColumnInitializationDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping,
			ColumnMapping<TModel> col)
		{
		}


		public virtual void EmitTableConstraintDDL<TModel>(StringBuilder buffer, IMapping<TModel> mapping,
			ColumnMapping<TModel> col, object handback)
		{
		}

		public virtual void EmitColumnDDLForHierarchy<TModel>(StringBuilder buffer, int ordinal, IMapping<TModel> mapping,
			IMapping baseMapping, ColumnMapping col)
		{
			DbTypeDetails details = col.DbTypeDetails;
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
				else if (TreatMissingLengthAsMaximum)
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

		protected virtual void EmitTranslateDbType(ILGenerator il)
		{
		}

		/// <summary>
		///   Emits IL to translate the runtime type to the dbtype.
		/// </summary>
		/// <param name="il"></param>
		/// <remarks>
		///   At the time of the call the runtime value is on top of the stack.
		///   When the method returns the translated type must be on the top of the stack.
		/// </remarks>
		protected virtual void EmitTranslateRuntimeType(ILGenerator il)
		{
		}

		public virtual void BindParameterOnDbCommand<TDbParameter>(MethodBuilder method, ColumnMapping column,
			string bindingName, LocalBuilder parm, Action<ILGenerator> loadCmd, Action<ILGenerator> loadModel, Action<ILGenerator> loadProp,
			LocalBuilder flag)
			where TDbParameter : DbParameter
		{
			ILGenerator il = method.GetILGenerator();
			DbTypeDetails details = column.DbTypeDetails;

			il.NewObj(typeof(TDbParameter).GetConstructor(Type.EmptyTypes));
			il.StoreLocal(parm);
			il.LoadLocal(parm);
			il.LoadValue(bindingName);
			il.CallVirtual<TDbParameter>("set_ParameterName", typeof(string));
			
			EmitDbParameterSetDbType(il, parm);
			
			if (IsLengthRequired)
			{
				il.LoadLocal(parm);
				il.LoadValue(details.Length.Value);
				il.CallVirtual<TDbParameter>("set_Size", typeof(int));
			}
			else if (IsPrecisionRequired)
			{
				il.LoadLocal(parm);
				il.LoadValue(details.Length.Value);
				il.CallVirtual<TDbParameter>("set_Precision", typeof(byte));
				if (IsScaleRequired || (IsScaleOptional && details.Scale.HasValue))
				{
					il.LoadLocal(parm);
					il.LoadValue(details.Scale.Value);
					il.CallVirtual<TDbParameter>("set_Scale", typeof(byte));
				}
			}
			
			loadCmd(il);
			il.CallVirtual<DbCommand>("get_Parameters");
			il.LoadLocal(parm);
			il.CallVirtual<DbParameterCollection>("Add", typeof(DbParameter));
			il.Pop();

			LocalBuilder local = il.DeclareLocal(column.RuntimeType);

			loadModel(il);
			loadProp(il);
			il.StoreLocal(local);
			EmitDbParameterSetValue(il, column, parm, local, flag);
		}

		protected internal virtual void EmitDbParameterSetValue(ILGenerator il, ColumnMapping column, LocalBuilder parm,
			LocalBuilder local, LocalBuilder flag)
		{
			if (column.IsReference && column.RuntimeType.IsValueType)
			{
				Label fin = il.DefineLabel();
				Label ifelse = il.DefineLabel();
				il.DeclareLocal(column.RuntimeType);
				Type comparerType = typeof (EqualityComparer<>).MakeGenericType(column.RuntimeType);
				il.Call(comparerType.GetProperty("Default").GetGetMethod());
				il.LoadDefaultValue(typeof (int));
				il.LoadLocal(local);
				il.CallVirtual(comparerType.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null,
					new[] {column.RuntimeType, column.RuntimeType}, null)
					);
				il.LoadValue(0);
				il.CompareEqual();
				il.StoreLocal(flag);
				il.LoadLocal(flag);
				il.BranchIfTrue(ifelse);
				il.LoadLocal(parm);
				il.LoadField(typeof (DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public));
				il.CallVirtual<DbParameter>("set_Value");
				il.Branch(fin);
				il.MarkLabel(ifelse);
				il.LoadLocal(parm);
				il.LoadLocal(local);
				EmitTranslateRuntimeType(il);
				il.CallVirtual<DbParameter>("set_Value");
				il.MarkLabel(fin);
			}
			else
			{
				il.LoadLocal(parm);
				il.LoadLocal(local);
				EmitTranslateRuntimeType(il);
				il.CallVirtual<DbParameter>("set_Value");
			}
		}

		protected internal virtual void EmitDbParameterSetDbType(ILGenerator il, LocalBuilder parm)
		{
			il.LoadLocal(parm);
			il.LoadValue(DbType);
			il.CallVirtual<DbParameter>("set_DbType", typeof(DbType));
		}

		public virtual string PrepareConstantValueForSql(object value)
		{
			string res = TransformConstantValueToString(value);
			if (IsQuoteRequired)
			{
				if (CanConstantContainQuote)
				{
					res = res.Replace(QuoteChars, DelimitedQuoteChars);
				}
				res = String.Concat(QuoteChars, res, QuoteChars);
			}
			return res;
		}

		protected virtual string TransformConstantValueToString(object value)
		{
			return Convert.ToString(value);
		}
	}

	internal abstract class MappedDbTypeEmitter<T> : MappedDbTypeEmitter
	{
		protected MappedDbTypeEmitter(DbType dbType)
			: base(dbType, typeof (T))
		{
		}
	}

	internal abstract class MappedDbTypeEmitter<T, TDbType> : MappedDbTypeEmitter<T>
		where TDbType : struct
	{
		protected MappedDbTypeEmitter(DbType dbType, TDbType specializedDbType)
			: base(dbType)
		{
			SpecializedDbTypeValue = Convert.ToInt32(specializedDbType);
			SpecializedDbType = specializedDbType;
			SpecializedSqlTypeName = specializedDbType.ToString()
				.ToUpperInvariant();
		}

		/// <summary>
		///   Gets the mapping's specialized DbType.
		/// </summary>
		public TDbType SpecializedDbType { get; private set; }
	}
}