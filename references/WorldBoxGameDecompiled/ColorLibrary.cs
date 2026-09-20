using System.Collections.Generic;

public class ColorLibrary : AssetLibrary<ColorAsset>
{
	private readonly List<ColorAsset> _free_colors_main = new List<ColorAsset>();

	private readonly List<ColorAsset> _free_colors_bonus = new List<ColorAsset>();

	private readonly List<ColorAsset> _free_colors_preferred = new List<ColorAsset>();

	internal bool must_be_global;

	public override void post_init()
	{
		base.post_init();
		foreach (ColorAsset item in list)
		{
			item.initColor();
		}
	}

	public ColorAsset getColorByIndex(int pIndex)
	{
		if (pIndex >= list.Count)
		{
			return list[0];
		}
		return list[pIndex];
	}

	public ColorAsset getNextColor(ActorAsset pActorAsset)
	{
		_free_colors_bonus.Clear();
		_free_colors_main.Clear();
		_free_colors_preferred.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			ColorAsset colorAsset = list[i];
			if (!isColorUsedInWorld(colorAsset))
			{
				if (pActorAsset != null && pActorAsset.preferred_colors != null && pActorAsset.preferred_colors.Contains(colorAsset.id))
				{
					_free_colors_preferred.Add(colorAsset);
				}
				if (colorAsset.favorite)
				{
					_free_colors_main.Add(colorAsset);
				}
				else
				{
					_free_colors_bonus.Add(colorAsset);
				}
			}
		}
		if (_free_colors_preferred.Count > 0)
		{
			return _free_colors_preferred.GetRandom();
		}
		if (_free_colors_main.Count > 0)
		{
			return _free_colors_main.GetRandom();
		}
		if (_free_colors_bonus.Count > 0)
		{
			return _free_colors_bonus.GetRandom();
		}
		return list.GetRandom();
	}

	public int getNextColorIndex(ActorAsset pActorAsset)
	{
		ColorAsset nextColor = getNextColor(pActorAsset);
		return list.IndexOf(nextColor);
	}

	public virtual bool isColorUsedInWorld(ColorAsset pAsset)
	{
		return false;
	}

	protected bool checkColor(ColorAsset pAsset, int pColorIndex)
	{
		if (pColorIndex == pAsset.index_id)
		{
			return true;
		}
		return false;
	}

	public override ColorAsset add(ColorAsset pAsset)
	{
		ColorAsset colorAsset = base.add(pAsset);
		ColorAsset.saveToGlobalList(colorAsset, must_be_global);
		return colorAsset;
	}

	public void useSameColorsFrom(ColorLibrary pSource)
	{
		list = pSource.list;
		dict = pSource.dict;
	}
}
