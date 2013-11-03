using System.Configuration;

namespace FlitBit.Data.Configuration
{
	/// <summary>
	/// Configuration section for DataModels.
	/// </summary>
	public class DataModelConfigSection : ConfigurationSection
	{
		internal const string SectionName = "flitbit.datamodel";
		const string PropertyNameMapConnectionStrings = "map-connection-strings";

		/// <summary>
		///   Gets the collection of mapped connection strings.
		/// </summary>
		[ConfigurationProperty(PropertyNameMapConnectionStrings, IsDefaultCollection = true)]
		public MapConnectionStringsConfigurationElementCollection MapConnectionStrings { get { return (MapConnectionStringsConfigurationElementCollection)this[PropertyNameMapConnectionStrings]; } }

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
