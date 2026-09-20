using System.Collections;
using UnityEngine;

public class KingdomWarsContainer : KingdomDiplomacyContainer<WarBanner, War, WarData>
{
	protected override IEnumerator showContent()
	{
		if (!base.kingdom.hasEnemies())
		{
			yield break;
		}
		using ListPool<War> tList = new ListPool<War>(base.kingdom.getWars());
		track_objects.AddRange(tList);
		yield return (object)new WaitForSecondsRealtime(0.025f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0.8f, 0.8f, 1f);
		foreach (ref War item in tList)
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
				component.setDefaultScale(val);
				next.buttons_enabled = true;
				next.load(current);
				UiButtonHoverAnimation component2 = ((Component)next).GetComponent<UiButtonHoverAnimation>();
				((Behaviour)component2).enabled = false;
				component2.scale_size = 1f;
				component2.default_scale = val;
				RectTransform component3 = ((Component)next).GetComponent<RectTransform>();
				component3.SetAnchor(AnchorPresets.MiddleCenter);
				((Transform)component3).localScale = val;
				component3.anchoredPosition = new Vector2(0f, 0f);
			}
		}
	}
}
