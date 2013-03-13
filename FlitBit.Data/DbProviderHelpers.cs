#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using FlitBit.Registrar;

namespace FlitBit.Data
{
	public static class DbProviderHelpers
	{
		static readonly Registrar<string, Type> __registry = new Registrar<string, Type>();

		public static DbProviderHelper GetDbProviderHelperForDbConnection(DbConnection connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);

			var key = KeyFor(connection);
			IRegistrationKey<string, Type> registration;
			if (__registry.TryGetRegistration(key, out registration))
			{
				return (DbProviderHelper) Activator.CreateInstance(registration.Handback);
			}
			return default(DbProviderHelper);
		}

		public static DbProviderHelper GetDbProviderHelperForDbConnection(string connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);

			var pr = DbExtensions.GetProviderByConnectionName(connection);
			var key = KeyFor(pr);
			IRegistrationKey<string, Type> registration;
			if (__registry.TryGetRegistration(key, out registration))
			{
				return (DbProviderHelper) Activator.CreateInstance(registration.Handback);
			}
			return default(DbProviderHelper);
		}

		public static void RegisterHelper<Pr, Cn, Cm, H>()
			where Pr : DbProviderFactory
			where Cn : DbConnection
			where Cm : DbCommand
			where H : DbProviderHelper
		{
			IRegistrationKey<string, Type> registration;
			__registry.TryRegister(KeyFor<Pr>(), typeof(H), out registration);
			__registry.TryRegister(KeyFor<Cn>(), typeof(H), out registration);
			__registry.TryRegister(KeyFor<Cm>(), typeof(H), out registration);
		}

		static string KeyFor<T>() { return typeof(T).AssemblyQualifiedName; }

		static string KeyFor(object instance) { return instance.GetType().AssemblyQualifiedName; }
	}
}