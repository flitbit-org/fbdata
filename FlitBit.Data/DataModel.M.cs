using System;
using System.Threading;
using FlitBit.Core;
using FlitBit.Data.Meta;

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

		public static Type ConcreteType { get { return __concreteType.Value; } }

		public static Mapping<M> Mapping { get { return __mapping.Value; } }
	}
}
