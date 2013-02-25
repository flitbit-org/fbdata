#region COPYRIGHT© 2009-2013 Phillip Clark.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace FlitBit.Data
{
	public static class DbDataReaderExtensions
	{
		public static string ToJson(this DbDataReader reader)
		{
			return MultiResultToJArray(reader).ToString();
		}

		public static string ToJson(this DbDataReader reader, string rootName)
		{
			return MultiResultToJObject(reader, rootName).ToString();
		}

		public static JArray MultiResultToJArray(this DbDataReader reader)
		{
			var result = new JArray();
			while (true)
			{
				while (reader.Read())
				{
					result.Add(reader.RecordAsJObject());
				}
				if (!reader.NextResult())
					break;
			}
			return result;
		}

		public static JArray ResultToJArray(this DbDataReader reader)
		{
			var result = new JArray();
			while (reader.Read())
			{
				result.Add(reader.RecordAsJObject());
			}
			return result;
		}

		public static JObject MultiResultToJObject(this DbDataReader reader, params string[] collectionNames)
		{
			Contract.Requires<ArgumentNullException>(reader != null);

			var result = new JObject();
			int res = 0;
			while (true)
			{
				var name = (collectionNames != null && collectionNames.Length > res)
					? collectionNames[res++]
					: String.Concat("unamed_collection_", res++);

				result.Add(new JProperty(name, ResultToJArray(reader)));

				if (!reader.NextResult())
					break;
			}
			return result;
		}

		public static JObject ResultToJObject(this DbDataReader reader, string collectioName)
		{
			Contract.Requires<ArgumentNullException>(reader != null);
			Contract.Requires<ArgumentNullException>(collectioName != null);
			Contract.Requires(collectioName.Length > 0);
			
			return new JObject(new JProperty(collectioName, ResultToJArray(reader)));			
		}
		
		public static XElement CurrentRowAsXElement(this DbDataRecord record, string elementName)
		{
			var data = new object[record.FieldCount];
			var columns = record.GetValues(data);

			var result = new XElement(elementName);
			for (int j = 0; j < columns; j++)
			{
				result.Add(new XElement(record.GetName(j), data[j]));
			}
			return result;
		}

		public static XElement CurrentRowAsXElement(this DbDataRecord record, string elementName, bool columnsAsAttributes)
		{
			var data = new object[record.FieldCount];
			var columns = record.GetValues(data);

			var result = new XElement(elementName);
			if (columnsAsAttributes)
			{
				for (int j = 0; j < columns; j++)
				{
					result.Add(new XAttribute(record.GetName(j), data[j]));
				}
			}
			else
			{
				for (int j = 0; j < columns; j++)
				{
					result.Add(new XElement(record.GetName(j), data[j]));
				}
			}
			return result;
		}
	}
}
