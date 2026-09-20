using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class OrderedContractResolver : DefaultContractResolver
{
	protected override IList<JsonProperty> CreateProperties(Type pObjectType, MemberSerialization pMemberSerialization)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		List<JsonProperty> list = new List<JsonProperty>(((DefaultContractResolver)this).CreateProperties(pObjectType, pMemberSerialization));
		list.Sort(orderedPropertySorter);
		return list;
	}

	private int orderedPropertySorter(JsonProperty p1, JsonProperty p2)
	{
		if (p1.Order != p2.Order)
		{
			int num = p1.Order ?? int.MaxValue;
			int value = p2.Order ?? int.MaxValue;
			return num.CompareTo(value);
		}
		bool flag = isDelegate(p1.PropertyType);
		bool flag2 = isDelegate(p2.PropertyType);
		if (flag != flag2)
		{
			return flag.CompareTo(flag2);
		}
		bool flag3 = isCollection(p1.PropertyType);
		bool flag4 = isCollection(p2.PropertyType);
		if (flag3 != flag4)
		{
			return flag3.CompareTo(flag4);
		}
		return p1.PropertyName.CompareTo(p2.PropertyName);
	}

	private int getBaseTypesCount(Type pType)
	{
		int num = 0;
		while (pType != null)
		{
			num++;
			pType = pType.BaseType;
		}
		return num;
	}

	private bool isDelegate(Type pType)
	{
		if (!(pType == typeof(Delegate)))
		{
			return pType.IsSubclassOf(typeof(Delegate));
		}
		return true;
	}

	private bool isCollection(Type pType)
	{
		return typeof(ICollection).IsAssignableFrom(pType);
	}
}
