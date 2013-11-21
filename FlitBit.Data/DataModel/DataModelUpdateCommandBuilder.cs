using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using FlitBit.Data.Expressions;
using FlitBit.Data.SqlServer;

namespace FlitBit.Data.DataModel
{
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam> Where(
            Expression<Func<TDataModel, TParam, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam> ConstructCommandOnConstraints(
            Constraints constraints);
    }
    /// <summary>
    /// Builds SQL commands over a data model.
    /// </summary>
    /// <typeparam name="TDataModel">data model's type</typeparam>
    /// <typeparam name="TDbConnection">db connection type</typeparam>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TParam1"></typeparam>
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1> Where(
            Expression<Func<TDataModel, TParam, TParam1, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> ConstructCommandOnConstraints(
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
    public abstract class DataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> : DataModelCommandBuilder<TDataModel>,
        IDataModelUpdateCommandBuilder<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="queryKey">the query's key</param>
        /// <param name="sqlWriter">a writer</param>
        protected DataModelUpdateCommandBuilder(IDataModelBinder<TDataModel> binder, string queryKey, DataModelSqlWriter<TDataModel> sqlWriter)
            : base(binder, queryKey, sqlWriter)
        {
        }

        /// <summary>
        /// Specifies constraints on the data model. The expression must evaluate like a predicate in order to be translated to SQL.
        /// </summary>
        /// <param name="predicate">a predicate expression</param>
        /// <returns></returns>
        public IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> Where(
            Expression<Func<TDataModel, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, bool>> predicate)
        {
            var cns = new Constraints();
            var lambda = (LambdaExpression)predicate;
            var i = 0;
            foreach (var parm in lambda.Parameters)
            {
              cns.Arguments.Add(new ParameterValueReference(parm.Name, i++, parm.Type));
            }
            return ConstructCommandOnConstraints(
                PrepareTranslateExpression(cns, lambda.Body)
                );
        }

        /// <summary>
        /// Builds a query command with the specified constraints.
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        protected abstract IDataModelNonQueryCommand<TDataModel, TDbConnection, TParam, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> ConstructCommandOnConstraints(
            Constraints constraints);
    }
}