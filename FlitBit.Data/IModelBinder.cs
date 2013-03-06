using System.Data.Common;

namespace FlitBit.Data
{
	/// <summary>
	/// Indicates the model binding scheme used.
	/// </summary>
	/// <remarks>
	/// These are analogous to ORM patterns documented since 
	/// the `90s here http://www.objectarchitects.de/ObjectArchitects/orpatterns/
	/// and later here http://martinfowler.com/eaaCatalog/
	/// </remarks>
	public enum ModelBindingScheme
	{
		/// <summary>
		/// Indicates the default `DynamicHybridInheritanceTree`
		/// </summary>
		Default = 0,
		/// <summary>
		/// A hybrid inheritance path scheme.
		/// </summary>
		DynamicHybridInheritanceTree = 0,
		/// <summary>
		/// Indicates that one class maps to one table.
		/// </summary>
		/// <remarks>
		/// As described by http://www.objectarchitects.de/ObjectArchitects/orpatterns/ 
		/// Map the attributes of each class to a separate table. Insert a Synthetic OID into each table 
		/// to link derived classes rows with their parent table's corresponding rows.
		/// </remarks>
		OneClassOneTable = 1,
		/// <summary>
		/// Indicates that one class hierarchy maps to one table.
		/// </summary>																					 
		/// <remarks>
		/// As described by http://www.objectarchitects.de/ObjectArchitects/orpatterns/ 
		/// Use the union of all attributes of all objects in the inheritance hierarchy as the 
		/// columns of a single database table. Use Null values to fill the unused fields in each record.
		/// </remarks>
		OneInheritanceTreeOneTable = 2,
		/// <summary>
		/// Maps each class in a hierarchy to its own table.
		/// </summary>
		/// <remarks>
		/// As described by http://www.objectarchitects.de/ObjectArchitects/orpatterns/ 
		/// Map the attributes of each class to a separate table. To a classes’ 
		/// table add the attributes of all classes the class inherits from.
		/// </remarks>
		OneInheritancePathOneTable = 3,
	}

	public interface IModelBinder<TModel, Id, in TModelImpl, in TDbConnection>
		where TModelImpl: class, TModel, new()
		where TDbConnection : DbConnection
	{
		ModelBindingScheme Scheme { get; }
		IModelCommand<TModel, TModelImpl, TDbConnection> GetCreateCommand();
		IModelCommand<TModel, Id, TDbConnection> GetReadCommand();
		IModelCommand<TModel, TModelImpl, TDbConnection> GetUpdateCommand();
		IModelCommand<TModel, Id, TDbConnection> GetDeleteCommand();
		IModelCommand<TModel, object, TDbConnection> GetAllCommand();
		IModelCommand<TModel, TMatch, TDbConnection> MakeReadMatchCommand<TMatch>(TMatch match)
			where TMatch : class;
		IModelCommand<TModel, TMatch, TDbConnection> MakeUpdateMatchCommand<TMatch>(TMatch match)
			where TMatch : class;
		IModelCommand<TModel, TMatch, TDbConnection> MakeDeleteMatchCommand<TMatch>(TMatch match)
			where TMatch : class;
	}	
}
