using System.Collections;
using UnityEngine;

public class UnitLoverElement : UnitElement
{
	[SerializeField]
	private PrefabUnitElement _lover_element;

	[SerializeField]
	private GameObject _lover_title;

	protected override IEnumerator showContent()
	{
		if (actor.hasLover() && !actor.lover.isRekt())
		{
			track_objects.Add(actor.lover);
			_lover_element.show(actor.lover);
			_lover_title.SetActive(true);
			yield return (object)new WaitForSecondsRealtime(0.025f);
			((Component)_lover_element).gameObject.SetActive(true);
		}
	}

	protected override void clear()
	{
		_lover_title.SetActive(false);
		((Component)_lover_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_lover_element).gameObject.activeSelf)
		{
			if (!actor.hasLover())
			{
				return true;
			}
			if (actor.lover.isRekt())
			{
				return true;
			}
		}
		return base.checkRefreshWindow();
	}
}
