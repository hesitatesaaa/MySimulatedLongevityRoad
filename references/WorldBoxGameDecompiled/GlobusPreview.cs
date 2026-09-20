using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlobusPreview : MonoBehaviour
{
	public bool use_current_world_info;

	public Image main_image_1;

	public Image main_image_2;

	public GameObject images_parent;

	public Image clouds;

	public Sprite preview_default;

	private float _tweenSpeed = 18f;

	private float _gap_size = 25f;

	private float _box_size = 100f;

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			if (use_current_world_info)
			{
				setCurrentWorldSprite();
			}
			else if (SaveManager.currentWorkshopMapData != null)
			{
				setWorkshopSlotSprite();
			}
			else
			{
				startLoadCurrentSaveSlotSprite();
			}
			startTweenGlobus();
		}
	}

	private void startLoadCurrentSaveSlotSprite()
	{
		((MonoBehaviour)this).StartCoroutine(loadSaveSlotImage());
	}

	private void setCurrentWorldSprite()
	{
		Sprite currentWorldPreview = PreviewHelper.getCurrentWorldPreview();
		setSprites(currentWorldPreview);
	}

	private void setWorkshopSlotSprite()
	{
		Sprite sprites = PreviewHelper.loadWorkshopMapPreview();
		setSprites(sprites);
	}

	private void setSprites(Sprite pSprite)
	{
		makeGradient(pSprite);
		main_image_1.sprite = pSprite;
		main_image_2.sprite = pSprite;
	}

	private void showDefaultImage()
	{
		main_image_1.sprite = preview_default;
		main_image_2.sprite = preview_default;
	}

	private IEnumerator loadSaveSlotImage()
	{
		string path = SaveManager.getCurrentPreviewPath();
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			showDefaultImage();
			yield break;
		}
		UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file://" + path);
		try
		{
			yield return webRequest.SendWebRequest();
			if ((int)webRequest.result == 3 || (int)webRequest.result == 2)
			{
				showDefaultImage();
				yield break;
			}
			Texture2D content = DownloadHandlerTexture.GetContent(webRequest);
			((Object)content).name = "save_slot_preview_" + Path.GetFileNameWithoutExtension(path);
			Sprite sprites = Sprite.Create(content, new Rect(0f, 0f, (float)((Texture)content).width, (float)((Texture)content).height), new Vector2(0.5f, 0.5f));
			setSprites(sprites);
		}
		finally
		{
			((IDisposable)webRequest)?.Dispose();
		}
	}

	private void makeGradient(Sprite pSprite)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((Texture)pSprite.texture).width * 0.1f;
		Texture2D texture = pSprite.texture;
		((Object)texture).name = "gradient_" + ((Object)texture).name;
		for (int i = 0; (float)i < num; i++)
		{
			for (int j = 0; j < ((Texture)texture).height; j++)
			{
				int num2 = i;
				Color pixel = texture.GetPixel(num2, j);
				pixel.a = (float)num2 / num;
				texture.SetPixel(num2, j, pixel);
				num2 = ((Texture)pSprite.texture).width - i;
				pixel = texture.GetPixel(num2, j);
				pixel.a = (float)i / num;
				texture.SetPixel(num2, j, pixel);
			}
		}
		texture.Apply();
	}

	private void startTweenGlobus()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		float num = _box_size + _gap_size;
		float num2 = num / _tweenSpeed;
		ShortcutExtensions.DOKill((Component)(object)images_parent.transform, false);
		images_parent.transform.localPosition = new Vector3(_gap_size, 0f, 0f);
		((Tween)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(images_parent.transform, new Vector3(0f - num, 0f, 0f), num2, false), (Ease)1)).onComplete = new TweenCallback(tweenLoop);
	}

	private void tweenLoop()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		float num = _box_size + _gap_size;
		float num2 = num / _tweenSpeed;
		images_parent.transform.localPosition = new Vector3(0f, 0f, 0f);
		((Tween)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(images_parent.transform, new Vector3(0f - num, 0f, 0f), num2, false), (Ease)1)).onComplete = new TweenCallback(tweenLoop);
	}
}
