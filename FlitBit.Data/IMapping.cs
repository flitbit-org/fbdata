#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	public interface IMapping
	{
		bool IsComplete { get; }

		Type RuntimeType { get; }

		/// <summary>
		/// The Db object to which type T maps; either a table or view.
		/// </summary>
		string TargetObject { get; }
		/// <summary>
		/// The Db schema where the target object resides.
		/// </summary>
		string TargetSchema { get; }
		/// <summary>
		/// The Db catalog (database) where the target object resides.
		/// </summary>
		string TargetCatalog { get; }
		/// <summary>
		/// The connection name where the type's data resides.
		/// </summary>
		string ConnectionName { get; }
		/// <summary>
		/// The ORM strategy.
		/// </summary>
		MappingStrategy Strategy { get; }

		bool IsEnum { get; }

		string DbObjectReference { get; }

		string QuoteObjectNameForSQL(string name);

		IEnumerable<ColumnMapping> Columns { get; }
		IEnumerable<ColumnMapping> DeclaredColumns { get; }

		IEnumerable<CollectionMapping> Collections { get; }
		IEnumerable<CollectionMapping> DeclaredCollections { get; }

		IEnumerable<Dependency> Dependencies { get; }
		IEnumerable<Dependency> DeclaredDependencies { get; }

		/// <summary>
		/// All members on the RuntimeType participating in the mapping.
		/// </summary>
		IEnumerable<MemberInfo> ParticipatingMembers { get; }

		IModelBinder GetBinder();

		IMapping Completed(Action action);

		void NotifySubtype(IMapping mapping);
	}

	public interface IMapping<out M> : IMapping
	{	
	}
}
