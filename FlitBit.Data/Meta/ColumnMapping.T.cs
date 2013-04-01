using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;
using FlitBit.Data.DataModel;

namespace FlitBit.Data.Meta
{
	public sealed class ColumnMapping<T> : ColumnMapping
	{
		internal ColumnMapping(IMapping mapping, MemberInfo member, int ordinal)
			: base(mapping, member, ordinal)
		{}

		public Mapping<T> End()
		{
			return (Mapping<T>) Mapping;
		}

		public ColumnMapping<T> WithBehaviors(ColumnBehaviors behaviors)
		{
			this.Behaviors = behaviors;
			return this;
		}

		public ColumnMapping<T> WithReference<U>(Expression<Func<U, object>> expression,
			ReferenceBehaviors behaviors = ReferenceBehaviors.Lazy)
		{
			Contract.Requires(expression != null);

			var member = expression.GetMemberFromExpression();
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

		public ColumnMapping<T> WithTargetName(string name)
		{
			Contract.Requires(name != null, "name cannot be null");
			Contract.Requires(name.Length > 0, "name cannot be empty");

			this.TargetName = name;
			return this;
		}

		public ColumnMapping<T> WithVariableLength(int length)
		{
			Contract.Requires(length >= 0, "length must be greater than zero");
			this.VariableLength = length;
			return this;
		}

		internal ColumnMapping<T> DefineReference(ColumnMapping foreignColumn,
			ReferenceBehaviors behaviors = ReferenceBehaviors.Lazy)
		{
			this.ReferenceTargetMember = foreignColumn.Member;
			this.ReferenceBehaviors = behaviors;
			this.IsReference = true;
			this.VariableLength = foreignColumn.VariableLength;
			return this;
		}
	}
}