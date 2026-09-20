using System;
using UnityEngine;

public class TooltipItemsRow<TComponent> : MonoBehaviour where TComponent : MonoBehaviour
{
	public Transform items_parent;

	public TComponent item;

	protected Tooltip tooltip;

	protected TooltipData tooltip_data;

	protected ObjectPoolGenericMono<TComponent> items_pool;

	public void init(Tooltip pTooltip, TooltipData pData)
	{
		tooltip = pTooltip;
		tooltip_data = pData;
		if (items_pool == null)
		{
			items_pool = new ObjectPoolGenericMono<TComponent>(item, items_parent);
		}
		loadItems();
	}

	protected virtual void loadItems()
	{
		throw new NotImplementedException();
	}
}
