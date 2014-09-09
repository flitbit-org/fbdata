#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Configuration;

namespace FlitBit.Data.Configuration
{
    /// <summary>
    ///     Configuration section for DataModels.
    /// </summary>
    public class DataModelConfigSection : ConfigurationSection
    {
        internal const string SectionName = "flitbit.datamodel";
        const string PropertyNameMapConnectionStrings = "map-connection-strings";

        /// <summary>
        ///     Gets the collection of mapped connection strings.
        /// </summary>
        [ConfigurationProperty(PropertyNameMapConnectionStrings, IsDefaultCollection = true)]
        public MapConnectionStringsConfigurationElementCollection MapConnectionStrings
        {
            get { return (MapConnectionStringsConfigurationElementCollection)this[PropertyNameMapConnectionStrings]; }
        }

        internal static DataModelConfigSection Instance
        {
            get
            {
                var config = ConfigurationManager.GetSection(SectionName)
                             as DataModelConfigSection;
                return config ?? new DataModelConfigSection();
            }
        }
    }
}