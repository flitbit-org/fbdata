using System;
using System.Diagnostics.Contracts;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data
{
	/// <summary>
	/// Data model utilities.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public static class DataModel<TModel>
	{
		static Lazy<Type> LazyConcreteType = new Lazy<Type>(
			() =>
			{
				if (typeof(TModel).IsAbstract)
				{
					var res = FactoryProvider.Factory.GetImplementationType<TModel>();
					if (res == null)
					{
						throw new NotImplementedException(String.Concat("Concrete implementation not found: `",
																														typeof(TModel).GetReadableFullName(), "`. Are you missing a wireup?"));
					}
					return res;
				}
				return typeof(TModel);
			},
			LazyThreadSafetyMode.ExecutionAndPublication);

		static Lazy<HierarchyMapping<TModel>> __hier = new Lazy<HierarchyMapping<TModel>>(LazyThreadSafetyMode.ExecutionAndPublication);

		static Lazy<IdentityKey<TModel>> __identityKey = new Lazy<IdentityKey<TModel>>(
			() => FactoryProvider.Factory.CreateInstance<IdentityKey<TModel>>(),
			LazyThreadSafetyMode.ExecutionAndPublication
			);

		static Lazy<Mapping<TModel>> __mapping = new Lazy<Mapping<TModel>>(() => Mappings.Instance.ForType<TModel>(),
																														LazyThreadSafetyMode.ExecutionAndPublication);
		
		public static Type ConcreteType { get { return LazyConcreteType.Value; } }

		public static HierarchyMapping<TModel> Hierarchy { get { return __hier.Value; } }

		public static IdentityKey<TModel> IdentityKey
		{
			get
			{
				Contract.Ensures(Contract.Result<IdentityKey<TModel>>() != null);
				return __identityKey.Value;
			}
		}

		public static Mapping<TModel> Mapping { get { return __mapping.Value; } }
		
		internal static TModel ResolveIdentityKey<TIdentityKey>(TIdentityKey id)
		{
			if (typeof(TIdentityKey) == IdentityKey.KeyType && Repository<TIdentityKey>.HasRepository(Mapping))
			{}
			return default(TModel);
		}

		internal static class Repository<TIdentityKey>
		{
			public static bool HasRepository(Mapping<TModel> mapping)
			{
				return (!String.IsNullOrEmpty(mapping.ConnectionName))
					&& DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName) != null;
			}
		}


	}
}