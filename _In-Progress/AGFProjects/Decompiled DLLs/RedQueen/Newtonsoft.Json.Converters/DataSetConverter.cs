using System;
using System.Data;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class DataSetConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		DataSet obj = (DataSet)value;
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		DataTableConverter dataTableConverter = new DataTableConverter();
		writer.WriteStartObject();
		foreach (DataTable table in obj.Tables)
		{
			writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName(table.TableName) : table.TableName);
			dataTableConverter.WriteJson(writer, table, serializer);
		}
		writer.WriteEndObject();
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public override object ReadJson(JsonReader reader, Type objectType, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		DataSet dataSet = ((objectType == typeof(DataSet)) ? new DataSet() : ((DataSet)Activator.CreateInstance(objectType)));
		DataTableConverter dataTableConverter = new DataTableConverter();
		reader.ReadAndAssert();
		while (reader.TokenType == JsonToken.PropertyName)
		{
			DataTable dataTable = dataSet.Tables[(string)reader.Value];
			bool num = dataTable != null;
			dataTable = (DataTable)dataTableConverter.ReadJson(reader, typeof(DataTable), dataTable, serializer);
			if (!num)
			{
				dataSet.Tables.Add(dataTable);
			}
			reader.ReadAndAssert();
		}
		return dataSet;
	}

	public override bool CanConvert(Type valueType)
	{
		return typeof(DataSet).IsAssignableFrom(valueType);
	}
}
