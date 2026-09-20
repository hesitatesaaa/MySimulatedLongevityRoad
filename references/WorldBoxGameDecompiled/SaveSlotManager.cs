using System.Collections.Generic;
using UnityEngine;

public class SaveSlotManager : MonoBehaviour
{
	public GameObject buttonsContainer;

	private List<LevelPreviewButton> previews = new List<LevelPreviewButton>();

	private List<GameObject> containers = new List<GameObject>();

	public GameObject slotButtonPrefab;

	public RectTransform content;

	private Vector3 originalPos;

	public bool loaded;

	public bool worldNetUpload;

	private void Init()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		SaveManager.clearCurrentSelectedWorld();
		int num = 65;
		int num2 = 65;
		int num3 = 0;
		int num4 = 1;
		int num5 = 10;
		for (int i = 0; i < num5; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(slotButtonPrefab, buttonsContainer.transform);
			val.transform.localPosition = new Vector3((float)(-num), (float)(-num3 * num2));
			setID(val, num4++);
			val = Object.Instantiate<GameObject>(slotButtonPrefab, buttonsContainer.transform);
			val.transform.localPosition = new Vector3(0f, (float)(-num3 * num2));
			setID(val, num4++);
			val = Object.Instantiate<GameObject>(slotButtonPrefab, buttonsContainer.transform);
			val.transform.localPosition = new Vector3((float)num, (float)(-num3 * num2));
			setID(val, num4++);
			num3++;
		}
		content.sizeDelta = new Vector2(0f, (float)(num5 * num2));
	}

	private void OnEnable()
	{
		loaded = false;
		Init();
	}

	private void Update()
	{
		foreach (LevelPreviewButton preview in previews)
		{
			if (!preview.loaded && !preview.loading)
			{
				preview.reloadImage();
				break;
			}
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < containers.Count; i++)
		{
			previews[i].checkTextureDestroy();
			Object.Destroy((Object)(object)containers[i]);
			containers[i] = null;
		}
		previews.Clear();
		containers.Clear();
	}

	private void setID(GameObject pContainer, int pID)
	{
		Transform val = pContainer.transform.Find("AnimationContainer/Mask/SizeContainer/Button");
		((Component)val).GetComponent<SlotButtonCallback>().slotID = pID;
		((Component)val).GetComponent<LevelPreviewButton>().loaded = false;
		((Component)val).GetComponent<LevelPreviewButton>().worldNetUpload = worldNetUpload;
		if (pID > 1)
		{
			((Component)val).GetComponent<LevelPreviewButton>().premiumOnly = true;
		}
		previews.Add(((Component)val).GetComponent<LevelPreviewButton>());
		containers.Add(pContainer);
	}
}
