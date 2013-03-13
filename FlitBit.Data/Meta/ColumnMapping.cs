#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using FlitBit.Emit;

namespace FlitBit.Data.Meta
{
	public class ColumnMapping
	{
		protected ColumnMapping(IMapping mapping, MemberInfo member, int ordinal)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);

			this.Member = member;
			this.TargetName = member.Name;
			this.Ordinal = ordinal;
			this.Mapping = mapping;
		}

		public IMapping Mapping { get; private set; }
		public string TargetName { get; set; }
		public MemberInfo Member { get; private set; }
		public int Ordinal { get; internal set; }
		public ColumnBehaviors Behaviors { get; internal set; }
		public int VariableLength { get; internal set; }
		public bool IsReference { get; internal set; }

		public bool IsAlternateKey
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.AlternateKey); }
		}

		public bool IsCalculated
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.Calculated); }
		}

		public bool IsSynthetic
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.Synthetic); }
		}

		public bool IsImmutable
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.Immutable); }
		}

		public bool IsNullable
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.Nullable); }
		}

		public bool IsIdentity
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.Identity); }
		}

		public bool IsTimestampOnInsert
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnInsert); }
		}

		public bool IsTimestampOnUpdate
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnUpdate); }
		}

		public bool IsRevisionTracking
		{
			get { return Behaviors.HasFlag(ColumnBehaviors.RevisionConcurrency); }
		}

		public MemberInfo ReferenceTargetMember { get; internal set; }
		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }

		/// <summary>
		///   Gets the runtime type of the column's value, as seen from the database mapping's perspective.
		///   For reference column's this will be the type of the ReferenceTargetMember.
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
			get { return String.Concat(Mapping.DbObjectReference, '.', Mapping.QuoteObjectNameForSQL(TargetName)); }
		}

		internal static ColumnMapping FromMember<T>(IMapping mapping, MemberInfo member, int ordinal)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);
			var memberType = member.MemberType;

			return new ColumnMapping<T>(mapping, member, ordinal);
		}
	}
}