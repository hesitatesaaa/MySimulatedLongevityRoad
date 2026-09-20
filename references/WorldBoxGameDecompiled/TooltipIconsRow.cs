using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipIconsRow : TooltipItemsRow<Image>
{
	private List<(Sprite, Color)> _icons = new List<(Sprite, Color)>();

	protected override void loadItems()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		items_pool.clear();
		if (_icons.Count == 0)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		foreach (var icon in _icons)
		{
			Image next = items_pool.getNext();
			next.sprite = icon.Item1;
			((Graphic)next).color = icon.Item2;
		}
		clearIcons();
	}

	public void addIcon(Sprite pIcon, string pColor = "#FFFFFF")
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Color item = Toolbox.makeColor(pColor);
		_icons.Add((pIcon, item));
	}

	private void clearIcons()
	{
		_icons.Clear();
	}
}
