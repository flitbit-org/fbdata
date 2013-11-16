#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data.DataModel
{
	public interface IMapping
	{
		/// <summary>
		///   Indicates the entity's behaviors.
		/// </summary>
		EntityBehaviors Behaviors { get; }

		/// <summary>
		/// Gets descriptions of detected errors.
		/// </summary>
		IEnumerable<string> Errors { get; }

		/// <summary>
		/// Indicates whether the mapping has a binder.
		/// </summary>
		bool HasBinder { get; }

		/// <summary>
		/// Indicates whether the model has a defined identity.
		/// </summary>
		bool HasIdentity { get; }

		/// <summary>
		///   Gets the mapped type's identity mapping.
		/// </summary>
		IdentityMapping Identity { get; }

		/// <summary>
		///   Indicates the data model's runtime type.
		/// </summary>
		Type RuntimeType { get; }

		/// <summary>
		///   Indicates whether the mapping has been completed.
		/// </summary>
		bool IsComplete { get; }

		/// <summary>
		///   Indicates whether the entity is an enum type.
		/// </summary>
		bool IsEnum { get; }

    /// <summary>
    /// Indicates whether the mapping behaves like a lookup list.
    /// </summary>
    bool IsLookupList { get; }

		/// <summary>
		///   Indicates whether the entity's database object name is pluralized.
		/// </summary>
		bool IsPluralized { get; }

		/// <summary>
		///   The Db object to which type T maps; either a table or view.
		/// </summary>
		string TargetObject { get; set; }

		/// <summary>
		///   The Db schema where the target object resides.
		/// </summary>
		string TargetSchema { get; set; }

		/// <summary>
		///   The Db catalog (database) where the target object resides.
		/// </summary>
		string TargetCatalog { get; set; }

		/// <summary>
		///   The connection name where the type's data resides.
		/// </summary>
		string ConnectionName { get; set; }

		/// <summary>
		///   The ORM strategy.
		/// </summary>
		MappingStrategy Strategy { get; }

		/// <summary>
		///   The full name of the primary underlying database object.
		/// </summary>
		string DbObjectReference { get; }

		/// <summary>
		///   The columns that are mapped to the object.
		/// </summary>
		IEnumerable<ColumnMapping> Columns { get; }

		/// <summary>
		///   The columns that are mapped to the object.
		/// </summary>
		IEnumerable<ColumnMapping> DeclaredColumns { get; }

		/// <summary>
		/// Gets member info for each member participating in the database mapping.
		/// </summary>
		IEnumerable<MemberInfo> ParticipatingMembers { get; }

		/// <summary>
		///   The collections that are mapped to the object.
		/// </summary>
		IEnumerable<CollectionMapping> Collections { get; }

		/// <summary>
		///   The collections that are mapped to the object.
		/// </summary>
		IEnumerable<CollectionMapping> DeclaredCollections { get; }

		/// <summary>
		///   The data model's dependencies.
		/// </summary>
		IEnumerable<Dependency> Dependencies { get; }

		/// <summary>
		///   The data model's declared dependencies.
		/// </summary>
		IEnumerable<Dependency> DeclaredDependencies { get; }

		/// <summary>
		/// Gets the model's identity key type.
		/// </summary>
		Type IdentityKeyType { get; }

		/// <summary>
		/// Indicates the mapping's revision number.
		/// </summary>
		int Revision { get; }

		/// <summary>
		///   Completes the mapping.
		/// </summary>
		/// <returns></returns>
		Mappings End();

		/// <summary>
		///   Gets the DbProviderHelper associated with the mapping's connection.
		/// </summary>
		/// <returns></returns>
		DbProviderHelper GetDbProviderHelper();

		/// <summary>
		/// Gets the specified column's emitter.
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		MappedDbTypeEmitter GetColumnEmitter(ColumnMapping column);

		/// <summary>
		/// Marks a mapping as complete.
		/// </summary>
		void MarkComplete();

		/// <summary>
		/// Gets the type's database object name quoted for the underlying database's script syntax.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string QuoteObjectName(string name);

		/// <summary>
		/// Registers a callback action to be invoked when the mapping is completed.
		/// </summary>
		/// <param name="action">the callback action</param>
		/// <returns>the mapping itself</returns>
		IMapping OnCompleted(Action<IMapping> action);

		/// <summary>
		///   Gets the mapping's model binder.
		/// </summary>
		/// <returns></returns>
		IDataModelBinder GetBinder();

		/// <summary>
		///   Notifies the mapping of a subtype.
		/// </summary>
		/// <param name="mapping"></param>
		void NotifySubtype(IMapping mapping);
	}


	public interface IMapping<TModel> : IMapping
	{
		/// <summary>
		/// The model's hierarchy mapping.
		/// </summary>
		HierarchyMapping<TModel> Hierarchy { get; }

		/// <summary>
		/// The model's identity key.
		/// </summary>
		IdentityKey<TModel> IdentityKey { get; }

		
		/// <summary>
		/// Gets the mapping's natural key if defined.
		/// </summary>
		NaturalKeyMapping<TModel> NaturalKey { get; }

		/// <summary>
		/// Maps a collection.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		CollectionMapping<TModel> Collection(Expression<Func<TModel, object>> expression);

		/// <summary>
		///   Maps a column to a member (property or field) of the object.
		///   The property or field indicated will serve as the column definition.
		/// </summary>
		/// <param name="expression">
		///   An expression that identifies the member
		///   upon which a column will be mapped.
		/// </param>
		/// <returns>
		///   A ColumnMapping object for further refinement of the column's
		///   definition.
		/// </returns>
		ColumnMapping<TModel> Column(Expression<Func<TModel, object>> expression);

		/// <summary>
		/// Establishes a mapping to a schema in the database.
		/// </summary>
		/// <param name="schema"></param>
		/// <returns></returns>
		IMapping<TModel> InSchema(string schema);

		/// <summary>
		/// Infers a mapped collection's reference target member.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="elmMapping"></param>
		/// <returns></returns>
		MemberInfo InferCollectionReferenceTargetMember(MemberInfo member, IMapping elmMapping);


		IMapping<TModel> MapAllOperations();

		IMapping<TModel> MapAllOperations(MappingStrategy strategy);

		/// <summary>
		/// Establishes a reference to another type.
		/// </summary>
		/// <typeparam name="TOther"></typeparam>
		/// <param name="expression"></param>
		/// <returns></returns>
		IMapping<TModel> ReferencesType<TOther>(Expression<Func<TModel, TOther, bool>> expression);

		/// <summary>
		/// Identifies the mapping's connection to a database.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		IMapping<TModel> UsesConnection(string connection);

		/// <summary>
		///   Sets the database object name
		/// </summary>
		/// <param name="name">
		///   Name of the database object where data is stored for
		///   the type.
		/// </param>
		/// <returns></returns>
		IMapping<TModel> WithName(string name);

		/// <summary>
		/// Adds a contributed column.
		/// </summary>
		/// <param name="contributed"></param>
		void AddContributedColumn(ColumnMapping contributed);

		/// <summary>
		/// Adds a dependency.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="kind"></param>
		/// <param name="member"></param>
		void AddDependency(IMapping target, DependencyKind kind, MemberInfo member);

		/// <summary>
		/// Defines a collection mapping.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		CollectionMapping<TModel> DefineCollection(PropertyInfo property);

		/// <summary>
		///  Defines a column mapping.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		ColumnMapping<TModel> DefineColumn(MemberInfo member);

		/// <summary>
		/// Initializes the mapping based on metadata.
		/// </summary>
		/// <returns></returns>
		IMapping<TModel> InitFromMetadata();

		/// <summary>
		/// Maps a collection from metadata.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="mapColl"></param>
		void MapCollectionFromMeta(PropertyInfo p, MapCollectionAttribute mapColl);

		/// <summary>
		/// Maps a column from metadata.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="mapColumn"></param>
		void MapColumnFromMeta(PropertyInfo p, MapColumnAttribute mapColumn);


  }

}