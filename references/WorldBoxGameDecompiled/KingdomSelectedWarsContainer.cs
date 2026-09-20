using UnityEngine;

public class KingdomSelectedWarsContainer : KingdomDiplomacyContainer<WarBanner, War, WarData>
{
	protected override void OnEnable()
	{
	}

	public void update(Kingdom pKingdom)
	{
		meta_object = pKingdom;
		clear();
		if (!base.kingdom.hasEnemies())
		{
			return;
		}
		using ListPool<War> listPool = new ListPool<War>(base.kingdom.getWars());
		track_objects.AddRange(listPool);
		foreach (ref War item in listPool)
		{
			War current = item;
			if (!current.isRekt())
			{
				WarBanner next = pool_elements.getNext();
				TipButton component = ((Component)next).GetComponent<TipButton>();
				if (!((Component)(object)next).HasComponent<DraggableLayoutElement>())
				{
					((Component)(object)next).AddComponent<DraggableLayoutElement>();
				}
				component.showOnClick = true;
				next.buttons_enabled = true;
				next.load(current);
			}
		}
	}
}
