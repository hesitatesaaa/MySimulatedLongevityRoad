using System;
using System.Collections.Generic;

public static class OnomasticsCache
{
	[ThreadStatic]
	private static Dictionary<string, OnomasticsData> _cache;

	public static OnomasticsData getOriginalData(string pShortTemplate)
	{
		if (_cache == null)
		{
			_cache = new Dictionary<string, OnomasticsData>();
		}
		if (!_cache.TryGetValue(pShortTemplate, out var value))
		{
			value = new OnomasticsData();
			value.loadFromShortTemplate(pShortTemplate);
			_cache.Add(pShortTemplate, value);
		}
		return value;
	}
}
