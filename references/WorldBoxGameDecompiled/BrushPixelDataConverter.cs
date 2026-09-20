using System;
using Newtonsoft.Json;

public class BrushPixelDataConverter : JsonConverter
{
	public override void WriteJson(JsonWriter pWriter, object pValue, JsonSerializer pSerializer)
	{
		BrushPixelData brushPixelData = (BrushPixelData)pValue;
		string text = brushPixelData.x + "," + brushPixelData.y + "," + brushPixelData.dist;
		pSerializer.Serialize(pWriter, (object)text, typeof(string));
	}

	public override object ReadJson(JsonReader pReader, Type pObjectType, object pExistingValue, JsonSerializer pSerializer)
	{
		string text = pSerializer.Deserialize<string>(pReader);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		int[] array = Array.ConvertAll(text.Split(','), int.Parse);
		return new BrushPixelData(array[0], array[1], array[2]);
	}

	public override bool CanConvert(Type pObjectType)
	{
		if (pObjectType != null)
		{
			return pObjectType == typeof(BrushPixelData);
		}
		return false;
	}
}
