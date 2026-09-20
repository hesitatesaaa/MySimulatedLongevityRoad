using System;
using System.Collections.Generic;
using RSG;

[Serializable]
public class Favorites
{
	public static Dictionary<string, bool> favorites = new Dictionary<string, bool>();

	public static Promise promise;
}
