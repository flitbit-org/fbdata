using System;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.Meta;
using FlitBit.ObjectIdentity;

namespace FlitBit.Data
{
	public static class DataModel<M>
	{
		static Lazy<Type> __concreteType = new Lazy<Type>(
			() => 
			{
				if (typeof(M).IsAbstract) {
					var res = FactoryProvider.Factory.GetImplementationType<M>();
					if (res == null)
					{
						throw new NotImplementedException(String.Concat("Concrete implementation not found: `", typeof(M).GetReadableFullName(), "`. Are you missing a wireup?"));
					}
					return res;
				}
				return typeof(M);
			}, 
			LazyThreadSafetyMode.ExecutionAndPublication);
		static Lazy<Mapping<M>> __mapping = new Lazy<Mapping<M>>(() => Mappings.Instance.ForType<M>(), LazyThreadSafetyMode.ExecutionAndPublication);
		static Lazy<HierarchyMapping<M>> __hier = new Lazy<HierarchyMapping<M>>(LazyThreadSafetyMode.ExecutionAndPublication);
		static Lazy<IDataModelReferenceFactory<M>> __referenceFactory = new Lazy<IDataModelReferenceFactory<M>>(
			() => FactoryProvider.Factory.CreateInstance<IDataModelReferenceFactory<M>>(),
			LazyThreadSafetyMode.ExecutionAndPublication
			);
		static Lazy<IdentityKey<M>> __identityKey = new Lazy<IdentityKey<M>>(
			() => FactoryProvider.Factory.CreateInstance<IdentityKey<M>>(),
			LazyThreadSafetyMode.ExecutionAndPublication
			);

		public static Type ConcreteType { get { return __concreteType.Value; } }

		public static Mapping<M> Mapping { get { return __mapping.Value; } }

		public static HierarchyMapping<M> Hierarchy { get { return __hier.Value; } }

		public static IdentityKey<M> IdentityKey { get { return __identityKey.Value; } }

		public static IDataModelReferenceFactory<M> ReferenceFactory { get { return __referenceFactory.Value; } }

		internal static M ResolveIdentityKey<IK>(IK id)
		{
			if (typeof(IK) == IdentityKey.KeyType && Repository<IK>.HasRepository(Mapping))
			{

			}
			return default(M);
		}

		internal static class Repository<IK>
		{
			public static bool HasRepository(Mapping<M> mapping)
			{
				return (!String.IsNullOrEmpty(mapping.ConnectionName))
					&& DbProviderHelpers.GetDbProviderHelperForDbConnection(mapping.ConnectionName) != null;
			}
		}
	}
}
