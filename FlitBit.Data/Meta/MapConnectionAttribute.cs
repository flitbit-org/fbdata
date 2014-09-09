#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Data.Meta
{
    /// <summary>
    ///     Associates a database connection name with an assembly. Any entity
    ///     class declared in the same assembly will use the connection name given
    ///     unless it declares its own ConnectionName.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
    public sealed class MapConnectionAttribute : Attribute
    {
        /// <summary>
        ///     Associates a database connection name with an assembly. Any entity
        ///     class declared in the same assembly will use the connection name given
        ///     unless it declares its own ConnectionName.
        /// </summary>
        public MapConnectionAttribute(string connectionName)
        {
            Contract.Requires<ArgumentNullException>(connectionName != null);
            Contract.Requires<ArgumentException>(connectionName.Length > 0);

            this.ConnectionName = connectionName;
        }

        /// <summary>
        ///     The default connection name used for DataModels and Entities defined within the scope.
        /// </summary>
        public string ConnectionName { get; private set; }

        internal void PrepareMapping<T>(Mapping<T> mapping)
        {
            if (!String.IsNullOrEmpty(ConnectionName))
            {
                mapping.UsesConnection(ConnectionName);
            }
        }
    }
}