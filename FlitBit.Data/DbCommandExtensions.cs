#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Data.Properties;

namespace FlitBit.Data
{
  public static class DbCommandExtensions
  {
    public static DbCommand BindParameters(this DbCommand command, Action<IDataParameterBinder> binder)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(binder != null);

      var b = DataParameterBinders.GetBinderForDbCommand(command);
      binder(b);
      b.PrepareDbCommand(command);
      return command;
    }

    public static IEnumerable<IDataRecord> ExecuteEnumerable(this DbCommand command)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));
      Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

      using (var reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          yield return reader;
        }
      }
    }

    public static IEnumerable<IDataRecord> ExecuteEnumerable(this DbCommand command, CommandBehavior behavior)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));
      Contract.Ensures(Contract.Result<IEnumerable<IDataRecord>>() != null);

      using (var reader = command.ExecuteReader(behavior))
      {
        while (reader.Read())
        {
          yield return reader;
        }
      }
    }

    public static IEnumerable<T> ExecuteEnumerable<T>(this DbCommand command, CommandBehavior behavior,
      Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));
      Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

      using (var reader = command.ExecuteReader(behavior))
      {
        while (reader.Read())
        {
          yield return transform(reader);
        }
      }
    }

    public static IEnumerable<T> ExecuteEnumerable<T>(this DbCommand command, Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));
      Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

      using (var reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          yield return transform(reader);
        }
      }
    }

    public static T ExecuteFirst<T>(this DbCommand command, Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      return ExecuteFirst(command, null, transform);
    }

    public static T ExecuteFirst<T>(this DbCommand command, Action<IDataParameterBinder> bind,
      Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      if (bind != null)
      {
        bind(DataParameterBinders.GetBinderForDbCommand(command));
      }
      using (var reader = command.ExecuteReader())
      {
        if (!reader.Read())
        {
          throw new InvalidOperationException(Resources.Err_EmptyDbResult);
        }
        return transform(reader);
      }
    }

    public static T ExecuteFirstOrDefault<T>(this DbCommand command, Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      return ExecuteFirstOrDefault(command, null, transform);
    }

    public static T ExecuteFirstOrDefault<T>(this DbCommand command, Action<IDataParameterBinder> bind,
      Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      if (bind != null)
      {
        bind(DataParameterBinders.GetBinderForDbCommand(command));
      }
      using (var reader = command.ExecuteReader())
      {
        if (!reader.Read())
        {
          return default(T);
        }
        return transform(reader);
      }
    }

    public static T ExecuteSingle<T>(this DbCommand command, Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      return ExecuteSingle(command, null, transform);
    }

    public static T ExecuteSingle<T>(this DbCommand command, Action<IDataParameterBinder> bind,
      Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      if (bind != null)
      {
        bind(DataParameterBinders.GetBinderForDbCommand(command));
      }
      using (var reader = command.ExecuteReader())
      {
        if (!reader.Read())
        {
          throw new InvalidOperationException(Resources.Err_EmptyDbResult);
        }
        var res = transform(reader);
        if (reader.Read())
        {
          throw new InvalidOperationException(Resources.Err_DuplicateDbResult);
        }
        return res;
      }
    }

    public static T ExecuteSingleOrDefault<T>(this DbCommand command, Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      return ExecuteSingleOrDefault(command, null, transform);
    }

    public static T ExecuteSingleOrDefault<T>(this DbCommand command, Action<IDataParameterBinder> bind,
      Func<IDataRecord, T> transform)
    {
      Contract.Requires<ArgumentNullException>(command != null);
      Contract.Requires<ArgumentNullException>(transform != null);
      Contract.Requires<ArgumentNullException>(!String.IsNullOrEmpty(command.CommandText));
      Contract.Requires<InvalidOperationException>(command.Connection != null);
      Contract.Requires<InvalidOperationException>(command.Connection.State.HasFlag(ConnectionState.Open));

      if (bind != null)
      {
        bind(DataParameterBinders.GetBinderForDbCommand(command));
      }
      using (var reader = command.ExecuteReader())
      {
        if (!reader.Read())
        {
          return default(T);
        }
        var res = transform(reader);
        if (reader.Read())
        {
          throw new InvalidOperationException(Resources.Err_DuplicateDbResult);
        }
        return res;
      }
    }
  }
}