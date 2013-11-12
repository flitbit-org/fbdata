#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using FlitBit.Data.DataModel;
using FlitBit.Emit;

namespace FlitBit.Data.Meta
{
	public abstract class CollectionMapping
	{
		internal CollectionMapping(IMapping mapping, MemberInfo member)
		{
			Contract.Requires<ArgumentNullException>(mapping != null);
			Contract.Requires<ArgumentNullException>(member != null);

			LocalMapping = mapping;
			LocalMember = member;
		}

		public MemberInfo LocalMember { get; internal set; }

		public IMapping LocalMapping { get; private set; }

		public IList<MemberInfo> LocalJoinProperties { get; internal set; }

		public ReferenceBehaviors ReferenceBehaviors { get; internal set; }

		public Type ReferencedType { get; internal set; }

		public IMapping ReferencedMapping { get; internal set; }

		public IList<MemberInfo> ReferencedProperties { get; internal set; }

		internal Type MakeCollectionReferenceType()
		{
			var paramTypes = new List<Type>();
			var arity = ReferencedProperties.Count;
			for (var i = 0; i < arity; i++)
			{
				paramTypes.Add(CalculateColumnTypeFromJoinProperties(LocalJoinProperties[i], ReferencedProperties[i]));
			}

			switch (arity)
			{
				case 1: return typeof(DataModelCollectionReference<,>).MakeGenericType(ReferencedType, paramTypes[0]);
				default: throw new NotImplementedException();
			}
		}

		private Type CalculateColumnTypeFromJoinProperties(MemberInfo local, MemberInfo reference)
		{
			Contract.Requires<ArgumentNullException>(local != null);
			Contract.Requires<ArgumentNullException>(reference != null);

			var lcolumn = LocalMapping.Columns.SingleOrDefault(c => c.Member == local);
			if (lcolumn == null)
			{
				throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".", LocalMember.Name,
					" uses a local property that is not mapped to a column: ", LocalMapping.RuntimeType.Name, ".", local.Name)
					);
			}
			var rcolumn = ReferencedMapping.Columns.SingleOrDefault(c => c.Member == reference);
			if (rcolumn == null)
			{
				throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".", LocalMember.Name,
					" references an property that is not mapped to a column: ", ReferencedType.Name, ".", reference.Name)
					);
			}
			if (lcolumn.UnderlyingType != rcolumn.UnderlyingType)
			{
				throw new MappingException(String.Concat("The mapped collection on ", LocalMapping.RuntimeType.Name, ".", LocalMember.Name,
					" references incompatible properties: ", LocalMapping.RuntimeType.Name, ".", local.Name, " cannot equal ", ReferencedType.Name, ".", reference.Name)
					);
			}
			return lcolumn.UnderlyingType;
		}
	}
}