using System.Collections;
using UnityEngine;

public class PlotMembers : PlotElement
{
	[SerializeField]
	private UiUnitAvatarElement _prefab_avatar;

	[SerializeField]
	private Transform _transform_members;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_members;

	protected override void Awake()
	{
		_pool_members = new ObjectPoolGenericMono<UiUnitAvatarElement>(_prefab_avatar, _transform_members);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		using ListPool<Actor> tPlotMembers = new ListPool<Actor>(base.plot.units.Count);
		foreach (Actor unit in base.plot.units)
		{
			if (unit.isRekt())
			{
				Debug.LogError((object)"dead actor inside plot found");
			}
			else
			{
				tPlotMembers.Add(unit);
			}
		}
		if (tPlotMembers.Count == 0)
		{
			yield break;
		}
		track_objects.AddRange(tPlotMembers);
		tPlotMembers.Sort(ListSorters.sortUnitByAgeOldFirst);
		foreach (ref Actor item in tPlotMembers)
		{
			Actor current2 = item;
			yield return showMember(current2);
		}
	}

	private IEnumerator showMember(Actor pActor)
	{
		if (pActor != null)
		{
			yield return (object)new WaitForSecondsRealtime(0.025f);
			UiUnitAvatarElement next = _pool_members.getNext();
			((Component)next).transform.localScale = new Vector3(0.6f, 0.6f, 1f);
			next.show(pActor);
		}
	}

	protected override void clear()
	{
		_pool_members.clear();
		base.clear();
	}
}
