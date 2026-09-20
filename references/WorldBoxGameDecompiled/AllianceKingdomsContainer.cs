using System.Collections;
using UnityEngine;

public class AllianceKingdomsContainer : AllianceBannersContainer<KingdomBanner, Kingdom, KingdomData>
{
	protected override IEnumerator showContent()
	{
		if (base.alliance.kingdoms_hashset.Count == 0)
		{
			yield break;
		}
		using ListPool<Kingdom> tList = new ListPool<Kingdom>(base.alliance.kingdoms_hashset);
		track_objects.AddRange(tList);
		foreach (ref Kingdom item in tList)
		{
			Kingdom tKingdom = item;
			yield return (object)new WaitForSecondsRealtime(0.025f);
			showBanner(tKingdom);
		}
	}

	protected void showBanner(Kingdom pKingdom)
	{
		KingdomBanner next = pool_elements.getNext();
		next.load(pKingdom);
		((Component)(object)next).AddComponent<DraggableLayoutElement>();
	}
}
