using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class PowerButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IScrollHandler
{
	public bool drag_power_bar;

	public string open_window_id = string.Empty;

	public bool block_same_window;

	public Image icon;

	private Image _image;

	private Button _button;

	public PowerButtonType type;

	public GameObject sizeButtons;

	public PowerButton mainSizeButton;

	internal GodPower godPower;

	private Image _icon_lock;

	public GameObject buttonUnlocked;

	public GameObject buttonUnlockedFlash;

	public static List<PowerButton> power_buttons = new List<PowerButton>();

	public static List<PowerButton> toggle_buttons = new List<PowerButton>();

	public static Dictionary<ActorAsset, PowerButton> actor_spawn_buttons = new Dictionary<ActorAsset, PowerButton>();

	internal RectTransform rect_transform;

	internal PowerButton left;

	internal PowerButton right;

	internal PowerButton down;

	internal PowerButton up;

	private Vector3 _default_scale;

	private Vector3 _clicked_scale;

	[HideInInspector]
	public bool is_selectable = true;

	private bool _initialized;

	private PowerButtonSelector _selected_buttons => PowerButtonSelector.instance;

	public static PowerButton get(string pID)
	{
		for (int i = 0; i < power_buttons.Count; i++)
		{
			PowerButton powerButton = power_buttons[i];
			if (((Object)((Component)powerButton).gameObject).name == pID)
			{
				return powerButton;
			}
		}
		return null;
	}

	private void init()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		rect_transform = ((Component)this).GetComponent<RectTransform>();
		_image = ((Component)this).GetComponent<Image>();
		_button = ((Component)this).GetComponent<Button>();
		if (type == PowerButtonType.Special || type == PowerButtonType.Active)
		{
			power_buttons.Add(this);
			godPower = AssetManager.powers.get(((Object)((Component)this).gameObject).name);
		}
		else if (type == PowerButtonType.Shop)
		{
			godPower = AssetManager.powers.get(((Object)((Component)this).gameObject).name);
		}
		if (godPower == null)
		{
			return;
		}
		if (godPower.disabled_on_mobile && Config.isMobile)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		if (type == PowerButtonType.Active)
		{
			GodPower.addPower(godPower, this);
		}
		if (godPower.toggle_action != null)
		{
			toggle_buttons.Add(this);
		}
		if (isActorSpawn())
		{
			ActorAsset actorAsset = godPower.getActorAsset();
			if (!actorAsset.isAvailable())
			{
				((Graphic)icon).color = Toolbox.color_black;
			}
			actor_spawn_buttons.TryAdd(actorAsset, this);
		}
	}

	private void OnEnable()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		init();
		_default_scale = ((Component)this).transform.localScale;
		_clicked_scale = _default_scale * 0.9f;
		if (type == PowerButtonType.Active && godPower != null)
		{
			if (((Object)((Component)this).gameObject).name.Contains("Button"))
			{
				Color color = default(Color);
				((Color)(ref color))._002Ector(0.5f, 0.5f, 0.5f, 1f);
				((Graphic)_image).color = color;
				((Graphic)icon).color = color;
			}
			else
			{
				godPower.id = ((Object)((Component)this).gameObject.transform).name;
			}
		}
		if (!((Component)(object)this).HasComponent<TipButton>())
		{
			_button.OnHover(new UnityAction(showTooltip));
			_button.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
		}
	}

	public void OnPointerClick(PointerEventData pEventData)
	{
		if (!draggingBarEnabled() || !ScrollRectExtended.isAnyDragged())
		{
			if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this) && (type == PowerButtonType.Active || type == PowerButtonType.Special) && (Object)(object)PowerButtonSelector.instance.selectedButton != (Object)(object)this)
			{
				showTooltip();
			}
			clickButton();
		}
	}

	internal void clickButton()
	{
		newClickAnimation();
		playSound();
		if ((type == PowerButtonType.Active || type == PowerButtonType.Library || type == PowerButtonType.Special) && (godPower == null || godPower.track_activity))
		{
			PowerTracker.trackPower(getText());
		}
		if (type == PowerButtonType.Active)
		{
			clickActivePower();
		}
		if (type == PowerButtonType.BrushSizeMain)
		{
			clickSizeMainTool();
		}
		if (type == PowerButtonType.TimeScale)
		{
			clickTimeScaleTool();
		}
		if (type == PowerButtonType.Shop)
		{
			clickShop();
		}
		if (type == PowerButtonType.Special)
		{
			clickSpecial();
		}
		if (type == PowerButtonType.Window)
		{
			clickOpenWindow();
		}
	}

	private void showTooltip()
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported && !Config.tooltips_active)
		{
			return;
		}
		if (godPower != null)
		{
			TooltipData tooltipData = new TooltipData();
			if ((Config.isComputer || Config.isEditor) && godPower.multiple_spawn_tip && type != PowerButtonType.Shop)
			{
				tooltipData.tip_description_2 = "hotkey_many_mod";
			}
			tooltipData.tip_name = godPower.getLocaleID();
			tooltipData.tip_description = godPower.getDescriptionID();
			switch (godPower.type)
			{
			case PowerActionType.PowerSpawnActor:
				tooltipData.power = godPower;
				Tooltip.show(this, "unit_spawn", tooltipData);
				break;
			case PowerActionType.PowerSpawnSeeds:
				tooltipData.power = godPower;
				Tooltip.show(this, "biome_seed", tooltipData);
				break;
			default:
				Tooltip.show(this, "normal", tooltipData);
				break;
			}
		}
		else
		{
			string text = getText();
			string description = getDescription();
			if (text == "")
			{
				return;
			}
			if (description != "")
			{
				Tooltip.show(this, "normal", new TooltipData
				{
					tip_name = text,
					tip_description = description
				});
			}
			else
			{
				Tooltip.show(this, "tip", new TooltipData
				{
					tip_name = text
				});
			}
		}
		((Component)this).transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 1f, 0.1f), (Ease)26);
	}

	private string getText()
	{
		if (godPower != null)
		{
			return godPower.getLocaleID();
		}
		string text = ((Object)_button).name.Underscore();
		if (LocalizedTextManager.stringExists("button_" + text))
		{
			return "button_" + text;
		}
		if (LocalizedTextManager.stringExists(text))
		{
			return text;
		}
		return "";
	}

	private string getDescription()
	{
		if (godPower != null)
		{
			return godPower.getDescriptionID();
		}
		string text = ((Object)_button).name.Underscore();
		if (LocalizedTextManager.stringExists("button_" + text + "_description"))
		{
			return "button_" + text + "_description";
		}
		if (LocalizedTextManager.stringExists(text + "_description"))
		{
			return text + "_description";
		}
		return "";
	}

	private void clickOpenWindow()
	{
		if (open_window_id == "steam" && (Config.isComputer || Config.isEditor))
		{
			open_window_id = "steam_workshop_main";
		}
		if (ScrollWindow.isAnimationActive() && !ScrollWindow.isCurrentWindow(open_window_id))
		{
			ScrollWindow.finishAnimations();
		}
		PowerButtonSelector.instance.clearHighlightedButton();
		showWindow(open_window_id, block_same_window);
	}

	internal void clickSpecial()
	{
		Analytics.LogEvent("select_power", "powerID", godPower.id);
		if (godPower.id == "pause")
		{
			((Component)this).GetComponent<PauseButton>().press();
		}
		if (godPower.toggle_action != null)
		{
			godPower.toggle_action(godPower.id);
			PowerButtonSelector.instance.unselectAll();
			PowerButtonSelector.instance.checkToggleIcons();
		}
	}

	public void checkToggleIcon()
	{
		GodPower godPower = this.godPower;
		if (godPower == null || string.IsNullOrEmpty(godPower.toggle_name))
		{
			return;
		}
		OptionAsset option_asset = this.godPower.option_asset;
		bool flag = option_asset.isActive();
		int num = 0;
		bool pEnabled = flag;
		bool pEnabled2 = flag;
		bool pEnabled3 = flag;
		ToggleIcon toggleIcon = null;
		ToggleIcon toggleIcon2 = null;
		ToggleIcon toggleIcon3 = null;
		ToggleIcon toggleIcon4 = null;
		Transform obj = ((Component)this).transform.Find("ToggleIcon");
		toggleIcon = ((obj != null) ? ((Component)obj).GetComponent<ToggleIcon>() : null);
		if (godPower.multi_toggle)
		{
			num = option_asset.current_int_value;
			Transform obj2 = ((Component)this).transform.Find("toggle_0");
			toggleIcon2 = ((obj2 != null) ? ((Component)obj2).GetComponent<ToggleIcon>() : null);
			Transform obj3 = ((Component)this).transform.Find("toggle_1");
			toggleIcon3 = ((obj3 != null) ? ((Component)obj3).GetComponent<ToggleIcon>() : null);
			Transform obj4 = ((Component)this).transform.Find("toggle_2");
			toggleIcon4 = ((obj4 != null) ? ((Component)obj4).GetComponent<ToggleIcon>() : null);
			switch (option_asset.max_value)
			{
			case 2:
				pEnabled = num == 0;
				pEnabled2 = num == 1;
				pEnabled3 = num == 2;
				break;
			case 1:
				pEnabled = num == 0;
				pEnabled3 = num == 1;
				break;
			default:
				pEnabled = flag;
				pEnabled2 = flag;
				pEnabled3 = flag;
				break;
			}
		}
		toggleIcon?.updateIcon(flag);
		if (godPower.multi_toggle)
		{
			toggleIcon2?.updateIconMultiToggle(flag, pEnabled3);
			toggleIcon3?.updateIconMultiToggle(flag, pEnabled);
			toggleIcon4?.updateIconMultiToggle(flag, pEnabled2);
		}
	}

	private void playSound()
	{
		SoundBox.click();
	}

	public void checkLockIcon()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		if (type == PowerButtonType.Shop)
		{
			return;
		}
		if (godPower != null && godPower.requires_premium)
		{
			if ((Object)(object)_icon_lock == (Object)null)
			{
				_icon_lock = Object.Instantiate<Image>(PrefabLibrary.instance.iconLock, ((Component)this).transform);
				((Behaviour)_icon_lock).enabled = true;
			}
			if ((Object)(object)buttonUnlocked == (Object)null)
			{
				buttonUnlocked = Object.Instantiate<GameObject>(PowerButtonSelector.instance.buttonUnlockedFlashNew, ((Component)this).transform);
				buttonUnlocked.transform.position = ((Component)this).transform.position;
				buttonUnlocked.SetActive(false);
				buttonUnlocked.transform.SetSiblingIndex(0);
				buttonUnlocked.GetComponent<RectTransform>().pivot = ((Component)this).GetComponent<RectTransform>().pivot;
			}
			if ((Object)(object)buttonUnlockedFlash == (Object)null)
			{
				buttonUnlockedFlash = Object.Instantiate<GameObject>(PowerButtonSelector.instance.buttonUnlockedFlash, ((Component)this).transform);
				buttonUnlockedFlash.transform.position = ((Component)this).transform.position;
				buttonUnlockedFlash.SetActive(false);
				buttonUnlockedFlash.transform.SetSiblingIndex(0);
				buttonUnlockedFlash.GetComponent<RectTransform>().pivot = ((Component)this).GetComponent<RectTransform>().pivot;
			}
		}
		if (!((Object)(object)_icon_lock == (Object)null))
		{
			if (godPower == null)
			{
				((Behaviour)_icon_lock).enabled = false;
			}
			else if (Config.hasPremium)
			{
				((Behaviour)_icon_lock).enabled = false;
			}
			else
			{
				((Behaviour)_icon_lock).enabled = true;
			}
		}
	}

	public void showOthers()
	{
		Debug.Log((object)"other");
	}

	public bool isSelected()
	{
		return _selected_buttons.isPowerSelected(this);
	}

	public void cancelSelection()
	{
		_selected_buttons.unselectAll();
	}

	public void unselectActivePower()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(angleToZero());
		}
		else
		{
			((Component)icon).transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		}
	}

	public void hideSizes()
	{
		sizeButtons.SetActive(false);
	}

	private void clickSizeMainTool()
	{
		sizeButtons.SetActive(!sizeButtons.activeSelf);
	}

	public void clickTimeScaleTool()
	{
		sizeButtons.SetActive(false);
		Config.setWorldSpeed(((Object)((Component)this).transform).name);
		mainSizeButton.newClickAnimation();
		World.world.player_control.inspect_timer_click = 1f;
	}

	private void clickActivePower()
	{
		_selected_buttons.clickPowerButton(this);
	}

	public bool canSelect()
	{
		if (!is_selectable)
		{
			return false;
		}
		if (!isActorSpawn())
		{
			return true;
		}
		if (godPower.getActorAsset().isAvailable())
		{
			return true;
		}
		return false;
	}

	private void clickShop()
	{
		WorldTip.showNow(LocalizedTextManager.getText(godPower.getLocaleID()) + "\n" + LocalizedTextManager.getText(godPower.getDescriptionID()), pTranslate: false, "top");
	}

	public void setSelectedPower(PowerButton pLibraryButton, bool pAnim = false)
	{
		godPower = pLibraryButton.godPower;
	}

	public void newClickAnimation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.transform.localScale = _clicked_scale;
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).gameObject.transform, _default_scale, 0.1f), (Ease)28);
	}

	protected IEnumerator angleToZero()
	{
		while (((Component)icon).transform.localEulerAngles.z != 0f)
		{
			Vector3 localEulerAngles = ((Component)icon).transform.localEulerAngles;
			localEulerAngles.z -= 100f * Time.deltaTime;
			if (localEulerAngles.z < 0f)
			{
				localEulerAngles.z = 0f;
			}
			((Component)icon).transform.localEulerAngles = localEulerAngles;
			yield return null;
		}
	}

	public void animate(float pElapsed)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)icon).transform.localEulerAngles.z < 20f)
		{
			Vector3 localEulerAngles = ((Component)icon).transform.localEulerAngles;
			localEulerAngles.z += 100f * pElapsed;
			if (localEulerAngles.z > 20f)
			{
				localEulerAngles.z = 20f;
			}
			((Component)icon).transform.localEulerAngles = localEulerAngles;
		}
	}

	public void destroyLockIcon()
	{
		if ((Object)(object)buttonUnlocked != (Object)null)
		{
			Object.Destroy((Object)(object)buttonUnlocked);
		}
		if ((Object)(object)buttonUnlockedFlash != (Object)null)
		{
			Object.Destroy((Object)(object)buttonUnlockedFlash);
		}
		if ((Object)(object)_icon_lock != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_icon_lock).gameObject);
			return;
		}
		Transform val = ((Component)this).transform.Find("IconLock");
		if ((Object)(object)val != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)val).gameObject);
			val = null;
			return;
		}
		val = ((Component)this).transform.Find("IconLock(Clone)");
		if ((Object)(object)val != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)val).gameObject);
		}
	}

	public void showWindow(string pID, bool pBlockSame)
	{
		if (!ScrollWindow.isAnimationActive())
		{
			ScrollWindow.showWindow(pID, pSkipAnimation: false, pBlockSame);
		}
	}

	public void showWindow(string pID)
	{
		ScrollWindow.showWindow(pID, false, false);
	}

	public void selectPowerTab(TweenCallback pOnComplete = null)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		if (!((Component)((Component)this).transform.parent).gameObject.activeInHierarchy)
		{
			Button tabForTabGroup = PowerTabController.instance.getTabForTabGroup(((Object)((Component)this).transform.parent).name);
			if ((Object)(object)tabForTabGroup != (Object)null)
			{
				PowersTab.showTabFromButton(tabForTabGroup);
			}
		}
		Transform parent = ((Component)this).transform.parent.parent.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		Rect rect = val.rect;
		float width = ((Rect)(ref rect)).width;
		float num = (float)Screen.width / CanvasMain.instance.canvas_ui.scaleFactor;
		float num2 = 0f - ((Component)this).transform.localPosition.x + 32f + num / 2f;
		float num3 = 0f;
		if (width > num)
		{
			num3 = -1f * (width - num);
		}
		num2 = Mathf.Clamp(num2, num3, 0f);
		ScrollRectExtended tScrollRect = ((Component)val).GetComponentInParent<ScrollRectExtended>();
		tScrollRect.movementType = ScrollRectExtended.MovementType.Clamped;
		if (pOnComplete == null)
		{
			TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveX((Transform)(object)val, num2, 1.5f, false), (Ease)27), 0.3f), (TweenCallback)delegate
			{
				tScrollRect.StopMovement();
				tScrollRect.movementType = ScrollRectExtended.MovementType.Elastic;
			});
			return;
		}
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveX((Transform)(object)val, num2, 0.125f, false), (Ease)27), (TweenCallback)delegate
		{
			tScrollRect.StopMovement();
			tScrollRect.movementType = ScrollRectExtended.MovementType.Elastic;
			pOnComplete.Invoke();
		});
	}

	internal void findNeighbours(List<PowerButton> pButtons, bool pCheckForActive = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rect_transform.anchoredPosition;
		if (anchoredPosition.y == -2f)
		{
			anchoredPosition.y = 16f;
		}
		foreach (PowerButton pButton in pButtons)
		{
			if ((Object)(object)pButton == (Object)(object)this || (pCheckForActive && !((Component)pButton).gameObject.activeSelf))
			{
				continue;
			}
			Vector2 anchoredPosition2 = pButton.rect_transform.anchoredPosition;
			if (anchoredPosition2.y == -2f)
			{
				anchoredPosition2.y = 16f;
			}
			if (anchoredPosition2.y == anchoredPosition.y)
			{
				if (anchoredPosition2.x < anchoredPosition.x)
				{
					if ((Object)(object)left == (Object)null)
					{
						left = pButton;
					}
					else if (left.rect_transform.anchoredPosition.x < anchoredPosition2.x)
					{
						left = pButton;
					}
				}
				if (anchoredPosition2.x > anchoredPosition.x)
				{
					if ((Object)(object)right == (Object)null)
					{
						right = pButton;
					}
					else if (right.rect_transform.anchoredPosition.x > anchoredPosition2.x)
					{
						right = pButton;
					}
				}
			}
			if (anchoredPosition2.x != anchoredPosition.x)
			{
				continue;
			}
			if (anchoredPosition2.y < anchoredPosition.y)
			{
				if ((Object)(object)down == (Object)null)
				{
					down = pButton;
				}
				else if (down.rect_transform.anchoredPosition.y < anchoredPosition2.y)
				{
					down = pButton;
				}
			}
			if (anchoredPosition2.y > anchoredPosition.y)
			{
				if ((Object)(object)up == (Object)null)
				{
					up = pButton;
				}
				else if (up.rect_transform.anchoredPosition.y > anchoredPosition2.y)
				{
					up = pButton;
				}
			}
		}
		if ((Object)(object)left == (Object)null)
		{
			foreach (PowerButton pButton2 in pButtons)
			{
				if ((Object)(object)pButton2 == (Object)(object)this || (pCheckForActive && !((Component)pButton2).gameObject.activeSelf))
				{
					continue;
				}
				Vector2 anchoredPosition3 = pButton2.rect_transform.anchoredPosition;
				if (anchoredPosition3.y == -2f)
				{
					anchoredPosition3.y = 16f;
				}
				if (anchoredPosition3.y == anchoredPosition.y)
				{
					if ((Object)(object)left == (Object)null)
					{
						left = pButton2;
					}
					else if (left.rect_transform.anchoredPosition.x < anchoredPosition3.x)
					{
						left = pButton2;
					}
				}
			}
		}
		if (!((Object)(object)right == (Object)null))
		{
			return;
		}
		foreach (PowerButton pButton3 in pButtons)
		{
			if ((Object)(object)pButton3 == (Object)(object)this || (pCheckForActive && !((Component)pButton3).gameObject.activeSelf))
			{
				continue;
			}
			Vector2 anchoredPosition4 = pButton3.rect_transform.anchoredPosition;
			if (anchoredPosition4.y == -2f)
			{
				anchoredPosition4.y = 16f;
			}
			if (anchoredPosition4.y == anchoredPosition.y)
			{
				if ((Object)(object)right == (Object)null)
				{
					right = pButton3;
				}
				else if (right.rect_transform.anchoredPosition.x > anchoredPosition4.x)
				{
					right = pButton3;
				}
			}
		}
	}

	private bool isActorSpawn()
	{
		if (godPower != null)
		{
			return godPower.type == PowerActionType.PowerSpawnActor;
		}
		return false;
	}

	public static void checkActorSpawnButtons()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Color color = default(Color);
		((Color)(ref color))._002Ector(0.75f, 0.75f, 0.75f, 0.9f);
		foreach (KeyValuePair<ActorAsset, PowerButton> actor_spawn_button in actor_spawn_buttons)
		{
			ActorAsset key = actor_spawn_button.Key;
			PowerButton value = actor_spawn_button.Value;
			if (key.isAvailable())
			{
				if (key.countPopulation() > 0)
				{
					value._image.sprite = ToolbarButtons.getSpriteButtonUnitExists();
					((Graphic)value.icon).color = Toolbox.color_white;
				}
				else
				{
					value._image.sprite = ToolbarButtons.getSpriteButtonNormal();
					((Graphic)value.icon).color = color;
				}
			}
			else
			{
				((Graphic)value.icon).color = Toolbox.color_black;
			}
		}
	}

	private bool draggingBarEnabled()
	{
		if (drag_power_bar)
		{
			return InputHelpers.mouseSupported;
		}
		return false;
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		if (draggingBarEnabled())
		{
			ScrollRectExtended.SendMessageToAll("OnBeginDrag", pEventData);
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		if (draggingBarEnabled())
		{
			ScrollRectExtended.SendMessageToAll("OnDrag", pEventData);
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		if (draggingBarEnabled())
		{
			ScrollRectExtended.SendMessageToAll("OnEndDrag", pEventData);
		}
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		if (draggingBarEnabled())
		{
			ScrollRectExtended.SendMessageToAll("OnInitializePotentialDrag", pEventData);
		}
	}

	public void OnScroll(PointerEventData pEventData)
	{
		if (draggingBarEnabled())
		{
			ScrollRectExtended.SendMessageToAll("OnScroll", pEventData);
		}
	}

	public void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		power_buttons.Remove(this);
		toggle_buttons.Remove(this);
		if (actor_spawn_buttons.ContainsValue(this))
		{
			ActorAsset key = actor_spawn_buttons.FirstOrDefault((KeyValuePair<ActorAsset, PowerButton> x) => (Object)(object)x.Value == (Object)(object)this).Key;
			actor_spawn_buttons.Remove(key);
		}
	}
}
