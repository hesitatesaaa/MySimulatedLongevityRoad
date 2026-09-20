using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipTraitsRow<TTrait> : TooltipItemsRow<Image> where TTrait : BaseTrait<TTrait>
{
	protected virtual IReadOnlyCollection<TTrait> traits_hashset
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	protected override void loadItems()
	{
		items_pool.clear();
		IReadOnlyCollection<TTrait> readOnlyCollection = traits_hashset;
		if (readOnlyCollection == null || readOnlyCollection.Count == 0)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		foreach (TTrait item in traits_hashset)
		{
			items_pool.getNext().sprite = item.getSprite();
		}
	}
}
