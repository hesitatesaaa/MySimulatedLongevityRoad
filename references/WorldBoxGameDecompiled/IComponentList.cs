using UnityEngine;
using UnityEngine.UI;

public interface IComponentList
{
	ListPool<NanoObject> getElements();

	void setShowAll();

	void setShowFavoritesOnly();

	void setShowDeadOnly();

	void setShowAliveOnly();

	void setDefault();

	void init(GameObject pNoItems, SortingTab pSortingTab, GameObject pListElementPrefab, Transform pListTransform, ScrollRect pScrollRect, Text pTitleCounter, Text pFavoritesCounter, Text pDeadCounter);
}
