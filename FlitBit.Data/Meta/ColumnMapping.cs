#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Emit;

namespace FlitBit.Data.Meta
{
	public class ColumnMapping
	{
		IMapping _mapping;

		protected ColumnMapping(IMapping mapping, MemberInfo member, int ordinal)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);

			this.Member = member;
			this.TargetName = member.Name;
			this.Ordinal = ordinal;
			this._mapping = mapping;
		}

		public string TargetName { get; set; }
		public MemberInfo Member { get; private set; }
		public int Ordinal { get; internal set; }
		public ColumnBehaviors Behaviors { get; internal set; }
		public int VariableLength { get; internal set; }
		public bool IsReference { get; internal set; }
		public bool IsAlternateKey { get { return Behaviors.HasFlag(ColumnBehaviors.AlternateKey); } }
		public bool IsCalculated { get { return Behaviors.HasFlag(ColumnBehaviors.Calculated); } }
		public bool IsSynthetic { get { return Behaviors.HasFlag(ColumnBehaviors.Synthetic); } }
		public bool IsImmutable { get { return Behaviors.HasFlag(ColumnBehaviors.Immutable); } }
		public bool IsNullable { get { return Behaviors.HasFlag(ColumnBehaviors.Nullable); } }
		public bool IsIdentity { get { return Behaviors.HasFlag(ColumnBehaviors.Identity); } }
		public bool IsTimestampOnInsert { get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnInsert); } }
		public bool IsTimestampOnUpdate { get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnUpdate); } }
		public bool IsRevisionTracking { get { return Behaviors.HasFlag(ColumnBehaviors.RevisionTracking); } }
		public MemberInfo ReferenceTargetMember { get; internal set; }
		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }

		/// <summary>
		/// Gets the runtime type of the column's value, as seen from the database mapping's perspective.
		/// For reference column's this will be the type of the ReferenceTargetMember.
		/// </summary>
		public Type RuntimeType
		{
			get
			{
				return (IsReference) 
					? ReferenceTargetMember.GetTypeOfValue()
					: Member.GetTypeOfValue();
			}
		}

		internal Type ReferenceTargetType
		{
			get
			{
				return (IsReference)
					? ReferenceTargetMember.DeclaringType
					: null;
			}
		}

		public string DbObjectReference
		{
			get
			{				
				return String.Concat(_mapping.DbObjectReference, '.', _mapping.QuoteObjectNameForSQL(TargetName));
			}
		}

		internal static ColumnMapping FromMember<T>(IMapping mapping, MemberInfo member, int ordinal)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);
			var memberType = member.MemberType;

			return new ColumnMapping<T>(mapping, member, ordinal);
		}
	}

	public sealed class ColumnMapping<T> : ColumnMapping
	{
		internal ColumnMapping(IMapping mapping, MemberInfo member, int ordinal)
			: base(mapping, member, ordinal)
		{
		}

		public ColumnMapping<T> WithBehaviors(ColumnBehaviors behaviors)
		{
			this.Behaviors = behaviors;
			return this;
		}

		public ColumnMapping<T> WithVariableLength(int length)
		{
			Contract.Requires(length >= 0, "length must be greater than zero");
			this.VariableLength = length;
			return this;
		}

		public ColumnMapping<T> WithReference<U>(Expression<Func<U, object>> expression, ReferenceBehaviors behaviors = ReferenceBehaviors.Lazy)
		{
			Contract.Requires(expression != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");
			Contract.Assert(member.DeclaringType == this.RuntimeType,
				"Type mismatch; typeof(U) must match the column's CLR type");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");
						
			this.ReferenceTargetMember = member;
			this.ReferenceBehaviors = behaviors;
			this.IsReference = true;
			return this;
		}

		internal ColumnMapping<T> DefineReference(ColumnMapping foreignColumn, ReferenceBehaviors behaviors = ReferenceBehaviors.Lazy)
		{
			this.ReferenceTargetMember = foreignColumn.Member;
			this.ReferenceBehaviors = behaviors;
			this.IsReference = true;
			this.VariableLength = foreignColumn.VariableLength;
			return this;
		}

		public ColumnMapping<T> WithTargetName(string name)
		{
			Contract.Requires(name != null, "name cannot be null");
			Contract.Requires(name.Length > 0, "name cannot be empty");

			this.TargetName = name;
			return this;
		}

		public Mapping<T> End()
		{
			return Mapping.Instance.ForType<T>();
		}
	}
}
