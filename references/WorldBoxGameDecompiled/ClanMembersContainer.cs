using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClanMembersContainer : ClanElement
{
	private ObjectPoolGenericMono<PrefabUnitElement> _pool_members;

	[SerializeField]
	private RectTransform _list_members;

	[SerializeField]
	private LocalizedText _title_members;

	[SerializeField]
	private PrefabUnitElement _prefab;

	[SerializeField]
	private Text _members_counter;

	protected override void Awake()
	{
		_pool_members = new ObjectPoolGenericMono<PrefabUnitElement>(_prefab, (Transform)(object)_list_members);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (base.clan.units.Count == 0)
		{
			yield break;
		}
		Actor chief = base.clan.getChief();
		using ListPool<Actor> _clan_members = new ListPool<Actor>(base.clan.units);
		track_objects.AddRange(_clan_members);
		_clan_members.Remove(chief);
		if (_clan_members.Count == 0)
		{
			yield break;
		}
		((Component)_title_members).gameObject.SetActive(true);
		((Component)_list_members).gameObject.SetActive(true);
		_members_counter.text = base.clan.getTextMaxMembers();
		Actor chief2 = base.clan.getChief();
		if (chief2 == null || !chief2.hasCulture())
		{
			_clan_members.Sort(ListSorters.sortUnitByAgeOldFirst);
		}
		else
		{
			ListSorters.sortUnitsSortedByAgeAndTraits(_clan_members, base.clan.getClanCulture());
		}
		foreach (ref Actor item in _clan_members)
		{
			Actor tActor = item;
			yield return (object)new WaitForSecondsRealtime(0.025f);
			showMember(tActor);
		}
	}

	private void showMember(Actor pActor)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		PrefabUnitElement next = _pool_members.getNext();
		((Component)next).transform.localScale = new Vector3(0.9f, 0.9f, 1f);
		next.show(pActor);
	}

	protected override void clear()
	{
		((Component)_title_members).gameObject.SetActive(false);
		((Component)_list_members).gameObject.SetActive(false);
		_pool_members.clear();
		base.clear();
	}
}
