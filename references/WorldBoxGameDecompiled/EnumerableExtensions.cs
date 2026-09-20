using System.Collections.Generic;

public static class EnumerableExtensions
{
	public static T GetRandom<T>(this IEnumerable<T> pEnumerable)
	{
		if (!(pEnumerable is List<T> list))
		{
			if (!(pEnumerable is ListPool<T> list2))
			{
				if (!(pEnumerable is T[] pArray))
				{
					if (pEnumerable is HashSet<T> pHashSet)
					{
						return pHashSet.GetRandom();
					}
					using ListPool<T> list3 = new ListPool<T>(pEnumerable);
					return list3.GetRandom();
				}
				return pArray.GetRandom();
			}
			return list2.GetRandom();
		}
		return list.GetRandom();
	}
}
