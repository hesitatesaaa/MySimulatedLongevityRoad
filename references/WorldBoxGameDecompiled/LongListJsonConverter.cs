using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class LongListJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanRead => true;

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		if ((int)reader.TokenType == 11)
		{
			return null;
		}
		if ((int)reader.TokenType == 2)
		{
			using ListPool<long> listPool = new ListPool<long>();
			while (reader.Read())
			{
				JsonToken tokenType = reader.TokenType;
				switch (tokenType - 7)
				{
				case 0:
					listPool.Add(Convert.ToInt64(reader.Value));
					continue;
				case 4:
					listPool.Add(-1L);
					continue;
				case 2:
				{
					string pString = (string)reader.Value;
					listPool.Add(LongJsonConverter.getLong(pString, reader));
					continue;
				}
				case 1:
				case 3:
					continue;
				}
				if ((int)tokenType != 14)
				{
					continue;
				}
				return new List<long>(listPool);
			}
		}
		Debug.LogWarning((object)("Unhandled type " + reader.Path + " " + reader.Value?.ToString() + " " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString() + " -> null"));
		return null;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(value);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(List<long>);
	}
}
