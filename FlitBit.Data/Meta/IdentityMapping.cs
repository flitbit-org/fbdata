#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Core;

namespace FlitBit.Data.Meta
{
	public abstract class IdentityMapping
	{
		protected readonly Dictionary<string, MappedSortColumn> _columns = new Dictionary<string, MappedSortColumn>();

		public string TargetName { get; protected set; }

		public IList<MappedSortColumn> Columns
		{
			get
			{
				return _columns.Values.OrderBy(c => c.Ordinal)
					.ToReadOnly();
			}
		}

		internal void AddColumn(ColumnMapping column, SortOrderKind sortOrder)
		{
			Contract.Requires<ArgumentNullException>(column != null);

			MappedSortColumn existing;
			Contract.Assert(!_columns.TryGetValue(column.TargetName, out existing),
				"A column may only appear in the identity once");

			_columns.Add(column.TargetName, new MappedSortColumn(column, sortOrder, _columns.Count));
		}
	}

	public class IdentityMapping<T> : IdentityMapping
	{
		private readonly Mapping<T> _owner;

		internal IdentityMapping(Mapping<T> owner)
		{
			Contract.Requires<ArgumentNullException>(owner != null);
			_owner = owner;
		}

		public IdentityMapping<T> Column(Expression<Func<T, object>> expression, SortOrderKind sortOrder)
		{
			Contract.Requires<ArgumentNullException>(expression != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			MemberTypes memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
			                || memberType == MemberTypes.Property, "Expression must reference a field or property member"
				);

			string name = member.Name;
			ColumnMapping col = _owner.Columns.SingleOrDefault(c => c.Member == member);

			Contract.Assert(col != null, "A column must be defined on the member before it can be used as an identity");
			Contract.Assert(col.Behaviors.HasFlag(ColumnBehaviors.Identity),
				"Column must include Identity behaviors in order to use it as an identity");

			MappedSortColumn existing;
			Contract.Assert(!_columns.TryGetValue(name, out existing), "A column may only appear in the identity once");

			AddColumn(col, sortOrder);

			return this;
		}

		public Mapping<T> End()
		{
			return _owner;
		}
	}
}