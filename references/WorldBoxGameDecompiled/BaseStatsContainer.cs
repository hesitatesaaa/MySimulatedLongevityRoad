using Newtonsoft.Json;
using UnityEngine;

public class BaseStatsContainer
{
	public string id;

	public float value;

	[JsonIgnore]
	public BaseStatAsset asset => AssetManager.base_stats_library.get(id);

	public void normalize()
	{
		BaseStatAsset baseStatAsset = asset;
		if (baseStatAsset.normalize)
		{
			value = Mathf.Clamp(value, baseStatAsset.normalize_min, baseStatAsset.normalize_max);
		}
	}
}
