#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using FlitBit.Emit;

namespace FlitBit.Data.Meta
{
	public abstract class CollectionMapping
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		MemberInfo _referenceTargetMember;

		internal CollectionMapping(IMapping mapping, MemberInfo member)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);

			this.Member = member;
			this.ElementType = member.GetTypeOfValue()
															.FindElementType();
			this.TargetName = member.Name;
			this.Mapping = mapping;
		}

		public Type ElementType { get; private set; }

		public IMapping Mapping { get; private set; }
		public MemberInfo Member { get; private set; }

		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }

		public MemberInfo ReferenceJoinMember
		{
			get
			{
				if (_referenceTargetMember == null)
				{
					_referenceTargetMember = InferCollectionReferenceTargetMember(Member, ElementType);
				}
				return _referenceTargetMember;
			}
			internal set { _referenceTargetMember = value; }
		}

		public string TargetName { get; set; }

		internal ColumnMapping BackReference { get; set; }
		internal Type CollectionType { get; set; }
		protected abstract MemberInfo InferCollectionReferenceTargetMember(MemberInfo Member, Type ElementType);
	}
}