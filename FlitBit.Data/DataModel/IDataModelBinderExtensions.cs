#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using FlitBit.Data.Expressions;

namespace FlitBit.Data.DataModel
{
    public static class IDataModelBinderExtensions
    {
        public static void BuildDdlBatch(this IDataModelBinder binder, SqlWriter batch)
        {
            var members = new List<Type>();
            binder.BuildDdlBatch(batch, members);
        }
    }
}