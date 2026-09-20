using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityPools;

[Serializable]
[JsonConverter(typeof(CustomDataContainerConverter))]
public class CustomDataContainer<TType> : IDisposable
{
	[NonSerialized]
	internal Dictionary<string, TType> dict = UnsafeCollectionPool<Dictionary<string, Dictionary<string, TType>>, KeyValuePair<string, Dictionary<string, TType>>>.Get();

	public TType this[string pKey]
	{
		get
		{
			return dict[pKey];
		}
		set
		{
			dict[pKey] = value;
		}
	}

	public IEnumerable<string> Keys => dict.Keys;

	public bool TryGetValue(string pKey, out TType pValue)
	{
		return dict.TryGetValue(pKey, out pValue);
	}

	public void Remove(string pKey)
	{
		dict.Remove(pKey);
	}

	public void Dispose()
	{
		if (dict != null)
		{
			dict.Clear();
			UnsafeCollectionPool<Dictionary<string, Dictionary<string, TType>>, KeyValuePair<string, Dictionary<string, TType>>>.Release((Dictionary<string, Dictionary<string, TType>>)(object)dict);
		}
		dict = null;
	}
}
