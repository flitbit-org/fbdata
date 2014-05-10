#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Data.DataModel
{
  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1, TParam2>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1, TParam2>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  /// <typeparam name="TParam5"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4, in TParam5>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  /// <typeparam name="TParam5"></typeparam>
  /// <typeparam name="TParam6"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4, in TParam5, in TParam6>
    : IDataModelQuerySingleCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>,
      IDataModelQueryManyCommand<TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  /// <typeparam name="TParam5"></typeparam>
  /// <typeparam name="TParam6"></typeparam>
  /// <typeparam name="TParam7"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4, in TParam5, in TParam6, in TParam7>
    :
      IDataModelQuerySingleCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>,
      IDataModelQueryManyCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  /// <typeparam name="TParam5"></typeparam>
  /// <typeparam name="TParam6"></typeparam>
  /// <typeparam name="TParam7"></typeparam>
  /// <typeparam name="TParam8"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4, in TParam5, in TParam6, in TParam7, in TParam8>
    :
      IDataModelQuerySingleCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>,
      IDataModelQueryManyCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
  {}

  /// <summary>
  ///   Interface for data model query commands.
  /// </summary>
  /// <typeparam name="TModel"></typeparam>
  /// <typeparam name="TDbConnection"></typeparam>
  /// <typeparam name="TParam"></typeparam>
  /// <typeparam name="TParam1"></typeparam>
  /// <typeparam name="TParam2"></typeparam>
  /// <typeparam name="TParam3"></typeparam>
  /// <typeparam name="TParam4"></typeparam>
  /// <typeparam name="TParam5"></typeparam>
  /// <typeparam name="TParam6"></typeparam>
  /// <typeparam name="TParam7"></typeparam>
  /// <typeparam name="TParam8"></typeparam>
  /// <typeparam name="TParam9"></typeparam>
  public interface IDataModelQueryCommand<out TModel, TDbConnection, in TParam, in TParam1, in TParam2, in TParam3,
                                          in TParam4, in TParam5, in TParam6, in TParam7, in TParam8, in TParam9>
    :
      IDataModelQuerySingleCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>,
      IDataModelQueryManyCommand
        <TModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
  {}
}