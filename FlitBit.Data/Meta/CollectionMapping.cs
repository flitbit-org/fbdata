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
	public class CollectionMapping<T>
	{
		Mapping<T> _mapping;
		MemberInfo _referenceTargetMember;

		internal CollectionMapping(Mapping<T> mapping, MemberInfo member)
		{
			Contract.Requires(mapping != null);
			Contract.Requires(member != null);
			
			this.Member = member;
			this.ElementType = member.GetTypeOfValue().FindElementType();
			this.TargetName = member.Name;
			this._mapping = mapping;
		}

		public string TargetName { get; set; }
		public MemberInfo Member { get; private set; }
		public MemberInfo ReferenceJoinMember
		{
			get
			{
				if (_referenceTargetMember == null)
				{
					_referenceTargetMember = _mapping.InferCollectionReferenceTargetMember(Member, ElementType);
				}
				return _referenceTargetMember;
			}
			internal set
			{
				_referenceTargetMember = value;
			}
		}
		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }
		public Type ElementType { get; private set; }
		internal Type CollectionType { get; set; }
		internal ColumnMapping BackReference { get; set; }

		public CollectionMapping<T> JoinReference<U>(Expression<Func<U, object>> expression)
		{
			Contract.Requires(expression != null);			

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");
			Contract.Assert(member.DeclaringType == this.ElementType,
				"Type mismatch; typeof(U) must match the collection's element type");
			
			this.ReferenceJoinMember = member;
			return this;
		}

		public CollectionMapping<T> Where<U>(Expression<Func<T, U, bool>> expression)
		{
			Contract.Requires(expression != null);
			
			if (expression.Body is BinaryExpression)
			{
				BinaryExpression bin = (BinaryExpression)expression.Body;				
				
			}			
			return this;
		}

		public Mapping<T> End()
		{
			return _mapping;
		}
	}
}

