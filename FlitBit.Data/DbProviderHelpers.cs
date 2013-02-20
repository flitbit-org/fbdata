#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Data;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;
using FlitBit.Registrar;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Data
{
	
	public static class DbProviderHelpers
	{
		readonly static Lazy<Registrar<string, Type>> __registry = new Lazy<Registrar<string, Type>>(
			LazyThreadSafetyMode.ExecutionAndPublication);

		public static DbProviderHelper GetDbProviderHelperForDbConnection(IDbConnection connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);

			var key = connection.GetType().AssemblyQualifiedName;
			var reg = __registry.Value;
			IRegistrationKey<string, Type> registration;
			if (reg.TryGetRegistration(key, out registration))
			{
				return (DbProviderHelper)Activator.CreateInstance(registration.Handback);
			}
			return default(DbProviderHelper);
		}

		public static DbProviderHelper GetDbProviderHelperForDbConnection(string connection)
		{
			Contract.Requires<ArgumentNullException>(connection != null);
			Contract.Requires<ArgumentException>(connection.Length > 0);

			var provider = DbExtensions.GetProviderName(connection);
			if (provider != null)
			{
				var reg = __registry.Value;
				IRegistrationKey<string, Type> registration;
				if (reg.TryGetRegistration(provider, out registration))
				{
					return (DbProviderHelper)Activator.CreateInstance(registration.Handback);
				}
			}
			return default(DbProviderHelper);
		}
	}
}
