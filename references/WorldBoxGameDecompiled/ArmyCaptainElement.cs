using System.Collections;
using UnityEngine;

public class ArmyCaptainElement : ArmyElement
{
	[SerializeField]
	private GameObject _title_element;

	[SerializeField]
	private PrefabUnitElement _captain_element;

	protected override IEnumerator showContent()
	{
		if (base.army.hasCaptain())
		{
			track_objects.Add(base.army.getCaptain());
			_title_element.gameObject.SetActive(true);
			((Component)_captain_element).gameObject.SetActive(true);
			_captain_element.show(base.army.getCaptain());
		}
		yield break;
	}

	protected override void clear()
	{
		_title_element.gameObject.SetActive(false);
		((Component)_captain_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_captain_element).gameObject.activeSelf && !base.army.hasCaptain())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
