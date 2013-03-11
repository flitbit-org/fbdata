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
	public class NaturalKeyMapping<T>
	{
		Mapping<T> _owner;
		readonly Dictionary<string, ColumnMapping> _columns = new Dictionary<string, ColumnMapping>();

		internal NaturalKeyMapping(Mapping<T> owner)
		{
			Contract.Requires(owner != null);			
			_owner = owner;
		}

		public string TargetName { get; protected set; }
		
		/// <summary>
		/// The columns that are mapped to the identity.
		/// </summary>
		public IEnumerable<ColumnMapping> Columns
		{
			get
			{
				return (from x in _columns.Values
								orderby x.Ordinal
								select x).ToArray();
			}
		}

		public NaturalKeyMapping<T> Column(Expression<Func<T, object>> expression)
		{
			Contract.Requires(expression != null);

			MemberInfo member = expression.GetMemberFromExpression();
			Contract.Assert(member != null, "Expression must reference a field or property member");

			var memberType = member.MemberType;
			Contract.Assert(memberType == MemberTypes.Field
				|| memberType == MemberTypes.Property, "Expression must reference a field or property member"
				);
			
			var name = member.Name;
			var col = (from c in _owner.Columns
								 where c.Member == member
								 select c).SingleOrDefault();
			
			Contract.Assert(col != null, "A column must be defined on the member before it can be used as an identity");
			ColumnMapping existing;
			Contract.Assert(!_columns.TryGetValue(name, out existing), "A column may only appear in the identity once");

			_columns.Add(name, col);

			return this;
		}
		
		public Mapping<T> End()
		{
			return Mappings.Instance.ForType<T>();
		}
	}
}
