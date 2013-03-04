#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Data.Meta
{
	/// <summary>
	/// Associates a database schema name with an assembly. Any entity 
	/// class declared in the same assembly will use the schema name given
	/// unless it declares its own SchemaTarget.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
	public sealed class MapSchemaAttribute: Attribute
	{
		/// <summary>
		/// Associates a database schema name with an assembly. Any entity 
		/// class declared in the same assembly will use the schema name given
		/// unless it declares its own SchemaTarget.
		/// </summary>
		public MapSchemaAttribute(string schema)
		{
			Contract.Requires(schema != null);
			Contract.Requires(schema.Length > 0);

			this.Schema = schema;
		}
		public string Schema { get; private set; }

		internal void PrepareMapping<T>(Mapping<T> mapping)
		{
			if (!String.IsNullOrEmpty(Schema))
			{
				mapping.InSchema(Schema);
			}
		}

	}
}
