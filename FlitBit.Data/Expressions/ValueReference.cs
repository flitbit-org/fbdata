using System;
using System.Reflection;
using FlitBit.Data.Meta;

namespace FlitBit.Data.Expressions
{
	public class ValueReference
	{
		public ValueReference(ValueReferenceKind kind)
		{
			Kind = kind;
		}
		public ValueReferenceKind Kind { get; private set; }
		public string Value { get; set; }
		public Join Join { get; set; }

		internal bool IsLiftCandidateFor(Join j)
		{
			var join = this.Join;
			return join == null || join.Ordinal <= j.Ordinal;
		}

		public ColumnMapping Column { get; private set; }

		public Parameter Parameter { get; set; }

		public ValueReference AssociateColumn(ColumnMapping col)
		{
			if (Column == null)
			{
				Column = col;
				if (Parameter != null && Parameter.Column == null)
				{
					Parameter.Column = col;
				}
			}
			return this;
		}
	}

	public class MemberValueReference : ValueReference
	{
		public MemberValueReference(MemberInfo member)
			: this(ValueReferenceKind.Member, member)
		{
		}

		public MemberValueReference(ValueReferenceKind kind, MemberInfo member)
			: base(kind)
		{
			Member = member;
		}

		public MemberInfo Member { get; private set; }
	}

	public class ParameterValueReference : ValueReference
	{
		public ParameterValueReference(string name, int ordinal, Type type)
			: base(ValueReferenceKind.Parameter)
		{
			Name = name;
			Ordinal = ordinal;
			RuntimeType = type;
		}

		public Type RuntimeType { get; set; }

		public int Ordinal { get; set; }

		public string Name { get; set; }
	}
}