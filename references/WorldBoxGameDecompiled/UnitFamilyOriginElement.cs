using System.Collections;
using UnityEngine;

public class UnitFamilyOriginElement : UnitElement
{
	[SerializeField]
	private FamilyListElement _origin_element;

	[SerializeField]
	private GameObject _family_origin_title;

	private Family _ancestor_family;

	protected override IEnumerator showContent()
	{
		if (!actor.data.ancestor_family.hasValue())
		{
			yield break;
		}
		_ancestor_family = World.world.families.get(actor.data.ancestor_family);
		if (!_ancestor_family.isRekt())
		{
			track_objects.Add(_ancestor_family);
			yield return (object)new WaitForSecondsRealtime(0.025f);
			if (_ancestor_family.isAlive())
			{
				_family_origin_title.SetActive(true);
				((Component)_origin_element).gameObject.SetActive(true);
				_origin_element.show(_ancestor_family);
			}
		}
	}

	protected override void clear()
	{
		_ancestor_family = null;
		_family_origin_title.SetActive(false);
		((Component)_origin_element).gameObject.SetActive(false);
		base.clear();
	}
}
