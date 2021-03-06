﻿#region COPYRIGHT© 2009-2013 Phillip Clark.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Data;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace FlitBit.Data
{
	public static class IDataRecordExtensions
	{
		static readonly int MaxDataColumns = 128;

		public static XElement CurrentRowAsXmlElement(this IDataRecord record, string elementName)
		{
			var data = new object[MaxDataColumns];
			var columns = record.GetValues(data);

			var result = new XElement(elementName);
			for (var j = 0; j < columns; j++)
			{
				result.Add(new XElement(record.GetName(j), data[j]));
			}
			return result;
		}

		public static XElement CurrentRowAsXmlElement(this IDataRecord record, string elementName, bool columnsAsAttributes)
		{
			var data = new object[MaxDataColumns];
			var columns = record.GetValues(data);

			var result = new XElement(elementName);
			if (columnsAsAttributes)
			{
				for (var j = 0; j < columns; j++)
				{
					result.Add(new XAttribute(record.GetName(j), data[j]));
				}
			}
			else
			{
				for (var j = 0; j < columns; j++)
				{
					result.Add(new XElement(record.GetName(j), data[j]));
				}
			}
			return result;
		}

		public static T GetValueOrDefault<T>(this IDataRecord record, int ordinal)
		{
			return (T) ((record.IsDBNull(ordinal)) ? default(T) : record.GetValue(ordinal));
		}

		public static T GetValueOrDefault<T>(this IDataRecord record, int ordinal, T defa)
		{
			return (T) ((record.IsDBNull(ordinal)) ? defa : record.GetValue(ordinal));
		}

		public static JArray RecordAsJArray(this IDataRecord record)
		{
			var data = new object[record.FieldCount];
			var columns = record.GetValues(data);

			var result = new JArray();
			for (var j = 0; j < columns; j++)
			{
				result.Add(new JObject(
										new JProperty("Name", record.GetName(j)),
										new JProperty("Value", data[j])));
			}
			return result;
		}

		public static JObject RecordAsJObject(this IDataRecord record)
		{
			var data = new object[record.FieldCount];
			var columns = record.GetValues(data);

			var result = new JObject();
			for (var j = 0; j < columns; j++)
			{
				result.Add(new JProperty(record.GetName(j), data[j]));
			}
			return result;
		}

		public static string RecordAsJson(this IDataRecord record)
		{
			return RecordAsJObject(record)
				.ToString();
		}
	}
}