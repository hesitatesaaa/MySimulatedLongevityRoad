using Newtonsoft.Json.Utilities;
using UnityEngine;

public class AotTypeEnforcer : MonoBehaviour
{
	public void Awake()
	{
		AotHelper.EnsureType<CustomDataContainer<int>>();
		AotHelper.EnsureType<CustomDataContainer<float>>();
		AotHelper.EnsureType<CustomDataContainer<bool>>();
		AotHelper.EnsureType<CustomDataContainer<string>>();
		AotHelper.EnsureList<int>();
		AotHelper.EnsureList<float>();
		AotHelper.EnsureList<bool>();
		AotHelper.EnsureList<string>();
	}
}
