using System.Collections;
using UnityEngine;

public class WarBannersContainer : WarElement
{
	private ObjectPoolGenericMono<KingdomBanner> pool_elements;

	[SerializeField]
	private KingdomBanner _prefab;

	[SerializeField]
	private Transform _container;

	protected override void Awake()
	{
		pool_elements = new ObjectPoolGenericMono<KingdomBanner>(_prefab, _container);
		base.Awake();
		((Component)_prefab).gameObject.SetActive(false);
	}

	protected override void clear()
	{
		pool_elements.clear();
		base.clear();
	}

	protected IEnumerator showBanner(Kingdom pKingdom, bool pLeft = false, bool pWinner = false, bool pLoser = false)
	{
		if (pKingdom.isRekt())
		{
			yield break;
		}
		yield return (object)new WaitForSecondsRealtime(0.025f);
		if (!pKingdom.isRekt())
		{
			track_objects.Add(pKingdom);
			KingdomBanner next = pool_elements.getNext();
			if (!((Component)(object)next).HasComponent<DraggableLayoutElement>())
			{
				((Component)(object)next).AddComponent<DraggableLayoutElement>();
			}
			next.load(pKingdom);
			if (pLeft)
			{
				next.hasLeftWar();
			}
			if (pWinner)
			{
				next.hasWon();
			}
			if (pLoser)
			{
				next.hasLost();
			}
		}
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
