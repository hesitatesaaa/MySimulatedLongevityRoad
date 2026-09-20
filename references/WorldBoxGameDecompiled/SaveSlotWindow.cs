using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotWindow : MonoBehaviour
{
	public GameObject buttonsContainer;

	private List<BoxPreview> previews = new List<BoxPreview>();

	public GameObject slotButtonPrefabNew;

	public ScrollRect scroll_rect;

	private void checkChildren()
	{
		if (previews.Count <= 0)
		{
			BoxPreview[] componentsInChildren = buttonsContainer.GetComponentsInChildren<BoxPreview>();
			previews.AddRange(componentsInChildren);
		}
	}

	private void OnEnable()
	{
		checkChildren();
		prepareLoadPreviews();
	}

	private void prepareLoadPreviews()
	{
		SaveManager.clearCurrentSelectedWorld();
		for (int i = 0; i < previews.Count; i++)
		{
			previews[i].setSlot(i + 1);
		}
	}
}
