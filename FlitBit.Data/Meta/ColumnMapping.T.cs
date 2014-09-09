#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

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

        public IMapping<T> End() { return (IMapping<T>)Mapping; }

        public ColumnMapping<T> WithBehaviors(ColumnBehaviors behaviors)
        {
            this.Behaviors = behaviors;
            return this;
        }

        public ColumnMapping<T> WithReference<U>(Expression<Func<U, object>> expression,
            ReferenceBehaviors behaviors = ReferenceBehaviors.Lazy)
        {
            Contract.Requires<ArgumentNullException>(expression != null);

            var member = expression.GetMemberFromExpression();
            Contract.Assert(member != null, "Expression must reference a field or property member");
            Contract.Assert(member.DeclaringType == this.RuntimeType,
                "Type mismatch; typeof(U) must match the column's CLR type");

            var memberType = member.MemberType;
            Contract.Assert(memberType == MemberTypes.Field
                            || memberType == MemberTypes.Property,
                "Expression must reference a field or property member");

            this.ReferenceTargetMember = member;
            this.ReferenceBehaviors = behaviors;
            this.IsReference = true;
            return this;
        }

        public ColumnMapping<T> WithTargetName(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name cannot be null");
            Contract.Requires<ArgumentException>(name.Length > 0, "name cannot be empty");

            this.TargetName = name;
            return this;
        }

        public ColumnMapping<T> WithVariableLength(int length)
        {
            Contract.Requires<ArgumentException>(length >= 0, "length must be greater than zero");
            this.VariableLength = length;
            return this;
        }

        public ColumnMapping<T> WithPrecision(short precision)
        {
            Contract.Requires<ArgumentException>(precision >= 0, "precision must be greater than zero");
            this.Precision = precision;
            return this;
        }

        public ColumnMapping<T> WithScale(byte scale)
        {
            this.Scale = scale;
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