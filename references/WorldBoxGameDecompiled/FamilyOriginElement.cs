using System.Collections;
using UnityEngine;

public class FamilyOriginElement : FamilyElement
{
	[SerializeField]
	private GameObject _family_origin_title;

	[SerializeField]
	private FamilyListElement _prefab;

	private ObjectPoolGenericMono<FamilyListElement> _pool_elements;

	[SerializeField]
	private Transform _container;

	protected override void Awake()
	{
		_pool_elements = new ObjectPoolGenericMono<FamilyListElement>(_prefab, _container);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		using ListPool<Family> tList = new ListPool<Family>(base.family.getOriginFamilies());
		track_objects.AddRange(tList);
		foreach (ref Family item in tList)
		{
			Family tFamily = item;
			if (!tFamily.isRekt())
			{
				yield return (object)new WaitForSecondsRealtime(0.025f);
				if (tFamily.isAlive())
				{
					_family_origin_title.SetActive(true);
					_pool_elements.getNext().show(tFamily);
				}
			}
		}
	}

	protected override void clear()
	{
		_pool_elements.clear();
		_family_origin_title.SetActive(false);
		base.clear();
	}

	protected override void clearInitial()
	{
		for (int i = 0; i < _container.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)_container.GetChild(i)).gameObject);
		}
		base.clearInitial();
	}
}
