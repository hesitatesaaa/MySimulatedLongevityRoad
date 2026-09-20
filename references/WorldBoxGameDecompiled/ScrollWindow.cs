using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollWindow : MonoBehaviour, IShakable
{
	public const int WINDOW_POSITION_Y = -6;

	private const float SCROLLBAR_POSITION_SHOW = 0f;

	private const float SCROLLBAR_POSITION_HIDE = -17.2f;

	private const float SCROLLBAR_ANIMATION_DURATION = 0.35f;

	private const float SCROLLBAR_ANIMATION_DURATION_COLOR = 0.35f;

	private const float SCROLLBAR_ANIMATION_DELAY = 0.25f;

	private const float SCROLLBAR_ANIMATION_DELAY_COLOR = 0.125f;

	private const string SCROLLBAR_ACTIVE_COLOR = "#E75340";

	private const string SCROLLBAR_INACTIVE_COLOR = "#545454";

	private static ScrollWindowNameAction _open_callback;

	private static ScrollWindowNameAction _show_started_callback;

	private static ScrollWindowNameAction _show_callback;

	private static ScrollWindowNameAction _show_finished_callback;

	private static ScrollWindowNameAction _hide_callback;

	private static ScrollWindowAction _close_callback;

	private static ScrollWindow _current_window = null;

	private static Dictionary<string, ScrollWindow> _all_windows = new Dictionary<string, ScrollWindow>();

	private static bool _is_window_active = false;

	private static bool _is_any_window_active = false;

	public string screen_id = "screen";

	public bool unselectPower = true;

	private Canvas _canvas;

	private CanvasGroup _canvas_group;

	private RectTransform _bg_rect;

	public Text titleText;

	[SerializeField]
	private GameObject _back_button_container;

	[SerializeField]
	private Button _back_button;

	public Image previous_window_icon;

	private static string _queued_window = "";

	public ScrollRect scrollRect;

	public Image scrollingGradient;

	public RectTransform transform_content;

	public RectTransform transform_viewport;

	public RectTransform transform_scrollRect;

	public GameObject[] destroyOnAwake;

	public bool force_gradient;

	public bool historyActionEnabled = true;

	private static bool _should_clear;

	public List<Sprite> close_sprites;

	public Image close_background;

	private int _current_background_sprite_index;

	[SerializeField]
	private TipButton _close_button_tip;

	public WindowMetaTabButtonsContainer tabs;

	public static bool skip_worldtip_hide;

	private static List<Tweener> _animations_list = new List<Tweener>();

	private Tweener _animation_tween;

	private WindowAsset _asset;

	private bool _scrollbar_cached_state;

	private Tweener _scrollbar_tweener;

	private Tweener _scrollbar_tweener_color;

	private Coroutine _scrollbar_routine;

	private bool _initialized;

	public float shake_duration { get; } = 0.5f;

	public float shake_strength { get; } = 8f;

	public Tweener shake_tween { get; set; }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isWindowActive()
	{
		return _is_window_active;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isAnimationActive()
	{
		return _animations_list.Count > 0;
	}

	private void Awake()
	{
		init();
	}

	public void init()
	{
		if (_initialized)
		{
			return;
		}
		_asset = AssetManager.window_library.get(screen_id);
		_canvas_group = ((Component)this).gameObject.GetComponent<CanvasGroup>();
		_bg_rect = ((Component)((Component)this).gameObject.transform.GetChild(0)).GetComponent<RectTransform>();
		if (destroyOnAwake != null)
		{
			GameObject[] array = destroyOnAwake;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy((Object)(object)array[i]);
			}
		}
		initComponents();
	}

	private void OnEnable()
	{
		WorldTip.hideNow();
	}

	private void Start()
	{
		if ((Object)(object)_canvas == (Object)null)
		{
			create(pHide: true);
		}
		toggleScrollbar(pState: false);
	}

	private void Update()
	{
		updateScrollbar();
		updateRightClickBack();
	}

	private void updateRightClickBack()
	{
		if (InputHelpers.GetMouseButtonDown(1) && canGoBackWithRightClick())
		{
			WindowHistory.clickBack();
		}
	}

	private bool canGoBackWithRightClick()
	{
		if (!historyActionEnabled)
		{
			return false;
		}
		if (Config.isDraggingItem())
		{
			return false;
		}
		if (!InputHelpers.mouseSupported)
		{
			return false;
		}
		if (World.world.isOverUiButton())
		{
			return false;
		}
		return true;
	}

	private void updateScrollbar()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Scrollbar verticalScrollbar = scrollRect.verticalScrollbar;
		bool flag = !Mathf.Approximately(verticalScrollbar.size, 1f);
		if (_scrollbar_cached_state != flag)
		{
			_scrollbar_cached_state = flag;
			toggleScrollbar(pState: true);
			checkGradient();
			TweenExtensions.Kill((Tween)(object)_scrollbar_tweener_color, false);
			Color val = ((!_scrollbar_cached_state) ? Toolbox.makeColor("#545454") : Toolbox.makeColor("#E75340"));
			_scrollbar_tweener_color = (Tweener)(object)DOTweenModuleUI.DOColor(((Selectable)verticalScrollbar).image, val, 0.35f);
			if (!_scrollbar_cached_state)
			{
				TweenSettingsExtensions.SetDelay<Tweener>(_scrollbar_tweener_color, 0.125f);
			}
			if (_scrollbar_routine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_scrollbar_routine);
			}
			_scrollbar_routine = ((MonoBehaviour)this).StartCoroutine(toggleScrollbarRoutine());
		}
	}

	private IEnumerator toggleScrollbarRoutine()
	{
		yield return (object)new WaitForSecondsRealtime(0.25f);
		if (!(scrollRect.verticalScrollbar.size < 1f))
		{
			toggleScrollbar(pState: false);
		}
	}

	private void toggleScrollbar(bool pState)
	{
		Scrollbar verticalScrollbar = scrollRect.verticalScrollbar;
		TweenExtensions.Kill((Tween)(object)_scrollbar_tweener, false);
		float num = (pState ? 0f : (-17.2f));
		_scrollbar_tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveX(((Component)verticalScrollbar).transform, num, 0.35f, false), (Ease)10);
	}

	public void resetScroll()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)transform_content).localPosition;
		localPosition.y = 0f;
		((Transform)transform_content).localPosition = localPosition;
	}

	internal void create(bool pHide = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		_canvas = CanvasMain.instance.canvas_windows;
		if (historyActionEnabled)
		{
			((UnityEvent)_back_button.onClick).AddListener(new UnityAction(WindowHistory.clickBack));
		}
		else
		{
			_back_button_container.gameObject.SetActive(false);
		}
		if (pHide)
		{
			hide("right", pPlaySound: false);
			finishTween();
		}
		checkGradient();
	}

	private void checkGradient()
	{
		if (force_gradient || _scrollbar_cached_state)
		{
			((Component)scrollingGradient).gameObject.SetActive(true);
		}
		else
		{
			((Component)scrollingGradient).gameObject.SetActive(false);
		}
	}

	public void clickBack()
	{
		WindowHistory.clickBack();
	}

	private static void setCurrentWindow(ScrollWindow pWindow)
	{
		if (!((Object)(object)pWindow == (Object)null))
		{
			_current_window = pWindow;
			_is_window_active = true;
			if (!_is_any_window_active)
			{
				_open_callback?.Invoke(pWindow.screen_id);
			}
			_is_any_window_active = true;
			_show_callback?.Invoke(pWindow.screen_id);
		}
	}

	private static void clearCurrentWindow(ScrollWindow pWindow)
	{
		if (!((Object)(object)_current_window == (Object)null))
		{
			_hide_callback?.Invoke(_current_window.screen_id);
			_current_window = null;
			_is_window_active = false;
			Config.debug_window_stats.setCurrent(null);
		}
	}

	public static void queueWindow(string pWindowID)
	{
		_queued_window = pWindowID;
	}

	public static void clearQueue()
	{
		if (_queued_window != "")
		{
			string queued_window = _queued_window;
			_queued_window = "";
			hideAllEvent(pWithAnimation: false);
			showWindow(queued_window);
		}
	}

	public static void showWindow(string pWindowID)
	{
		showWindow(pWindowID, false, false);
	}

	public static void showWindow(string pWindowID, bool pSkipAnimation = false, bool pBlockSame = false)
	{
		World.world.selected_buttons.clearHighlightedButton();
		if (!isAnimationActive())
		{
			bool pJustCreated = checkWindowExist(pWindowID);
			ScrollWindow scrollWindow = _all_windows[pWindowID];
			if (pBlockSame && pWindowID == _current_window.screen_id)
			{
				((IShakable)scrollWindow).shake();
				WindowToolbar.shake();
				randomDropHoveringIcon(3, 6);
			}
			else
			{
				scrollWindow.clickShow(pSkipAnimation, pJustCreated);
			}
		}
	}

	public static void randomDropHoveringIcon(int pMin, int pMax)
	{
		int num = Randy.randomInt(pMin, pMax);
		for (int i = 0; i < num; i++)
		{
			HoveringBgIconManager.randomDrop();
		}
	}

	public static string checkWindowID(string pWindowID)
	{
		if (pWindowID.StartsWith("worldnet", StringComparison.Ordinal))
		{
			return "not_found";
		}
		return pWindowID;
	}

	public static bool windowLoaded(string pWindowID)
	{
		string key = checkWindowID(pWindowID);
		return _all_windows.ContainsKey(key);
	}

	public static bool checkWindowExist(string pWindowID)
	{
		string text = checkWindowID(pWindowID);
		bool result = false;
		if (!_all_windows.ContainsKey(pWindowID))
		{
			result = true;
			string text2 = "windows/" + text;
			if (!WindowPreloader.TryGetPreloadedWindow(pWindowID, out var tScrollWindow))
			{
				ScrollWindow scrollWindow = (ScrollWindow)(object)Resources.Load(text2, typeof(ScrollWindow));
				if ((Object)(object)scrollWindow == (Object)null)
				{
					Debug.LogError((object)("Window with id " + text + " not found!"));
					scrollWindow = (ScrollWindow)(object)Resources.Load("windows/not_found", typeof(ScrollWindow));
				}
				ListPool<GameObject> pTabsObjects = disableTabsInPrefab(scrollWindow);
				tScrollWindow = Object.Instantiate<ScrollWindow>(scrollWindow, CanvasMain.instance.transformWindows);
				enableTabsInPrefab(pTabsObjects);
			}
			if (!_all_windows.ContainsKey(pWindowID))
			{
				_all_windows.Add(pWindowID, tScrollWindow);
			}
			tScrollWindow.screen_id = pWindowID;
			((Object)tScrollWindow).name = pWindowID;
			tScrollWindow.create();
		}
		return result;
	}

	public static ListPool<GameObject> disableTabsInPrefab(ScrollWindow pPrefab)
	{
		ListPool<GameObject> listPool = new ListPool<GameObject>();
		if ((Object)(object)pPrefab.tabs != (Object)null)
		{
			WindowMetaTab[] array = ((Component)pPrefab.tabs).transform.FindAllRecursive<WindowMetaTab>();
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Transform tab_element in array[i].tab_elements)
				{
					if (!((Object)(object)tab_element == (Object)null) && ((Component)tab_element).gameObject.activeSelf)
					{
						((Component)tab_element).gameObject.SetActive(false);
						listPool.Add(((Component)tab_element).gameObject);
					}
				}
			}
		}
		return listPool;
	}

	public static void enableTabsInPrefab(ListPool<GameObject> pTabsObjects)
	{
		foreach (ref GameObject pTabsObject in pTabsObjects)
		{
			pTabsObject.gameObject.SetActive(true);
		}
		pTabsObjects.Dispose();
	}

	public static ScrollWindow get(string pWindowID)
	{
		checkWindowExist(pWindowID);
		return _all_windows[pWindowID];
	}

	public void clickShow(bool pSkipAnimation = false, bool pJustCreated = false)
	{
		if (!isAnimationActive())
		{
			LogText.log("Window Opened", screen_id);
			if ((Object)(object)_current_window == (Object)(object)this)
			{
				showSameWindow();
				return;
			}
			moveAllToLeftAndRemove();
			show("right", "right", pSkipAnimation, pJustCreated);
		}
	}

	public void clickShowLeft()
	{
		if (!isAnimationActive())
		{
			LogText.log("Window Opened", screen_id);
			if ((Object)(object)_current_window == (Object)(object)this)
			{
				showSameWindow();
				return;
			}
			moveAllToRightAndRemove();
			show("left", "left");
		}
	}

	public void forceShow()
	{
		show("right", "right", pSkipAnimation: true);
	}

	public void show(string pDistPosition = "right", string pStartPosition = "right", bool pSkipAnimation = false, bool pJustCreated = false)
	{
		setActive(pActive: true, pDistPosition, pStartPosition, pSkipAnimation, pJustCreated);
		CanvasMain.addTooltipShowTimeout(0.01f);
		if (screen_id == "PremiumPurchaseError")
		{
			Analytics.LogEvent("purchase_premium_error");
		}
		Analytics.trackWindow(screen_id);
		historyAction();
		PowerTracker.trackWindow(screen_id, this);
		MusicBox.playSoundUI("event:/SFX/UI/WindowWhoosh");
	}

	private void historyAction()
	{
		if (historyActionEnabled)
		{
			if (WindowHistory.hasHistory())
			{
				_back_button_container.SetActive(true);
				Sprite sprite = WindowHistory.list.Last().window._asset.getSprite();
				previous_window_icon.sprite = sprite;
				_close_button_tip.text_description_2 = "";
			}
			else
			{
				_back_button_container.SetActive(false);
				_close_button_tip.text_description_2 = "hotkey_cancel";
			}
			WindowHistory.addIntoHistory(this);
		}
	}

	public static void moveAllToLeftAndRemove(bool pWithAnimation = true)
	{
		if ((Object)(object)_current_window != (Object)null)
		{
			if (pWithAnimation)
			{
				_current_window.moveToLeft(pRemove: true);
			}
			else
			{
				_current_window.activeToFalse();
			}
			_hide_callback?.Invoke(_current_window.screen_id);
			_current_window = null;
		}
		_is_window_active = false;
		Config.debug_window_stats.setCurrent(null);
	}

	public static bool isCurrentWindow(string pWindowID)
	{
		if ((Object)(object)_current_window == (Object)null)
		{
			return false;
		}
		return _current_window.screen_id == pWindowID;
	}

	public static ScrollWindow getCurrentWindow()
	{
		if (!isWindowActive())
		{
			return null;
		}
		return _current_window;
	}

	public static void setPreviousWindowSprite(Sprite pSprite)
	{
		if (isWindowActive())
		{
			getCurrentWindow().previous_window_icon.sprite = pSprite;
		}
	}

	public static void moveAllToRightAndRemove(bool pWithAnimation = true)
	{
		if ((Object)(object)_current_window != (Object)null)
		{
			if (pWithAnimation)
			{
				_current_window.moveToRight(pRemove: true);
			}
			else
			{
				_current_window.activeToFalse();
			}
			_hide_callback?.Invoke(_current_window.screen_id);
			_current_window = null;
		}
		_is_window_active = false;
		Config.debug_window_stats.setCurrent(null);
	}

	public void moveToLeft(bool pRemove = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		setCanvasGroupEnabled(_canvas_group, pValue: false);
		if (pRemove)
		{
			float pToX = ((Component)this).transform.localPosition.x + getHidePosLeft();
			moveTween(pToX, new TweenCallback(activeToFalse));
		}
	}

	public void moveToRight(bool pRemove = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		setCanvasGroupEnabled(_canvas_group, pValue: false);
		if (pRemove)
		{
			float pToX = ((Component)this).transform.localPosition.x - getHidePosLeft();
			moveTween(pToX, new TweenCallback(activeToFalse));
		}
	}

	public static void setCanvasGroupEnabled(CanvasGroup pGroup, bool pValue)
	{
		pGroup.interactable = pValue;
		pGroup.blocksRaycasts = pValue;
	}

	public void showSameWindow()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		sameWindowTween(0f, new TweenCallback(finishTween));
		historyAction();
		clearCurrentWindow(this);
		_show_started_callback?.Invoke(screen_id);
		((Component)this).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
		setCurrentWindow(this);
		Tooltip.hideTooltipNow();
		resetScroll();
	}

	public void setActive(bool pActive, string pDistPosition = "right", string pStartPosition = "right", bool pSkipAnimation = false, bool pJustCreated = false)
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		if (skip_worldtip_hide)
		{
			skip_worldtip_hide = false;
		}
		else
		{
			WorldTip.hideNow();
		}
		Tooltip.hideTooltipNow();
		if (unselectPower && (Object)(object)World.world.selected_buttons.selectedButton != (Object)null && World.world.selected_buttons.selectedButton.godPower.unselect_when_window)
		{
			World.world.selected_buttons.unselectAll();
		}
		if (pActive)
		{
			setCanvasGroupEnabled(_canvas_group, pValue: true);
			_show_started_callback?.Invoke(screen_id);
			((Component)this).gameObject.SetActive(true);
			setCurrentWindow(this);
			resetScroll();
			if (!(pStartPosition == "right"))
			{
				if (pStartPosition == "left")
				{
					((Component)this).transform.localPosition = new Vector3(getHidePosLeft(), -6f, ((Component)this).transform.localPosition.z);
				}
			}
			else
			{
				((Component)this).transform.localPosition = new Vector3(getHidePosRight(), -6f, ((Component)this).transform.localPosition.z);
			}
			if (pSkipAnimation)
			{
				finishTween();
				((Component)this).transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			else
			{
				moveTween(0f, new TweenCallback(finishTween), pJustCreated);
			}
		}
		else
		{
			clearCurrentWindow(this);
			if (pDistPosition == "left")
			{
				float hidePosLeft = getHidePosLeft();
				moveTween(hidePosLeft, new TweenCallback(activeToFalse), pJustCreated);
			}
			else
			{
				moveTween(getHidePosRight(), new TweenCallback(activeToFalse), pJustCreated);
			}
		}
	}

	protected void moveTween(float pToX = 0f, TweenCallback pCompleteCallback = null, bool pJustCreated = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		float num = 0.35f;
		Ease val = (Ease)27;
		if (pCompleteCallback == null)
		{
			pCompleteCallback = new TweenCallback(finishTween);
		}
		if ((Delegate?)(object)pCompleteCallback == (Delegate?)new TweenCallback(activeToFalse))
		{
			num = 0.1f;
			val = (Ease)1;
		}
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(pToX, -6f, ((Component)this).transform.localPosition.z);
		TweenExtensions.Kill((Tween)(object)_animation_tween, true);
		float num2 = 0.02f;
		if (pJustCreated)
		{
			num2 = 0.1f;
		}
		_animation_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, val2, num, false), num2), val), (TweenCallback)delegate
		{
			pCompleteCallback.Invoke();
			_animations_list.Remove(_animation_tween);
		});
		_animations_list.Add(_animation_tween);
	}

	protected void sameWindowTween(float pToX = 0f, TweenCallback pCompleteCallback = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		float num = 0.09f;
		if (pCompleteCallback == null)
		{
			pCompleteCallback = new TweenCallback(finishTween);
		}
		((Component)this).transform.localPosition = new Vector3(0f, -4f, 0f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(pToX, -6f, ((Component)this).gameObject.transform.localPosition.z);
		TweenExtensions.Kill((Tween)(object)_animation_tween, true);
		_animation_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, val, num, false), (Ease)4), (TweenCallback)delegate
		{
			pCompleteCallback.Invoke();
			_animations_list.Remove(_animation_tween);
		});
		_animations_list.Add(_animation_tween);
	}

	public static void hideAllEvent(bool pWithAnimation = true)
	{
		if (isWindowActive())
		{
			_should_clear = true;
			_close_callback?.Invoke();
			moveAllToLeftAndRemove(pWithAnimation);
		}
		Tooltip.hideTooltipNow();
		World.world.player_control.controls_lock_timer = 0.1f;
		PowerTracker.trackWatching();
	}

	public void clickCloseButton(string pDirection = "right")
	{
		clickHide(pDirection);
		_current_background_sprite_index++;
		if (_current_background_sprite_index >= close_sprites.Count)
		{
			_current_background_sprite_index = close_sprites.Count - 1;
		}
		close_background.sprite = close_sprites[_current_background_sprite_index];
	}

	public void clickHide(string pDirection = "right")
	{
		if (canClickHide())
		{
			hide(pDirection);
			((Component)World.world.selected_buttons).gameObject.SetActive(true);
			_should_clear = true;
			_close_callback?.Invoke();
			World.world.player_control.controls_lock_timer = 0.3f;
			PowerTracker.trackWatching();
		}
	}

	internal static void checkElements()
	{
		if (!isWindowActive())
		{
			return;
		}
		ScrollWindow currentWindow = getCurrentWindow();
		bool flag = currentWindow.shouldClose();
		bool flag2 = false;
		if (!flag)
		{
			flag2 = currentWindow.shouldRefresh();
		}
		if (flag | flag2)
		{
			TabbedWindow tabbedWindow = default(TabbedWindow);
			if (((Component)currentWindow).TryGetComponent<TabbedWindow>(ref tabbedWindow))
			{
				((MonoBehaviour)tabbedWindow).StopAllCoroutines();
			}
			WindowMetaElementBase[] componentsInChildren = ((Component)currentWindow).GetComponentsInChildren<WindowMetaElementBase>(false);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((MonoBehaviour)componentsInChildren[i]).StopAllCoroutines();
			}
		}
		if (flag)
		{
			hideAllEvent(pWithAnimation: false);
		}
		else if (flag2)
		{
			WindowHistory.popHistory();
			currentWindow.showSameWindow();
		}
	}

	public bool shouldClose()
	{
		TabbedWindow tabbedWindow = default(TabbedWindow);
		if (((Component)this).TryGetComponent<TabbedWindow>(ref tabbedWindow) && tabbedWindow.checkCancelWindow())
		{
			return true;
		}
		return false;
	}

	public bool shouldRefresh()
	{
		IShouldRefreshWindow[] componentsInChildren = ((Component)this).GetComponentsInChildren<IShouldRefreshWindow>(false);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].checkRefreshWindow())
			{
				return true;
			}
		}
		return false;
	}

	private void OnDisable()
	{
		if (_should_clear)
		{
			_should_clear = false;
			WindowHistory.clear();
			clear();
			_is_any_window_active = false;
			Config.debug_window_stats.setCurrent(null);
		}
		((IShakable)this).killShakeTween();
	}

	internal static void clear()
	{
		foreach (MetaTypeAsset item in AssetManager.meta_type_library.list)
		{
			item.window_action_clear?.Invoke();
		}
		NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
		if (!selectedNanoObject.isRekt())
		{
			AssetManager.meta_type_library.getAsset(selectedNanoObject.getMetaType()).selectAndInspect(selectedNanoObject, pFromNameplate: false, pCheckNameplate: true, pClearAction: true);
		}
	}

	public static bool canClickHide()
	{
		if (WorkshopUploadingWorldWindow.uploading)
		{
			return false;
		}
		return true;
	}

	public void hide(string pDirection = "right", bool pPlaySound = true)
	{
		LogText.log("Window Hide", screen_id);
		setActive(pActive: false, pDirection);
		CanvasMain.addTooltipShowTimeout(0.01f);
		setCanvasGroupEnabled(_canvas_group, pValue: false);
		if (pPlaySound)
		{
			MusicBox.playSoundUI("event:/SFX/UI/WindowClose");
		}
		Analytics.hideWindow();
	}

	public static void finishAnimations()
	{
		using ListPool<Tweener> listPool = new ListPool<Tweener>(_animations_list);
		listPool.Sort(delegate(Tweener p1, Tweener p2)
		{
			float num = TweenExtensions.Duration((Tween)(object)p1, true) * TweenExtensions.ElapsedPercentage((Tween)(object)p1, true);
			float value = TweenExtensions.Duration((Tween)(object)p2, true) * TweenExtensions.ElapsedPercentage((Tween)(object)p2, true);
			return num.CompareTo(value);
		});
		foreach (ref Tweener item in listPool)
		{
			TweenExtensions.Kill((Tween)(object)item, true);
		}
	}

	public void finishTween()
	{
		_show_finished_callback?.Invoke(screen_id);
	}

	public void activeToFalse()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public float getHidePosRight()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)_canvas).GetComponent<RectTransform>().sizeDelta.x / 2f + _bg_rect.sizeDelta.x / 2f + _bg_rect.sizeDelta.x * 0.2f;
	}

	public float getHidePosLeft()
	{
		return getHidePosRight() * -1f;
	}

	public float getHidePosLeftHalf()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Component)_canvas).GetComponent<RectTransform>().sizeDelta.x - _bg_rect.sizeDelta.x) / 2f;
		return getHidePosLeft() + num / 2f;
	}

	public float getDistBetweenWindows()
	{
		return getHidePosLeftHalf();
	}

	public void openConsole()
	{
		World.world.console.Show();
	}

	public static void addCallbackOpen(ScrollWindowNameAction pAction)
	{
		_open_callback = (ScrollWindowNameAction)Delegate.Combine(_open_callback, pAction);
	}

	public static void removeCallbackOpen(ScrollWindowNameAction pAction)
	{
		_open_callback = (ScrollWindowNameAction)Delegate.Remove(_open_callback, pAction);
	}

	public static void addCallbackShowStarted(ScrollWindowNameAction pAction)
	{
		_show_started_callback = (ScrollWindowNameAction)Delegate.Combine(_show_started_callback, pAction);
	}

	public static void removeCallbackShowStarted(ScrollWindowNameAction pAction)
	{
		_show_started_callback = (ScrollWindowNameAction)Delegate.Remove(_show_started_callback, pAction);
	}

	public static void addCallbackShow(ScrollWindowNameAction pAction)
	{
		_show_callback = (ScrollWindowNameAction)Delegate.Combine(_show_callback, pAction);
	}

	public static void removeCallbackShow(ScrollWindowNameAction pAction)
	{
		_show_callback = (ScrollWindowNameAction)Delegate.Remove(_show_callback, pAction);
	}

	public static void addCallbackShowFinished(ScrollWindowNameAction pAction)
	{
		_show_finished_callback = (ScrollWindowNameAction)Delegate.Combine(_show_finished_callback, pAction);
	}

	public static void removeCallbackShowFinished(ScrollWindowNameAction pAction)
	{
		_show_finished_callback = (ScrollWindowNameAction)Delegate.Remove(_show_finished_callback, pAction);
	}

	public static void addCallbackHide(ScrollWindowNameAction pAction)
	{
		_hide_callback = (ScrollWindowNameAction)Delegate.Combine(_hide_callback, pAction);
	}

	public static void removeCallbackHide(ScrollWindowNameAction pAction)
	{
		_hide_callback = (ScrollWindowNameAction)Delegate.Remove(_hide_callback, pAction);
	}

	public static void addCallbackClose(ScrollWindowAction pAction)
	{
		_close_callback = (ScrollWindowAction)Delegate.Combine(_close_callback, pAction);
	}

	public static void removeCallbackClose(ScrollWindowAction pAction)
	{
		_close_callback = (ScrollWindowAction)Delegate.Remove(_close_callback, pAction);
	}

	private void initComponents()
	{
	}

	public static void debug(DebugTool pTool)
	{
		if (isWindowActive())
		{
			pTool.setText("currentWindow:", getCurrentWindow().screen_id, 0f, pShowBar: false, 0L);
			pTool.setText("historyActionEnabled:", getCurrentWindow().historyActionEnabled, 0f, pShowBar: false, 0L);
		}
		pTool.setText("_is_window_active:", _is_window_active, 0f, pShowBar: false, 0L);
		pTool.setText("_is_any_window_active:", _is_any_window_active, 0f, pShowBar: false, 0L);
		pTool.setText("isAnimationActive:", isAnimationActive(), 0f, pShowBar: false, 0L);
		pTool.setText("queuedWindow:", _queued_window, 0f, pShowBar: false, 0L);
	}
}
