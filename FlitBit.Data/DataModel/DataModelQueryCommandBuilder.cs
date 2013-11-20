using System;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Data.Expressions;
using FlitBit.Data.Meta;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data.DataModel
{
  /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    public abstract class DataModelCommandBuilder<TDataModel, TDbConnection, TParam> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam> Where(
            Expression<Func<TDataModel, TParam, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam> ConstructCommandOnConstraints(
            Constraints constraints);
    }

    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1> :
        DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where(
            Expression<Func<TDataModel, TParam, TParam1, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression) predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1>
            ConstructCommandOnConstraints(
            Constraints constraints);
    }

    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    /// <typeparam name="TParam2"></typeparam>
    /// <typeparam name="TParam3"></typeparam>
    /// <typeparam name="TParam4"></typeparam>
    /// <typeparam name="TParam5"></typeparam>
    /// <typeparam name="TParam6"></typeparam>
    /// <typeparam name="TParam7"></typeparam>
    /// <typeparam name="TParam8"></typeparam>
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
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
    public abstract class DataModelQueryCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> : DataModelCommandBuilder<TDataModel>,
        IDataModelCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelQueryCommandBuilder(string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            cns.Arguments = lambda.Parameters.Select(p => new ParameterValueReference(p.Name, i++, p.Type));

            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> ConstructCommandOnConstraints(
            Constraints constraints);
    }
}