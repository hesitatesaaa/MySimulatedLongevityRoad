using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LevelPreviewButton : MonoBehaviour
{
	public bool premiumOnly = true;

	public bool worldNetUpload;

	public Image premiumIcon;

	public Image rewardAdIcon;

	public Button button;

	public SlotButtonCallback slotData;

	public Sprite defaultSprite;

	private ButtonAnimation buttonAnimation;

	public bool loaded;

	public bool loading;

	public bool autoload;

	public void click()
	{
		if (ScrollWindow.isAnimationActive())
		{
			return;
		}
		if ((Object)(object)buttonAnimation == (Object)null)
		{
			buttonAnimation = ((Component)((Component)this).transform.parent.parent.parent).GetComponent<ButtonAnimation>();
		}
		buttonAnimation.clickAnimation();
		SaveManager.setCurrentSlot(slotData.slotID);
		if (worldNetUpload)
		{
			if (SaveManager.currentSlotExists() && SaveManager.currentPreviewExists() && SaveManager.currentMetaExists())
			{
				ScrollWindow.showWindow("worldnet_upload_world_name");
			}
		}
		else if (SaveManager.currentSlotExists())
		{
			ScrollWindow.showWindow("save_slot");
		}
		else
		{
			ScrollWindow.showWindow("save_slot_new");
		}
	}

	public void checkTextureDestroy()
	{
		if ((Object)(object)((Selectable)button).image.sprite.texture != (Object)(object)defaultSprite.texture)
		{
			Object.Destroy((Object)(object)((Selectable)button).image.sprite.texture);
		}
	}

	private void OnEnable()
	{
		if (autoload)
		{
			reloadImage();
		}
	}

	private void OnDisable()
	{
		Button obj = button;
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			Image image = ((Selectable)obj).image;
			if (image == null)
			{
				obj2 = null;
			}
			else
			{
				Sprite sprite = image.sprite;
				obj2 = ((sprite != null) ? sprite.texture : null);
			}
		}
		if ((Object)obj2 == (Object)(object)defaultSprite.texture)
		{
			return;
		}
		Button obj3 = button;
		object obj4;
		if (obj3 == null)
		{
			obj4 = null;
		}
		else
		{
			Image image2 = ((Selectable)obj3).image;
			if (image2 == null)
			{
				obj4 = null;
			}
			else
			{
				Sprite sprite2 = image2.sprite;
				obj4 = ((sprite2 != null) ? sprite2.texture : null);
			}
		}
		Object.Destroy((Object)obj4);
		Button obj5 = button;
		object obj6;
		if (obj5 == null)
		{
			obj6 = null;
		}
		else
		{
			Image image3 = ((Selectable)obj5).image;
			obj6 = ((image3 != null) ? image3.sprite : null);
		}
		Object.Destroy((Object)obj6);
	}

	public void reloadImage()
	{
		if ((Object)(object)this == (Object)null || !((Behaviour)this).isActiveAndEnabled)
		{
			return;
		}
		if (loaded)
		{
			Button obj = button;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				Image image = ((Selectable)obj).image;
				obj2 = ((image != null) ? image.sprite : null);
			}
			if ((Object)obj2 != (Object)null)
			{
				return;
			}
		}
		if (loading)
		{
			return;
		}
		loading = true;
		if (SaveManager.currentWorkshopMapData != null)
		{
			loadWorkshopMapPreview();
			return;
		}
		bool flag = SaveManager.currentSlotExists();
		if (slotData.slotID == -1 && !flag)
		{
			loadImage(PreviewHelper.getCurrentWorldPreview());
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(loadSaveSlotImage(slotData.slotID));
		}
	}

	private void loadWorkshopMapPreview()
	{
		loadImage(PreviewHelper.loadWorkshopMapPreview());
	}

	private IEnumerator loadSaveSlotImage(int slotID)
	{
		string path = SaveManager.getPngSlotPath(slotID);
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			loadImage(null);
			yield break;
		}
		UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file://" + path);
		try
		{
			yield return webRequest.SendWebRequest();
			if ((int)webRequest.result == 3 || (int)webRequest.result == 2)
			{
				Debug.LogError((object)(((Object)((Component)this).gameObject).name + " " + webRequest.error + " " + path));
				loadImage(null);
			}
			else
			{
				Texture2D content = DownloadHandlerTexture.GetContent(webRequest);
				Sprite pSource = Sprite.Create(content, new Rect(0f, 0f, (float)((Texture)content).width, (float)((Texture)content).height), new Vector2(0.5f, 0.5f));
				loadImage(pSource);
			}
		}
		finally
		{
			((IDisposable)webRequest)?.Dispose();
		}
	}

	public void loadImage(Sprite pSource)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || !((Behaviour)this).isActiveAndEnabled)
		{
			loaded = false;
			loading = false;
			return;
		}
		if (!premiumOnly || Config.hasPremium)
		{
			((Component)premiumIcon).gameObject.SetActive(false);
		}
		bool flag = false;
		if ((Object)(object)pSource != (Object)null)
		{
			flag = true;
			((Texture)pSource.texture).anisoLevel = 0;
			((Texture)pSource.texture).filterMode = (FilterMode)0;
		}
		else
		{
			pSource = defaultSprite;
		}
		((Selectable)button).image.sprite = pSource;
		RectTransform component = ((Component)this).gameObject.GetComponent<RectTransform>();
		Rect rect = pSource.rect;
		float width = ((Rect)(ref rect)).width;
		rect = pSource.rect;
		component.sizeDelta = new Vector2(width, ((Rect)(ref rect)).height);
		RectTransform component2 = ((Component)((Component)button).transform.parent.parent).GetComponent<RectTransform>();
		float num = 1f;
		float num2 = 1f;
		float x = component2.sizeDelta.x;
		rect = pSource.rect;
		num = x / ((Rect)(ref rect)).width;
		float y = component2.sizeDelta.y;
		rect = pSource.rect;
		num2 = y / ((Rect)(ref rect)).height;
		float num3 = ((num > num2) ? num : num2);
		Transform parent = ((Component)this).transform.parent;
		if (!flag)
		{
			num3 = 1f;
		}
		parent.localScale = new Vector3(num3, num3, 1f);
		loaded = true;
		loading = false;
	}
}
