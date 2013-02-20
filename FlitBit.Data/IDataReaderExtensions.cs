#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion


using System;
using System.Data;
using System.Data.Common;
using System.Text;
using Newtonsoft.Json;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Contracts;

namespace FlitBit.Data
{
	public static class IDataReaderExtensions
	{
		public static string ToJson(this IDataReader reader)
		{
			return MultiResultToJArray(reader).ToString();
		}

		public static string ToJson(this IDataReader reader, string rootName)
		{
			return MultiResultToJObject(reader, rootName).ToString();
		}

		public static JArray MultiResultToJArray(this IDataReader reader)
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

		public static JArray ResultToJArray(this IDataReader reader)
		{
			var result = new JArray();
			while (reader.Read())
			{
				result.Add(reader.RecordAsJObject());
			}
			return result;
		}

		public static JObject MultiResultToJObject(this IDataReader reader, params string[] collectionNames)
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

		public static JObject ResultToJObject(this IDataReader reader, string collectioName)
		{
			Contract.Requires<ArgumentNullException>(reader != null);
			Contract.Requires<ArgumentNullException>(collectioName != null);
			Contract.Requires(collectioName.Length > 0);
			
			return new JObject(new JProperty(collectioName, ResultToJArray(reader)));			
		}
		
		public static XElement CurrentRowAsXElement(this IDataRecord record, string elementName)
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

		public static XElement CurrentRowAsXElement(this IDataRecord record, string elementName, bool columnsAsAttributes)
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
