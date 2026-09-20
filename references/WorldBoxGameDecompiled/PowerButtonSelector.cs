using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerButtonSelector : MonoBehaviour
{
	private const float TOOLBAR_TEMP_SHOW_DURATION = 1f;

	private const float TOOLBAR_TEMP_SHOW_ALPHA = 0.5f;

	public GameObject buttonSelectionSprite;

	public GameObject buttonUnlocked;

	public GameObject buttonUnlockedFlash;

	public GameObject buttonUnlockedFlashNew;

	[SerializeField]
	private CanvasGroup _toolbar_canvas_group;

	public UiMover cancelUnitSelectedMover;

	public UiMover cancelButtMover;

	public UiMover sizeButtMover;

	public UiMover clockButtMover;

	public UiMover bottomElementsMover;

	public UiMover spectateUnitMover;

	public UiMover pauseButtonMover;

	public UiMover unhideButtonMover;

	public PowerButton clockButton;

	public PowerButton sizeButton;

	public CancelButton cancelButton;

	public PowerButton pauseButton;

	public PowerButton pauseButton2;

	public PowerButton cityInfo;

	public PowerButton cityZones;

	public PowerButton boatMarks;

	public PowerButton kingsAndLeaders;

	public PowerButton historyLog;

	public PowerButton followUnit;

	public GameObject joy_control_cancel_button;

	[SerializeField]
	private Image _joy_control_cancel_button_icon;

	private Coroutine _toolbar_hide_routine;

	private bool _is_toolbar_temp_showed;

	internal PowerButton selectedButton;

	public GameObject buttons;

	internal static PowerButtonSelector instance;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		clockButtMover.setVisible(pVisible: false, pNow: true);
		sizeButtMover.setVisible(pVisible: false, pNow: true);
		cancelButtMover.setVisible(pVisible: false, pNow: true);
		cancelUnitSelectedMover.setVisible(pVisible: false, pNow: true);
		toggleBottomElements(pState: false, pNow: true);
		spectateUnitMover.setVisible(pVisible: false, pNow: true);
		resetToolbarTempShow();
	}

	internal void checkToggleIcons()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Color color = default(Color);
		((Color)(ref color))._002Ector(0.7f, 0.7f, 0.7f, 1f);
		foreach (PowerButton toggle_button in PowerButton.toggle_buttons)
		{
			toggle_button.checkToggleIcon();
			if (toggle_button.godPower.option_asset.isActive())
			{
				((Graphic)toggle_button.icon).color = Color.white;
			}
			else
			{
				((Graphic)toggle_button.icon).color = color;
			}
		}
	}

	public virtual void setSelectedPower(PowerButton pButton, GodPower pPower, bool pAnim = false)
	{
		if (!((Object)(object)selectedButton == (Object)null))
		{
			_ = selectedButton.godPower;
			selectedButton.setSelectedPower(pButton);
			selectedButton.newClickAnimation();
		}
	}

	public void setPower(PowerButton pButton)
	{
		selectedButton = pButton;
		if ((Object)(object)selectedButton != (Object)null && selectedButton.godPower != null)
		{
			Config.debug_last_selected_power_button = selectedButton.godPower.id;
			if (selectedButton.godPower.type == PowerActionType.PowerSpawnActor)
			{
				ActorAsset actorAsset = selectedButton.godPower.getActorAsset();
				if (actorAsset.has_sound_spawn)
				{
					MusicBox.playSoundUI(actorAsset.sound_spawn);
				}
			}
		}
		if ((Object)(object)selectedButton != (Object)null)
		{
			cancelButton.setIconFrom(selectedButton);
			LogText.log("Power Selected", pButton.godPower.id);
		}
		if ((Object)(object)pButton == (Object)null)
		{
			PowerTracker.setPower(null);
		}
		else
		{
			PowerTracker.setPower(pButton.godPower);
		}
	}

	public void unselectTabs()
	{
		if (PowersTab.isTabSelected())
		{
			SelectedObjects.unselectNanoObject();
			PowersTab.unselect();
			SelectedTabsHistory.clear();
		}
	}

	public void unselectAll()
	{
		if ((Object)(object)selectedButton != (Object)null)
		{
			selectedButton.unselectActivePower();
			setPower(null);
			clearHighlightedButton();
			WorldTip.instance.startHide();
		}
		if (ControllableUnit.isControllingUnit())
		{
			ControllableUnit.clear();
		}
		if (MoveCamera.hasFocusUnit())
		{
			MoveCamera.clearFocusUnitOnly();
		}
	}

	public bool isPowerSelected()
	{
		return (Object)(object)selectedButton != (Object)null;
	}

	public bool isPowerSelected(PowerButton pButton)
	{
		return (Object)(object)selectedButton == (Object)(object)pButton;
	}

	public void clickPowerButton(PowerButton pButton)
	{
		if (!pButton.canSelect())
		{
			if (!InputHelpers.mouseSupported)
			{
				showToolbarText(pButton);
			}
			return;
		}
		if ((Object)(object)selectedButton == (Object)(object)pButton)
		{
			unselectAll();
			return;
		}
		if ((Object)(object)selectedButton != (Object)null)
		{
			selectedButton.unselectActivePower();
		}
		if (pButton.godPower.select_button_action != null && pButton.godPower.select_button_action(pButton.godPower.id))
		{
			return;
		}
		setPower(pButton);
		if ((Object)(object)selectedButton != (Object)null)
		{
			highlightButton(selectedButton);
			if (selectedButton.godPower != null)
			{
				Config.logSelectedPower(selectedButton.godPower);
			}
		}
		if (InputHelpers.mouseSupported)
		{
			showToolbarText(pButton);
		}
		Analytics.LogEvent("select_power", "powerID", pButton.godPower.id);
	}

	public void showToolbarText(PowerButton pButton)
	{
		WorldTip.instance.showToolbarText(pButton.godPower);
	}

	internal void highlightButton(PowerButton pButton)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)pButton == (Object)null))
		{
			buttonSelectionSprite.SetActive(true);
			RectTransform val = (RectTransform)buttonSelectionSprite.transform;
			((Transform)val).position = ((Component)pButton).transform.position;
			((Transform)val).SetParent(((Component)pButton).transform.parent);
			((Transform)val).localScale = Vector3.one;
			val.sizeDelta = pButton.rect_transform.sizeDelta;
		}
	}

	internal void clearHighlightedButton()
	{
		buttonSelectionSprite.SetActive(false);
	}

	private void Update()
	{
		updateSelectedPowerButtons();
		updateHideUiButton();
		updateSelectedUnitCancelButton();
	}

	private bool isSpecialTabActive()
	{
		if (SelectedUnit.isSet())
		{
			return true;
		}
		if (SelectedObjects.isNanoObjectSet())
		{
			return true;
		}
		return false;
	}

	private void updateSelectedUnitCancelButton()
	{
		if (!World.world.isAnyPowerSelected() && isSpecialTabActive() && !ScrollWindow.isWindowActive())
		{
			cancelUnitSelectedMover.setVisible(pVisible: true);
		}
		else
		{
			cancelUnitSelectedMover.setVisible(pVisible: false);
		}
	}

	private void updateHideUiButton()
	{
		if (Config.ui_main_hidden)
		{
			unhideButtonMover.setVisible(pVisible: true);
		}
		else
		{
			unhideButtonMover.setVisible(pVisible: false);
		}
	}

	private void updateSelectedPowerButtons()
	{
		PowerButton powerButton = selectedButton;
		GodPower godPower = (((Object)(object)powerButton != (Object)null) ? powerButton.godPower : null);
		if ((Object)(object)powerButton == (Object)null)
		{
			cancelButtMover.setVisible(pVisible: false);
		}
		else if (ScrollWindow.isWindowActive())
		{
			cancelButtMover.setVisible(pVisible: false);
		}
		else
		{
			cancelButtMover.setVisible(pVisible: true);
		}
		bool flag = MoveCamera.inSpectatorMode();
		if (((Object)(object)powerButton == (Object)null || ScrollWindow.isWindowActive()) | flag)
		{
			sizeButtMover.setVisible(pVisible: false);
			sizeButton.hideSizes();
			if (flag)
			{
				clockButtMover.setVisible(pVisible: true);
			}
			else
			{
				clockButtMover.setVisible(pVisible: false);
				clockButton.hideSizes();
			}
		}
		else
		{
			powerButton.animate(Time.deltaTime);
			if (godPower.show_tool_sizes)
			{
				sizeButtMover.setVisible(pVisible: true);
			}
			else
			{
				sizeButtMover.setVisible(pVisible: false);
				sizeButton.hideSizes();
			}
			if (selectedButton.godPower.id == "clock")
			{
				clockButtMover.setVisible(pVisible: true);
			}
			else
			{
				clockButtMover.setVisible(pVisible: false);
				clockButton.hideSizes();
			}
		}
		if (CanvasMain.isBottomBarShowing())
		{
			toggleBottomElements(pState: true);
			if (_is_toolbar_temp_showed)
			{
				resetToolbarTempShow();
			}
		}
		else if (!_is_toolbar_temp_showed)
		{
			toggleBottomElements(pState: false);
		}
		pauseButtonMover.setVisible(flag);
		spectateUnitMover.setVisible(flag);
		updateTopButtons();
	}

	private void updateTopButtons()
	{
		if (bottomElementsMover.visible)
		{
			cancelButton.goDown = false;
			cancelButton.goUp = false;
			((Component)cancelButton).gameObject.SetActive(true);
			PremiumElementsChecker.checkElements();
			bool active = DebugConfig.isOn(DebugOption.DebugButton);
			DebugConfig.instance.debugButton.SetActive(active);
			((Component)clockButton).GetComponent<CancelButton>().goDown = false;
			joy_control_cancel_button.SetActive(false);
			return;
		}
		if (ControllableUnit.isControllingUnit())
		{
			((Component)cancelButton).gameObject.SetActive(false);
			PremiumElementsChecker.toggleActive(pState: false);
			DebugConfig.instance.debugButton.SetActive(false);
			joy_control_cancel_button.SetActive(true);
			Sprite spriteIcon = ControllableUnit.getControllableUnit().getActorAsset().getSpriteIcon();
			_joy_control_cancel_button_icon.sprite = spriteIcon;
		}
		bool flag = MoveCamera.inSpectatorMode();
		cancelButton.goDown = flag || (ControllableUnit.isControllingUnit() && !Config.joyControls);
		((Component)clockButton).GetComponent<CancelButton>().goDown = flag;
	}

	private void toggleBottomElements(bool pState, bool pNow = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		TweenCallback pCompleteCallback = null;
		if (!pState)
		{
			pCompleteCallback = (TweenCallback)delegate
			{
				buttons.SetActive(false);
				resetToolbarCanvasGroup();
			};
		}
		else
		{
			buttons.SetActive(true);
		}
		bottomElementsMover.setVisible(pState, pNow, pCompleteCallback);
	}

	public void showBarTemporary()
	{
		if (Config.game_loaded)
		{
			if (_toolbar_hide_routine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_toolbar_hide_routine);
			}
			_is_toolbar_temp_showed = true;
			_toolbar_canvas_group.blocksRaycasts = false;
			_toolbar_canvas_group.alpha = 0.5f;
			toggleBottomElements(pState: true);
			_toolbar_hide_routine = ((MonoBehaviour)this).StartCoroutine(toolbarHideRoutine());
		}
	}

	private IEnumerator toolbarHideRoutine()
	{
		yield return (object)new WaitForSeconds(1f);
		toggleBottomElements(pState: false);
		_is_toolbar_temp_showed = false;
	}

	private void resetToolbarCanvasGroup()
	{
		_toolbar_canvas_group.blocksRaycasts = true;
		_toolbar_canvas_group.alpha = 1f;
	}

	private void resetToolbarTempShow()
	{
		resetToolbarCanvasGroup();
		_is_toolbar_temp_showed = false;
		if (_toolbar_hide_routine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_toolbar_hide_routine);
		}
	}
}
