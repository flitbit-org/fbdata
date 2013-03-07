using System;
using FlitBit.Data.Meta;
using FlitBit.Core;

namespace FlitBit.Data.SqlServer
{
	public class SqlModelBinderFactory: IModelBinderFactory
	{
		public IModelBinder<TModel, Id> GetModelBinder<TModel, Id>(IDataModelCatalog catalog, MappingStrategy scheme)
		{
			// Currently only support the default hybrid scheme...
			if (scheme != MappingStrategy.DynamicHybridInheritanceTree)
				throw new NotImplementedException();

			var concreteType = typeof(TModel);
			if (concreteType.IsAbstract && FactoryProvider.Factory.CanConstruct<TModel>())
			{
				concreteType = FactoryProvider.Factory.GetImplementationType<TModel>();
			}
			var binderType = typeof(DynamicHybridInheritanceTreeBinder<,,>).MakeGenericType(typeof(TModel), typeof(Id), concreteType);
			return (IModelBinder<TModel, Id>)Activator.CreateInstance(binderType, catalog);
		}
	}
}
