using System.Collections;
using UnityEngine;

public class FamilyMembersContainer : FamilyElement
{
	private ObjectPoolGenericMono<PrefabUnitElement> _pool_parents;

	private ObjectPoolGenericMono<PrefabUnitElement> _pool_children;

	[SerializeField]
	private RectTransform _list_parents;

	[SerializeField]
	private RectTransform _list_children;

	[SerializeField]
	private LocalizedText _title_parents;

	[SerializeField]
	private LocalizedText _title_children;

	[SerializeField]
	private PrefabUnitElement _prefab;

	protected override void Awake()
	{
		_pool_children = new ObjectPoolGenericMono<PrefabUnitElement>(_prefab, (Transform)(object)_list_children);
		_pool_parents = new ObjectPoolGenericMono<PrefabUnitElement>(_prefab, (Transform)(object)_list_parents);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (base.family.units.Count == 0)
		{
			yield break;
		}
		using ListPool<Actor> tFamilyMembers = new ListPool<Actor>(base.family.units);
		track_objects.AddRange(tFamilyMembers);
		tFamilyMembers.Sort(ListSorters.sortUnitByAgeOldFirst);
		tFamilyMembers.Sort(sortByMainParent);
		FamilyParentsMode family_show_parents = base.family.getActorAsset().family_show_parents;
		bool num = family_show_parents == FamilyParentsMode.Alpha;
		bool tShowNormalFamily = family_show_parents == FamilyParentsMode.Normal;
		bool tHaveParents = false;
		bool tHaveChildren = false;
		if (num)
		{
			string collectiveTermID = base.family.getActorAsset().getCollectiveTermID();
			_title_children.setKeyAndUpdate(collectiveTermID);
		}
		else
		{
			_title_children.setKeyAndUpdate("children");
		}
		foreach (ref Actor item in tFamilyMembers)
		{
			Actor tActor = item;
			if (base.family.isMainFounder(tActor) & tShowNormalFamily)
			{
				if (!tHaveParents)
				{
					tHaveParents = true;
					showParents();
				}
				yield return (object)new WaitForSecondsRealtime(0.025f);
				showMember(tActor, _pool_parents);
			}
			else
			{
				if (!tHaveChildren)
				{
					tHaveChildren = true;
					showChildren();
				}
				yield return (object)new WaitForSecondsRealtime(0.025f);
				showMember(tActor, _pool_children);
			}
		}
	}

	private void showParents()
	{
		((Component)_title_parents).gameObject.SetActive(true);
		((Component)_list_parents).gameObject.SetActive(true);
	}

	private void showChildren()
	{
		((Component)_title_children).gameObject.SetActive(true);
		((Component)_list_children).gameObject.SetActive(true);
	}

	private int sortByMainParent(Actor pActor1, Actor pActor2)
	{
		if (base.family.isMainFounder(pActor1) && !base.family.isMainFounder(pActor2))
		{
			return -1;
		}
		if (!base.family.isMainFounder(pActor1) && base.family.isMainFounder(pActor2))
		{
			return 1;
		}
		return 0;
	}

	private void showMember(Actor pActor, ObjectPoolGenericMono<PrefabUnitElement> pPool)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		PrefabUnitElement next = pPool.getNext();
		((Component)next).transform.localScale = new Vector3(0.9f, 0.9f, 1f);
		next.show(pActor);
	}

	protected override void clear()
	{
		((Component)_title_parents).gameObject.SetActive(false);
		((Component)_list_parents).gameObject.SetActive(false);
		((Component)_title_children).gameObject.SetActive(false);
		((Component)_list_children).gameObject.SetActive(false);
		_pool_children.clear();
		_pool_parents.clear();
		base.clear();
	}
}
