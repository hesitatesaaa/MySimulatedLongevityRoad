using System.Collections.Generic;

public static class List
{
	public static List<T> Of<T>(params T[] pArgs)
	{
		return new List<T>(pArgs);
	}
}
