using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BoxPreview : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__16_1;

		internal void _003CAwake_003Eb__16_1()
		{
			if (InputHelpers.mouseSupported)
			{
				Tooltip.hideTooltip();
			}
		}
	}

	[SerializeField]
	private Sprite _preview_default;

	[SerializeField]
	private Image _icon_gift;

	[SerializeField]
	private Image _icon_premium;

	[SerializeField]
	private Image _icon_broken;

	[SerializeField]
	private Image _icon_modded;

	[SerializeField]
	private Image _cursed_bg;

	[SerializeField]
	private Image _cursed_overlay;

	[SerializeField]
	private GameObject _favorited;

	[SerializeField]
	private Image _preview_image;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Text _text_id;

	private bool _wantLoad_preview;

	private float _timer_preview;

	private string _world_path;

	private int _slot_id;

	private MapMetaData _metaData;

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		_button.OnHover((UnityAction)delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showHoverTooltip();
			}
		});
		Button button = _button;
		object obj = _003C_003Ec._003C_003E9__16_1;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				if (InputHelpers.mouseSupported)
				{
					Tooltip.hideTooltip();
				}
			};
			_003C_003Ec._003C_003E9__16_1 = val;
			obj = (object)val;
		}
		button.OnHoverOut((UnityAction)obj);
	}

	public void setSlot(int pID)
	{
		_metaData = null;
		_text_id.text = "#" + pID;
		_slot_id = pID;
		_world_path = SaveManager.getSlotSavePath(pID);
		if (SaveManager.doesSaveExist(_world_path))
		{
			_metaData = SaveManager.getMetaFor(_world_path);
		}
		_preview_image.sprite = _preview_default;
		((Component)_icon_gift).gameObject.SetActive(false);
		((Component)_icon_premium).gameObject.SetActive(false);
		((Component)_icon_broken).gameObject.SetActive(false);
		((Component)_icon_modded).gameObject.SetActive(false);
		((Behaviour)_cursed_bg).enabled = false;
		((Behaviour)_cursed_overlay).enabled = false;
		if (_metaData != null)
		{
			if (_metaData.saveVersion > Config.WORLD_SAVE_VERSION)
			{
				((Component)_icon_broken).gameObject.SetActive(true);
			}
			if (_metaData.modded)
			{
				((Component)_icon_modded).gameObject.SetActive(true);
			}
			if (_metaData.cursed)
			{
				((Behaviour)_cursed_bg).enabled = true;
				((Behaviour)_cursed_overlay).enabled = true;
			}
		}
		_wantLoad_preview = true;
		_timer_preview = 0.02f * (float)pID;
		((Object)((Component)this).gameObject).name = "BoxPreview " + pID;
		bool active = PlayerConfig.instance.data.favorite_world == pID;
		_favorited.SetActive(active);
	}

	private void showHoverTooltip()
	{
		if (_metaData != null && Config.tooltips_active)
		{
			_metaData.temp_date_string = SaveManager.getMapCreationTime(_world_path);
			Tooltip.show(_button, "map_meta", new TooltipData
			{
				map_meta = _metaData
			});
		}
	}

	private void Update()
	{
		if (_wantLoad_preview)
		{
			if (_timer_preview > 0f)
			{
				_timer_preview -= Time.deltaTime;
				return;
			}
			_wantLoad_preview = false;
			((MonoBehaviour)this).StartCoroutine(loadSaveSlotImage());
		}
	}

	public void showDefaultImage()
	{
		_preview_image.sprite = _preview_default;
	}

	private void showPreview(Texture2D pTexture)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Sprite sprite = Sprite.Create(Toolbox.ScaleTexture(pTexture, 100, 100), new Rect(0f, 0f, 100f, 100f), new Vector2(0.5f, 0.5f));
		_preview_image.sprite = sprite;
	}

	private IEnumerator loadSaveSlotImage()
	{
		string tPath = SaveManager.generatePngPreviewPath(_world_path);
		if (string.IsNullOrEmpty(tPath) || !File.Exists(tPath))
		{
			showDefaultImage();
			yield break;
		}
		yield return CoroutineHelper.wait_for_next_frame;
		Texture2D val = new Texture2D(100, 100);
		((Object)val).name = "preview_" + _slot_id;
		try
		{
			byte[] array = File.ReadAllBytes(tPath);
			if (ImageConversion.LoadImage(val, array))
			{
				if ((Object)(object)val == (Object)null)
				{
					Debug.LogError((object)(((Object)((Component)this).gameObject).name + " texture is null from " + tPath));
					showDefaultImage();
				}
				else
				{
					showPreview(val);
				}
			}
			else
			{
				Debug.LogError((object)(((Object)((Component)this).gameObject).name + " cannot load image from " + tPath));
				showDefaultImage();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)(((Object)((Component)this).gameObject).name + " " + ex.Message + " when trying to load " + tPath));
			showDefaultImage();
		}
		Object.Destroy((Object)(object)val);
	}

	public void click()
	{
		if (ScrollWindow.isAnimationActive())
		{
			return;
		}
		if (Input.GetKey((KeyCode)304))
		{
			Application.OpenURL("file://" + _world_path);
			return;
		}
		SaveManager.setCurrentPathAndId(_world_path, _slot_id);
		if (SaveManager.currentSlotExists())
		{
			ScrollWindow.showWindow("save_slot");
		}
		else
		{
			ScrollWindow.showWindow("save_slot_new");
		}
	}
}
