using System;
using System.Reflection;
using Newtonsoft.Json;

public class CustomDataContainerConverter : JsonConverter
{
	public override void WriteJson(JsonWriter pWriter, object pValue, JsonSerializer pSerializer)
	{
		FieldInfo? field = pValue.GetType().GetField("dict", BindingFlags.Instance | BindingFlags.NonPublic);
		Type fieldType = field.FieldType;
		object value = field.GetValue(pValue);
		pSerializer.Serialize(pWriter, value, fieldType);
	}

	public override object ReadJson(JsonReader pReader, Type pObjectType, object pExistingValue, JsonSerializer pSerializer)
	{
		object obj = Activator.CreateInstance(pObjectType);
		FieldInfo? field = pObjectType.GetField("dict", BindingFlags.Instance | BindingFlags.NonPublic);
		Type fieldType = field.FieldType;
		field.SetValue(obj, pSerializer.Deserialize(pReader, fieldType));
		return obj;
	}

	public override bool CanConvert(Type pObjectType)
	{
		return false;
	}
}
