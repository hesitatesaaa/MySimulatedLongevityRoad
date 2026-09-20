using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AugmentationCategory<TAugmentation, TAugmentationButton, TAugmentationEditorButton> : MonoBehaviour where TAugmentation : BaseAugmentationAsset where TAugmentationButton : AugmentationButton<TAugmentation> where TAugmentationEditorButton : AugmentationEditorButton<TAugmentationButton, TAugmentation>
{
	public Text title;

	public Text counter;

	public RectTransform height;

	public Transform augmentation_buttons_transform;

	public BaseCategoryAsset asset;

	public List<TAugmentationEditorButton> augmentation_buttons = new List<TAugmentationEditorButton>();

	public void clearDebug()
	{
		for (int i = 0; i < augmentation_buttons_transform.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)augmentation_buttons_transform.GetChild(i)).gameObject);
		}
	}

	public void hideCounter()
	{
		counter.text = "";
		((Component)counter).gameObject.SetActive(false);
	}

	public void updateCounter()
	{
		int num = 0;
		foreach (TAugmentationEditorButton augmentation_button in augmentation_buttons)
		{
			if (augmentation_button.augmentation_button.isSelected())
			{
				num++;
			}
		}
		string arg = augmentation_buttons.Count.ToString();
		counter.text = $"{num}/{arg}";
	}

	protected virtual bool isUnlocked(TAugmentationButton pButton)
	{
		throw new NotImplementedException();
	}

	private void LateUpdate()
	{
		updateValues();
	}

	private void updateValues()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = height.sizeDelta;
		sizeDelta.y = ((Component)augmentation_buttons_transform).GetComponent<RectTransform>().sizeDelta.y + 15f;
		height.sizeDelta = sizeDelta;
	}

	public int countActiveButtons()
	{
		int num = 0;
		foreach (TAugmentationEditorButton augmentation_button in augmentation_buttons)
		{
			if (((Component)augmentation_button).gameObject.activeSelf)
			{
				num++;
			}
		}
		return num;
	}

	public bool hasAugmentation(TAugmentation pTrait)
	{
		foreach (TAugmentationEditorButton augmentation_button in augmentation_buttons)
		{
			if (augmentation_button.augmentation_button.getElementAsset() == pTrait)
			{
				return true;
			}
		}
		return false;
	}
}
