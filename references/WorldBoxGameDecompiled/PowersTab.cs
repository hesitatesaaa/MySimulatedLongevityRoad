using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PowersTab : MonoBehaviour
{
	private const float END_SPACE = 5f;

	private static PowersTab _current_tab;

	private static Button _current_tab_button;

	private static PowersTab _main_tab;

	private GameObject parentObj;

	public Sprite image_normal;

	public Sprite image_selected;

	public Button powerButton;

	public static float scale_time = 0.2f;

	public static float buttonScaleTime = 0.1f;

	private List<PowerButton> _power_buttons = new List<PowerButton>();

	private PowerButton _last_selected_button;

	private PowerTabAsset _asset;

	private SelectedNanoBase _selected_nano;

	private int _children;

	public const int BACK_BUTTON_Y = -2;

	public const int TOP_BUTTON_Y = 16;

	private const int Y_TOP_ITEM = 32;

	private const int Y_BOTTOM_ITEM = -4;

	private const float Y_LINE_ITEM = 37.2f;

	private const int SIDE_SPACING = 2;

	private const int BUTTON_WIDTH = 32;

	private void Start()
	{
		parentObj = ((Component)((Component)this).transform.parent.parent).gameObject;
		_selected_nano = ((Component)this).GetComponent<SelectedNanoBase>();
		if ((Object)(object)PowerTabController.instance.tab_main == (Object)(object)this)
		{
			setActive();
			_main_tab = PowerTabController.instance.tab_main;
		}
		else
		{
			hideTab();
		}
		PowerButton[] componentsInChildren = ((Component)this).GetComponentsInChildren<PowerButton>();
		foreach (PowerButton powerButton in componentsInChildren)
		{
			if (!((Object)(object)powerButton == (Object)null) && !((Object)(object)powerButton.rect_transform == (Object)null))
			{
				_power_buttons.Add(powerButton);
			}
		}
		findNeighbours();
		_asset = AssetManager.power_tab_library.get(((Object)((Component)this).gameObject).name);
		if (_asset == null)
		{
			Debug.LogError((object)("No Power_Tab_library found for " + ((Object)((Component)this).gameObject).name));
		}
	}

	public void findNeighbours(bool pCheckForActive = false)
	{
		foreach (PowerButton power_button in _power_buttons)
		{
			power_button.up = null;
			power_button.down = null;
			power_button.left = null;
			power_button.right = null;
		}
		foreach (PowerButton power_button2 in _power_buttons)
		{
			power_button2.findNeighbours(_power_buttons, pCheckForActive);
		}
	}

	public void update()
	{
		if ((Object)(object)_selected_nano != (Object)null && !ScrollWindow.isWindowActive() && !ScrollWindow.isAnimationActive())
		{
			_selected_nano.update();
		}
		int num = ((Component)this).transform.CountChildren(delegate(Transform pChild)
		{
			if (!((Component)pChild).gameObject.activeSelf)
			{
				return false;
			}
			return !((Object)pChild).name.StartsWith("ButtonSelection");
		});
		if (_children != num)
		{
			_children = num;
			sortButtons();
			setNewWidth();
			if (_asset != null)
			{
				_asset.last_scroll_position = 0f;
			}
		}
	}

	public PowerTabAsset getAsset()
	{
		return _asset;
	}

	private void selectButton(PowerButton pButton)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		World.world.selected_buttons.unselectAll();
		if ((Object)(object)pButton == (Object)null)
		{
			World.world.selected_buttons.clearHighlightedButton();
			return;
		}
		_last_selected_button = pButton;
		if (pButton.godPower != null && pButton.godPower.activate_on_hotkey_select)
		{
			World.world.selected_buttons.clickPowerButton(pButton);
		}
		else
		{
			World.world.selected_buttons.highlightButton(pButton);
		}
		float num = (float)Screen.width / CanvasMain.instance.canvas_ui.scaleFactor;
		if (!(((Transform)pButton.rect_transform).position.x > 0f) || !(((Transform)pButton.rect_transform).position.x + 32f < (float)Screen.width))
		{
			float num2 = 0f - ((Component)pButton).transform.localPosition.x - 96f + num;
			if (num2 > 0f)
			{
				num2 = 0f;
			}
			ShortcutExtensions.DOLocalMoveX(((Component)((Component)pButton).transform.parent.parent.parent).transform, num2, 0.25f, false);
		}
	}

	internal int currentPowerIndex()
	{
		int num = -1;
		PowerButton selectedButton = World.world.selected_buttons.selectedButton;
		if ((Object)(object)selectedButton != (Object)null && (Object)(object)selectedButton != (Object)(object)_last_selected_button)
		{
			if ((Object)(object)_last_selected_button == (Object)null)
			{
				_last_selected_button = selectedButton;
			}
			else if (_power_buttons.IndexOf(selectedButton) >= 0)
			{
				_last_selected_button = selectedButton;
			}
		}
		num = _power_buttons.IndexOf(_last_selected_button);
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	internal PowerButton getActiveButton()
	{
		return _power_buttons[currentPowerIndex()];
	}

	internal void leftButton()
	{
		PowerButton powerButton = getActiveButton();
		while ((Object)(object)powerButton.left != (Object)null)
		{
			powerButton = powerButton.left;
			if (powerButton.canSelect() && ((Behaviour)powerButton).isActiveAndEnabled)
			{
				break;
			}
		}
		selectButton(powerButton);
	}

	internal void rightButton()
	{
		PowerButton powerButton = getActiveButton();
		while ((Object)(object)powerButton.right != (Object)null)
		{
			powerButton = powerButton.right;
			if (powerButton.canSelect() && ((Behaviour)powerButton).isActiveAndEnabled)
			{
				break;
			}
		}
		selectButton(powerButton);
	}

	internal void upButton()
	{
		PowerButton activeButton = getActiveButton();
		if ((Object)(object)activeButton.up != (Object)null && ((Behaviour)activeButton.up).isActiveAndEnabled && activeButton.up.canSelect())
		{
			selectButton(activeButton.up);
		}
		else if ((Object)(object)activeButton.down != (Object)null && ((Behaviour)activeButton.down).isActiveAndEnabled && activeButton.down.canSelect())
		{
			selectButton(activeButton.down);
		}
	}

	internal void downButton()
	{
		PowerButton activeButton = getActiveButton();
		if ((Object)(object)activeButton.down != (Object)null && ((Behaviour)activeButton.down).isActiveAndEnabled && activeButton.down.canSelect())
		{
			selectButton(activeButton.down);
		}
		else if ((Object)(object)activeButton.up != (Object)null && ((Behaviour)activeButton.up).isActiveAndEnabled && activeButton.up.canSelect())
		{
			selectButton(activeButton.up);
		}
	}

	public static void showTabFromButton(Button pButtonTab, bool pHideTooltips = false)
	{
		((UnityEvent)pButtonTab.onClick).Invoke();
		if (pHideTooltips)
		{
			Tooltip.hideTooltipNow();
		}
	}

	public static PowersTab getActiveTab()
	{
		if (!isTabSelected())
		{
			return _main_tab;
		}
		return _current_tab;
	}

	public static bool isTabSelected()
	{
		return (Object)(object)_current_tab != (Object)null;
	}

	public static void unselect()
	{
		_current_tab?.hideTab();
		_main_tab.setActive();
		Tooltip.hideTooltip();
	}

	public bool isCurrentPowerTabSelected()
	{
		return (Object)(object)_current_tab == (Object)(object)this;
	}

	public void tryToShowTab()
	{
		if (!isCurrentPowerTabSelected())
		{
			showTab(null);
		}
	}

	public void showTab(Button pTabButton)
	{
		bool flag = false;
		if ((Object)(object)_current_tab != (Object)null)
		{
			if (isCurrentPowerTabSelected())
			{
				flag = true;
			}
			_current_tab.hideTab();
			_current_tab = null;
		}
		if (flag)
		{
			_main_tab.setActive();
			return;
		}
		PowerTabController.instance.tab_main.hideTab();
		_current_tab = this;
		_current_tab_button = pTabButton;
		setActive(pTabButton);
		if ((Object)(object)pTabButton != (Object)null)
		{
			string textOnClick = ((Component)pTabButton).gameObject.GetComponent<TipButton>().textOnClick;
			WorldTip.instance.showToolbarText(LocalizedTextManager.getText(textOnClick));
		}
		MusicBox.playSoundUI("event:/SFX/UI/ThumbnailsSlide");
	}

	private void setActive(Button pTabButton = null)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)pTabButton != (Object)null)
		{
			((Selectable)pTabButton).image.sprite = image_selected;
		}
		((Component)this).gameObject.SetActive(true);
		((Component)this).gameObject.transform.localPosition = new Vector3(0f, -16f);
		setNewWidth();
		if (_asset != null)
		{
			PowerTabController.loadScrollPosition(_asset.last_scroll_position);
			if (_asset.tab_type_main)
			{
				SelectedTabsHistory.clear();
			}
		}
		TabCenterer component = ((Component)this).GetComponent<TabCenterer>();
		_ = (Object)(object)component == (Object)null;
		((Component)this).gameObject.transform.localScale = new Vector3(0.2f, 0.9f, 0.9f);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).gameObject.transform, 1f, scale_time), (Ease)9);
		if (_asset == null || !_asset.tab_type_main)
		{
			return;
		}
		foreach (PowerTabAsset item in AssetManager.power_tab_library.list)
		{
			item.on_main_tab_select?.Invoke(item);
		}
	}

	private bool setNewWidth()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		int childCount = ((Component)this).transform.childCount;
		RectTransform component = parentObj.GetComponent<RectTransform>();
		float x = component.sizeDelta.x;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < childCount; i++)
		{
			GameObject gameObject = ((Component)((Component)this).transform.GetChild(i)).gameObject;
			if (gameObject.activeSelf)
			{
				RectTransform component2 = gameObject.GetComponent<RectTransform>();
				float num3 = ((Transform)component2).localPosition.x + component2.sizeDelta.x;
				Rect rect = component2.rect;
				num = num3 + ((Rect)(ref rect)).x;
				if (num > num2)
				{
					num2 = num;
				}
			}
		}
		component.sizeDelta = new Vector2(num2 + 5f, component.sizeDelta.y);
		PowerTabController.instance.resetToStartScrollPosition();
		return x != num2;
	}

	public bool recalc()
	{
		return setNewWidth();
	}

	public void hideTab()
	{
		saveScrollPosition();
		completeHide();
		_current_tab = null;
		if ((Object)(object)_current_tab_button != (Object)null)
		{
			((Selectable)_current_tab_button).image.sprite = image_normal;
			_current_tab_button = null;
		}
	}

	private void saveScrollPosition()
	{
		if (_asset != null)
		{
			_asset.last_scroll_position = PowerTabController.currentScrollPosition();
		}
	}

	private void completeHide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		if (Config.isDraggingItem())
		{
			Config.getDraggingObject().KillDrag();
		}
	}

	private void prepareButtonPosition(RectTransform pRect)
	{
		if (!((Object)(object)pRect == (Object)null))
		{
			pRect.SetAnchor(AnchorPresets.TopLeft);
			pRect.SetPivot(PivotPresets.TopLeft);
		}
	}

	private void restoreButtonPosition(RectTransform pRect)
	{
		if (!((Object)(object)pRect == (Object)null))
		{
			pRect.SetPivot(PivotPresets.MiddleCenter, pKeepPosition: true);
		}
	}

	public void sortButtons()
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).gameObject.transform;
		float num = 9.6f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		float num5 = 20f;
		RectTransform pRect = default(RectTransform);
		PowerButton powerButton = default(PowerButton);
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject gameObject = ((Component)transform.GetChild(i)).gameObject;
			ShortcutExtensions.DOKill((Component)(object)gameObject.transform, true);
			if (!gameObject.activeSelf)
			{
				continue;
			}
			gameObject.TryGetComponent<RectTransform>(ref pRect);
			if (((Object)gameObject).name.StartsWith("_space_half"))
			{
				if (num4 > 0)
				{
					num5 += 36f;
					num4 = 0;
				}
				num5 += num;
				continue;
			}
			if (((Object)gameObject).name.StartsWith("_line") || ((Object)gameObject).name.StartsWith("element_"))
			{
				if (num4 > 0)
				{
					num5 += 36f;
					num4 = 0;
				}
				float num6 = 32f;
				if (((Object)gameObject).name.StartsWith("_line"))
				{
					num6 = 37.2f;
				}
				prepareButtonPosition(pRect);
				gameObject.transform.localPosition = new Vector3(num5, num6, 0f);
				float num7 = num5;
				Rect rect = ((RectTransform)gameObject.transform).rect;
				num5 = num7 + ((Rect)(ref rect)).width;
				num5 += 4f;
				restoreButtonPosition(pRect);
				continue;
			}
			gameObject.TryGetComponent<PowerButton>(ref powerButton);
			if (!((Object)gameObject).name.Contains("_space") && ((Object)(object)powerButton == (Object)null || !((Behaviour)powerButton).isActiveAndEnabled))
			{
				continue;
			}
			bool flag = false;
			if (num4 % 2 == 0)
			{
				num2++;
				num3 = 0;
				flag = num4 > 0;
			}
			else
			{
				num3 = 1;
			}
			num4++;
			if (((Object)gameObject).name.StartsWith("_space"))
			{
				if (flag)
				{
					num5 += 36f;
				}
				continue;
			}
			if (flag)
			{
				num5 += 36f;
			}
			if (!((Object)(object)powerButton == (Object)null) && ((Behaviour)powerButton).isActiveAndEnabled)
			{
				float num8 = 0f;
				num8 = ((num3 != 0) ? (-4f) : 32f);
				if (((Object)gameObject).name.Contains("tab_back_button"))
				{
					num8 = 32f;
				}
				prepareButtonPosition(pRect);
				((Component)powerButton).transform.localPosition = new Vector3(num5, num8, 0f);
				restoreButtonPosition(pRect);
			}
		}
	}
}
