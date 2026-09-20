using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldLogAsset : Asset, IMultiLocalesAsset
{
	public string group;

	public string locale_id;

	public string path_icon;

	public Color color;

	public int random_ids;

	public WorldLogTextFormatter text_replacer;

	public string getLocaleID()
	{
		return locale_id ?? id;
	}

	public string getLocaleID(int pIndex)
	{
		return $"{getLocaleID()}_{pIndex}";
	}

	public IEnumerable<string> getLocaleIDs()
	{
		if (random_ids == 0)
		{
			yield return getLocaleID();
			yield break;
		}
		for (int i = 1; i <= random_ids; i++)
		{
			yield return getLocaleID(i);
		}
	}

	public WorldLogAsset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		color = Toolbox.color_log_neutral;
		base._002Ector();
	}
}
