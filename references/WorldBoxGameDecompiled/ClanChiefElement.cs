using System.Collections;
using UnityEngine;

public class ClanChiefElement : ClanElement
{
	[SerializeField]
	private GameObject _title_element;

	[SerializeField]
	private PrefabUnitElement _chief_element;

	protected override IEnumerator showContent()
	{
		if (base.clan.hasChief())
		{
			track_objects.Add(base.clan.getChief());
			_title_element.SetActive(true);
			((Component)_chief_element).gameObject.SetActive(true);
			_chief_element.show(base.clan.getChief());
		}
		yield break;
	}

	protected override void clear()
	{
		_title_element.SetActive(false);
		((Component)_chief_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_chief_element).gameObject.activeSelf && !base.clan.hasChief())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
