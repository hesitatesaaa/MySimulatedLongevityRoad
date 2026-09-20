using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class LongJsonConverter : JsonConverter
{
	internal static long next_long = 100000000L;

	internal static Dictionary<string, long> longs = new Dictionary<string, long>();

	public override bool CanWrite => false;

	public override bool CanRead => true;

	public static void reset()
	{
		next_long = 100000000L;
		longs.Clear();
	}

	public static long getLong(string pString, JsonReader pReader)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return -1L;
		}
		string s = pString;
		if (pString.IndexOf('_') > 0)
		{
			string[] array = pString.Split('_');
			if (array.Length == 2)
			{
				string pValue = array[0] + "_";
				if (MapStats.possible_formats.IndexOf(pValue) > -1)
				{
					s = array[1];
				}
			}
		}
		if (long.TryParse(s, out var result))
		{
			return result;
		}
		bool flag = pString.Length == 8 || (pString.Length == 36 && pString[8] == '-' && pString[13] == '-' && pString[18] == '-' && pString[23] == '-');
		if (!longs.TryGetValue(pString, out var value))
		{
			value = next_long++;
			longs[pString] = value;
			if (!flag)
			{
				Debug.LogWarning((object)(pReader.Path + " Failed to parse long <b>" + pString + "</b> " + pString.Length + " -> " + value));
			}
		}
		else if (!flag)
		{
			Debug.LogWarning((object)(pReader.Path + " Failed to parse long <b>" + pString + "</b> " + pString.Length + " -> " + value + " already had it"));
		}
		return value;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected I4, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		JsonToken tokenType = reader.TokenType;
		switch (tokenType - 7)
		{
		case 4:
			return -1L;
		case 0:
			return Convert.ToInt64(reader.Value);
		case 2:
			return getLong((string)reader.Value, reader);
		default:
			Debug.LogWarning((object)("Unhandled type " + reader.Path + " " + reader.Value?.ToString() + " " + ((object)reader.TokenType/*cast due to constrained. prefix*/).ToString() + " -> " + -1L));
			return -1L;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(value);
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(long);
	}
}
