using System.Collections;
using UnityEngine;

public class KingdomAlliesContainer : KingdomDiplomacyContainer<KingdomBanner, Kingdom, KingdomData>
{
	protected override IEnumerator showContent()
	{
		using ListPool<Kingdom> tList = World.world.wars.getNeutralKingdoms(base.kingdom);
		if (base.kingdom.hasAlliance())
		{
			foreach (Kingdom item in base.kingdom.getAlliance().kingdoms_list)
			{
				if (item != base.kingdom && !item.isRekt())
				{
					tList.Add(item);
				}
			}
		}
		track_objects.AddRange(tList);
		if (tList.Count == 0)
		{
			yield break;
		}
		yield return (object)new WaitForSecondsRealtime(0.025f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0.5f, 0.5f, 1f);
		foreach (ref Kingdom item2 in tList)
		{
			Kingdom current2 = item2;
			if (!current2.isRekt())
			{
				KingdomBanner next = pool_elements.getNext();
				next.diplo_banner = true;
				((Component)next).GetComponent<TipButton>().showOnClick = true;
				((Behaviour)((Component)next).GetComponentInChildren<RotateOnHover>()).enabled = true;
				if (!((Component)(object)next).HasComponent<DraggableLayoutElement>())
				{
					((Component)(object)next).AddComponent<DraggableLayoutElement>();
				}
				next.load(current2);
				((Behaviour)((Component)next).GetComponent<UiButtonHoverAnimation>()).enabled = false;
				((Component)next).GetComponent<UiButtonHoverAnimation>().scale_size = 1f;
				((Component)next).GetComponent<UiButtonHoverAnimation>().default_scale = val;
				((Component)next).GetComponent<TipButton>().setDefaultScale(val);
				RectTransform component = ((Component)next).GetComponent<RectTransform>();
				component.SetAnchor(AnchorPresets.MiddleCenter);
				((Transform)component).localScale = val;
				component.anchoredPosition = new Vector2(0f, 0f);
			}
		}
	}
}
