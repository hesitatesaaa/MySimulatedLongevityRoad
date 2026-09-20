using System;
using Newtonsoft.Json;
using UnityEngine;

public class NullableLongJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanRead => true;

	public static long? getLong(string pString, JsonReader pReader)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return null;
		}
		return LongJsonConverter.getLong(pString, pReader);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected I4, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		switch (tokenType - 7)
		{
		case 4:
			return null;
		case 0:
			return Convert.ToInt64(reader.Value);
		case 2:
			return getLong((string)reader.Value, reader);
		default:
			Debug.LogWarning((object)("Unhandled type " + reader.Path + " " + reader.Value?.ToString() + " " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString() + " -> null"));
			return null;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(value);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(long?);
	}
}
