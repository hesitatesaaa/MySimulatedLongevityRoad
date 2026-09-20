using System.Collections;
using UnityEngine;

public class CityLeaderElement : CityElement
{
	[SerializeField]
	private GameObject _title_element;

	[SerializeField]
	private PrefabUnitElement _ruler_element;

	protected override IEnumerator showContent()
	{
		if (base.city.hasLeader())
		{
			track_objects.Add(base.city.leader);
			_title_element.gameObject.SetActive(true);
			((Component)_ruler_element).gameObject.SetActive(true);
			_ruler_element.show(base.city.leader);
		}
		yield break;
	}

	protected override void clear()
	{
		_title_element.gameObject.SetActive(false);
		((Component)_ruler_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_ruler_element).gameObject.activeSelf && !base.city.hasLeader())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
