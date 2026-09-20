using System;
using UnityEngine;

[Serializable]
public class BaseCategoryAsset : Asset, ILocalizedAsset
{
	public string name;

	public string color;

	public bool show_counter = true;

	[NonSerialized]
	public Color? _color;

	public virtual string getLocaleID()
	{
		return name;
	}

	public Color getColor()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!_color.HasValue)
		{
			_color = Toolbox.makeColor(color);
		}
		return _color.Value;
	}
}
