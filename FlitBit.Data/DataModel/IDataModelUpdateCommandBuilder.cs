#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using System.Linq.Expressions;

namespace FlitBit.Data.DataModel
{
    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam> Where(
            Expression<Func<TDataModel, TParam, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where(
            Expression<Func<TDataModel, TParam, TParam1, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4, TParam5>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4, TParam5, TParam6>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand
            <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
            Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4, TParam5, TParam6, TParam7>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand
            <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>>
                predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
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
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4, TParam5, TParam6, TParam7, TParam8>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand
            <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
            Where(
            Expression
                <Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>>
                predicate);
    }

    /// <summary>
    ///     Builds a data model command, binding parameters of the specified types.
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
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
    public interface IDataModelUpdateCommandBuilder<TDataModel, in TDbConnection, TParam, TParam1, TParam2, TParam3,
                                                    TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
        where TDbConnection : DbConnection
    {
        /// <summary>
        ///     Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to
        ///     SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        IDataModelNonQueryCommand
            <TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
                TParam9> Where(
            Expression
                <
                    Func
                        <TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8,
                            TParam9, bool>
                    > predicate);
    }
}