﻿#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
	public interface IMapping
	{
		EntityBehaviors Behaviors { get; }

		IEnumerable<CollectionMapping> Collections { get; }

		IEnumerable<ColumnMapping> Columns { get; }

		/// <summary>
		///   The connection name where the type's data resides.
		/// </summary>
		string ConnectionName { get; }

		string DbObjectReference { get; }

		IEnumerable<CollectionMapping> DeclaredCollections { get; }
		IEnumerable<ColumnMapping> DeclaredColumns { get; }

		IEnumerable<Dependency> DeclaredDependencies { get; }
		IEnumerable<Dependency> Dependencies { get; }

		IdentityMapping Identity { get; }

		bool IsComplete { get; }
		bool IsEnum { get; }

		/// <summary>
		///   All members on the RuntimeType participating in the mapping.
		/// </summary>
		IEnumerable<MemberInfo> ParticipatingMembers { get; }

		/// <summary>
		/// The mapping's runtime type.
		/// </summary>
		Type RuntimeType { get; }

		/// <summary>
		///   The ORM strategy.
		/// </summary>
		MappingStrategy Strategy { get; }

		/// <summary>
		///   The Db catalog (database) where the target object resides.
		/// </summary>
		string TargetCatalog { get; }

		/// <summary>
		///   The Db object to which type T maps; either a table or view.
		/// </summary>
		string TargetObject { get; }

		/// <summary>
		///   The Db schema where the target object resides.
		/// </summary>
		string TargetSchema { get; }

		Type IdentityKeyType { get; }

		int Revision { get; }

		IMapping Completed(Action action);

		IDataModelBinder GetBinder();

		void NotifySubtype(IMapping mapping);

		string QuoteObjectName(string name);

		MappedDbTypeEmitter GetEmitterFor(ColumnMapping column);

		DbProviderHelper GetDbProviderHelper();

		/// <summary>
		/// Indicates whether the mapping has a binder.
		/// </summary>
		bool HasBinder { get; }
	}


	public interface IMapping<out TModel> : IMapping
	{
		Type ConcreteType { get; }
	}
}