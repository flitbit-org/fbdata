#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public class CollectionMapping<T> : CollectionMapping
	{
		internal CollectionMapping(Mapping<T> mapping, MemberInfo member)
			: base(mapping, member)
		{}

		public Mapping<T> End()
		{
			return (Mapping<T>) Mapping;
		}

		public CollectionMapping<T> JoinReference<U>(Expression<Func<U, object>> expression)
		{
			Contract.Requires(expression != null);

			var member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member");
			Contract.Assert(member.DeclaringType == this.ElementType,
											"Type mismatch; typeof(U) must match the collection's element type");

			this.ReferenceJoinMember = member;
			return this;
		}

		public CollectionMapping Where<U>(Expression<Func<T, U, bool>> expression)
		{
			Contract.Requires(expression != null);

			if (expression.Body is BinaryExpression)
			{
				var bin = (BinaryExpression) expression.Body;
			}
			return this;
		}

		protected override MemberInfo InferCollectionReferenceTargetMember(MemberInfo member, Type elementType)
		{
			var typedMapping = (Mapping<T>) Mapping;
			IMapping elmMapping = (elementType == typedMapping.RuntimeType) ? typedMapping : Mappings.AccessMappingFor(elementType);
			
			return typedMapping.InferCollectionReferenceTargetMember(member, elmMapping);
		}
	}
}