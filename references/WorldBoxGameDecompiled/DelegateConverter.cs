using System;
using Newtonsoft.Json;

public class DelegateConverter : JsonConverter
{
	public override void WriteJson(JsonWriter pWriter, object pValue, JsonSerializer pSerializer)
	{
		if (pValue != null)
		{
			Delegate[] invocationList = ((Delegate)pValue).GetInvocationList();
			string[] array = new string[invocationList.Length];
			for (int i = 0; i < invocationList.Length; i++)
			{
				array[i] = invocationList[i].Method.DeclaringType?.ToString() + "." + invocationList[i].Method.Name;
			}
			pSerializer.Serialize(pWriter, (object)array, typeof(string[]));
		}
	}

	public override object ReadJson(JsonReader pReader, Type pObjectType, object pExistingValue, JsonSerializer pSerializer)
	{
		return null;
	}

	public override bool CanConvert(Type pObjectType)
	{
		if (pObjectType != null)
		{
			if (!(pObjectType == typeof(Delegate)))
			{
				return pObjectType.IsSubclassOf(typeof(Delegate));
			}
			return true;
		}
		return false;
	}
}
