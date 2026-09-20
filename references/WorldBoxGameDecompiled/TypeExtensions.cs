using System;
using System.Collections.Generic;
using System.Reflection;

public static class TypeExtensions
{
	public static bool hasField(this Type pStaticType, string pFieldName)
	{
		if (pStaticType.GetField(pFieldName, BindingFlags.Static | BindingFlags.Public) != null)
		{
			return true;
		}
		return false;
	}

	public static IEnumerable<string> getFields(this Type pStaticType)
	{
		FieldInfo[] fields = pStaticType.GetFields(BindingFlags.Static | BindingFlags.Public);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			yield return fieldInfo.Name;
		}
	}
}
