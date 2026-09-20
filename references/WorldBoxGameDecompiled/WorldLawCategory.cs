using System.Collections.Generic;
using LayoutGroupExt;
using UnityEngine;
using UnityEngine.UI;

public class WorldLawCategory : MonoBehaviour
{
	[SerializeField]
	private Text _title;

	[SerializeField]
	private Text _selected_counter;

	public GridLayoutGroupExtended grid;

	private WorldLawGroupAsset _asset;

	private HashSet<WorldLawElement> _laws_list = new HashSet<WorldLawElement>();

	public void init(WorldLawGroupAsset pGroupAsset)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_asset = pGroupAsset;
		((Graphic)_title).color = _asset.getColor();
		((Component)_title).GetComponent<LocalizedText>().setKeyAndUpdate(_asset.getLocaleID());
	}

	public void addElement(WorldLawElement pElement)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_laws_list.Add(pElement);
		pElement.setSelectionColor(ColorStyleLibrary.m.getSelectorColor());
	}

	public void updateCounter()
	{
		int num = 0;
		foreach (WorldLawElement item in _laws_list)
		{
			if (item.isLawEnabled())
			{
				num++;
			}
		}
		_selected_counter.text = $"{num} / {_laws_list.Count}";
	}

	public void updateButtons()
	{
		foreach (WorldLawElement item in _laws_list)
		{
			item.updateStatus();
		}
	}
}
