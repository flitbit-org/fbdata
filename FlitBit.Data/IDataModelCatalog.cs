using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.Data.Meta;

namespace FlitBit.Data
{
	public interface IDataModelCatalog
	{
		void Register<TModel, Id>(Mapping<TModel> mapping, IModelBinder<TModel, Id> binder);
	}
}
