﻿#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

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

		public ColumnBehaviors Behaviors { get; internal set; }
		public string DbObjectReference { get { return String.Concat(Mapping.DbObjectReference, '.', Mapping.QuoteObjectNameForSQL(TargetName)); } }

		public bool IsAlternateKey { get { return Behaviors.HasFlag(ColumnBehaviors.AlternateKey); } }

		public bool IsCalculated { get { return Behaviors.HasFlag(ColumnBehaviors.Calculated); } }
		public bool IsIdentity { get { return Behaviors.HasFlag(ColumnBehaviors.Identity); } }

		public bool IsImmutable { get { return Behaviors.HasFlag(ColumnBehaviors.Immutable); } }

		public bool IsNullable { get { return Behaviors.HasFlag(ColumnBehaviors.Nullable); } }
		public bool IsReference { get; internal set; }
		public bool IsRevisionTracking { get { return Behaviors.HasFlag(ColumnBehaviors.RevisionConcurrency); } }
		public bool IsSynthetic { get { return Behaviors.HasFlag(ColumnBehaviors.Synthetic); } }

		public bool IsTimestampOnInsert { get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnInsert); } }

		public bool IsTimestampOnUpdate { get { return Behaviors.HasFlag(ColumnBehaviors.TimestampOnUpdate); } }
		public IMapping Mapping { get; private set; }
		public MemberInfo Member { get; private set; }
		public int Ordinal { get; internal set; }

		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }
		public MemberInfo ReferenceTargetMember { get; internal set; }

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

		public string TargetName { get; set; }
		public int VariableLength { get; internal set; }

		internal Type ReferenceTargetType
		{
			get
			{
				return (IsReference)
					? ReferenceTargetMember.DeclaringType
					: null;
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
}