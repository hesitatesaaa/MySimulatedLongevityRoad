using System.Collections.Generic;
using RSG;
using UnityEngine;

internal class WorldLoader : MonoBehaviour
{
	public static WorldLoader instance;

	public static Dictionary<string, Map> mapCache = new Dictionary<string, Map>();

	public static Dictionary<string, Promise<Dictionary<string, Map>>> listCache = new Dictionary<string, Promise<Dictionary<string, Map>>>();
}
