#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using FlitBit.Data.DataModel;
using FlitBit.Emit;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public class ColumnMapping
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Tuple<int, MappedDbTypeEmitter> _emitter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		DbTypeDetails _detail;

		public MappedDbTypeEmitter Emitter
		{
			get
			{
				if (_emitter == null || _emitter.Item1 != Mapping.Revision)
				{
					_emitter = Tuple.Create<int, MappedDbTypeEmitter>(Mapping.Revision, Mapping.GetColumnEmitter(this));
				}
				return _emitter.Item2;
			}
		}

		protected ColumnMapping(IMapping mapping, MemberInfo member, int ordinal)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(member != null);
			
			Member = member;
			TargetName = member.Name;
			Ordinal = ordinal;
			Mapping = mapping;
		}

		public ColumnBehaviors Behaviors { get; internal set; }

		public string DbObjectReference { get { return String.Concat(Mapping.DbObjectReference, '.', Mapping.QuoteObjectName(TargetName)); } }

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
				return Member.GetTypeOfValue();
			}
		}

		public string TargetName { get; set; }

		public int VariableLength { get; internal set; }

    public short Precision { get; internal set; }

    public byte Scale { get; internal set; }

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
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentException>(member != null);
			
			return new ColumnMapping<T>(mapping, member, ordinal);
		}

		public DbTypeDetails DbTypeDetails
		{
			get
			{
				if (_detail.IsEmpty)
				{
					_detail = ResolveDbTypeDetails();
				}
				return _detail;
			}
		}

		protected virtual DbTypeDetails ResolveDbTypeDetails()
		{
			var emitter = this.Emitter;
			if (emitter != null)
			{
				return emitter.GetDbTypeDetails(this);
			}
			else return default(DbTypeDetails);
		}

		public override string ToString()
		{
			var buffer = new StringBuilder(200);
			var emitter = this.Emitter;
			buffer.Append(Member.DeclaringType.GetReadableSimpleName())
						.Append('.')
						.Append(Member.Name)
						.Append(": ")
						.Append(Member.GetTypeOfValue()
													.GetReadableSimpleName())
						.Append(" ~> ");
			if (emitter != null)
			{
				emitter.DescribeColumn(buffer, this);
			}
			return buffer.ToString();
		}

		public Type UnderlyingType
		{
			get
			{
				return Emitter.UnderlyingType;
			}
		}
	}
}