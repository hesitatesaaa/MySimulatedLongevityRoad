using System;
using System.ComponentModel;

[Serializable]
public class WorldTimeScaleAsset : Asset, ILocalizedAsset
{
	public float multiplier;

	[DefaultValue(1)]
	public int ticks = 1;

	public int conway_ticks;

	public bool sonic;

	public bool render_skip;

	public string path_icon;

	public string locale_key;

	public string getLocaleID()
	{
		return locale_key;
	}

	public WorldTimeScaleAsset getNext(bool pCycle = false)
	{
		int num = AssetManager.time_scales.list.Count - 2;
		if (DebugConfig.debug_enabled)
		{
			num = AssetManager.time_scales.list.Count - 1;
		}
		int num2 = AssetManager.time_scales.list.IndexOf(this);
		if (++num2 > num)
		{
			if (!pCycle)
			{
				return this;
			}
			num2 = 0;
		}
		return AssetManager.time_scales.list[num2];
	}

	public WorldTimeScaleAsset getPrevious(bool pCycle = false)
	{
		int num = AssetManager.time_scales.list.IndexOf(this);
		if (--num < 0)
		{
			if (!pCycle)
			{
				return this;
			}
			num = AssetManager.time_scales.list.Count - 1;
		}
		return AssetManager.time_scales.list[num];
	}
}
