using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class MetaSwitchManager : MonoBehaviour
{
	public enum Direction
	{
		Left,
		Right
	}

	private const float POSITION_SHOW = 0f;

	private const float POSITION_HIDE = -44f;

	private const float WINDOW_MAX_SIZE_PERCENT = 100f;

	private const float WINDOW_SIZE_PORTRAIT_START = 100f;

	private const float WINDOW_SIZE_PORTRAIT_END = 115f;

	private const float WINDOW_SIZE_PORTRAIT_RATIO_MIN = 1.275f;

	private const float WINDOW_SIZE_PORTRAIT_RATIO_MAX = 1.45f;

	private const float ANIMATION_DURATION = 0.35f;

	[SerializeField]
	private MetaSwitchButton _button_left;

	[SerializeField]
	private MetaSwitchButton _button_right;

	[SerializeField]
	private Text _window_number_current;

	[SerializeField]
	private Text _window_number_total;

	[SerializeField]
	private GameObject _container;

	private StatsWindow _stats_window;

	private MetaTypeAsset _meta_type_asset;

	private ListPool<NanoObject> _list;

	private static MetaSwitchManager _instance;

	private bool _is_switching_enabled;

	private bool _was_just_opened;

	private bool _is_enabled;

	private Tweener _tweener;

	private void Awake()
	{
		_instance = this;
		ScrollWindow.addCallbackOpen(delegate
		{
			_was_just_opened = true;
			enable(pOpen: true);
		});
		ScrollWindow.addCallbackShow(delegate
		{
			if (_was_just_opened)
			{
				_was_just_opened = false;
			}
			else
			{
				enable(pOpen: false);
			}
		});
		ScrollWindow.addCallbackClose(delegate
		{
			disable();
		});
		_button_left.init(Direction.Left, switchWindowsWithCheck);
		_button_right.init(Direction.Right, switchWindowsWithCheck);
	}

	private void Start()
	{
		CanvasMain.instance.addCallbackResize(delegate
		{
			if (!_is_enabled)
			{
				enable(pOpen: false, pCompleteOnDisable: false);
			}
			else
			{
				refresh(pCompleteTween: false, pCompleteOnDisable: false);
			}
		});
	}

	private void enable(bool pOpen, bool pCompleteOnDisable = true)
	{
		_is_enabled = true;
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		StatsWindow statsWindow = ((currentWindow != null) ? ((Component)currentWindow).GetComponent<StatsWindow>() : null);
		if ((Object)(object)statsWindow == (Object)(object)_stats_window && (Object)(object)_stats_window != (Object)null)
		{
			updateShowingData();
			return;
		}
		if ((Object)(object)statsWindow == (Object)null)
		{
			disable(!pOpen);
			return;
		}
		_stats_window = statsWindow;
		_meta_type_asset = AssetManager.meta_type_library.getAsset(_stats_window.meta_type);
		refresh(pCompleteTween: true, pCompleteOnDisable);
	}

	private void disable(bool pAnimated = true, bool pCompleteTween = true)
	{
		_is_enabled = false;
		if (pAnimated)
		{
			toggleControlsPosition(pState: false, pCompleteTween);
		}
		else
		{
			toggleControls(pState: false);
		}
		_stats_window = null;
		_list?.Dispose();
		_list = null;
	}

	public static void checkAndRefresh()
	{
		_instance.checkRefresh();
	}

	public static void refresh()
	{
		_instance.refresh(true, true);
	}

	public static void refreshWithoutComplete()
	{
		_instance.refresh(pCompleteTween: false);
	}

	private void checkRefresh()
	{
		if (_is_enabled)
		{
			refresh(pCompleteTween: false);
		}
	}

	internal void refresh(bool pCompleteTween = true, bool pCompleteOnDisable = true)
	{
		int optionInt = PlayerConfig.getOptionInt("ui_size_windows");
		if ((float)optionInt > 100f)
		{
			float num = Mathf.Lerp(1.275f, 1.45f, 1f - Mathf.InverseLerp(100f, 115f, (float)optionInt));
			float num2 = (float)Screen.width / (float)Screen.height * num;
			if ((float)optionInt * num2 > 100f)
			{
				disable(pAnimated: true, pCompleteOnDisable);
				return;
			}
		}
		_list?.Dispose();
		_list = _meta_type_asset.getSortedList();
		bool flag = (_is_switching_enabled = _list.Count >= 2);
		toggleControlsPosition(flag, pCompleteTween);
		if (flag)
		{
			updateShowingData();
		}
	}

	private static void switchWindowsWithCheck(Direction pDirection)
	{
		if (ScrollWindow.isWindowActive() && !ScrollWindow.isAnimationActive())
		{
			switchWindows(pDirection);
		}
	}

	public static void switchWindows(Direction pDirection)
	{
		_instance.switchWindow(pDirection);
	}

	private int getCurrentMetaIndex()
	{
		NanoObject item = _meta_type_asset.get_selected();
		int num = _list.IndexOf(item);
		if (num == -1)
		{
			_list.Add(item);
			num = _list.IndexOf(item);
		}
		return num;
	}

	private void switchWindow(Direction pDirection)
	{
		if (_is_switching_enabled && !((Object)(object)_stats_window == (Object)null) && _list.Count >= 2)
		{
			NanoObject element = getElement(pDirection);
			_meta_type_asset.set_selected(element);
			WindowHistory.popHistory();
			ScrollWindow.showWindow(_meta_type_asset.window_name);
			updateShowingData();
		}
	}

	private void updateShowingData()
	{
		updateWindowNumber();
		showBannersAndNames();
	}

	private void updateWindowNumber()
	{
		if (_list == null)
		{
			_window_number_current.text = "";
			_window_number_total.text = "";
			return;
		}
		int num = getCurrentMetaIndex() + 1;
		int count = _list.Count;
		_window_number_current.text = $"{num}";
		_window_number_total.text = $"{count}";
	}

	private void showBannersAndNames()
	{
		clear();
		showBanner(getIndex(Direction.Left), _button_left);
		showBanner(getIndex(Direction.Right), _button_right);
	}

	private IBanner showBanner(int pIndex, MetaSwitchButton pButton)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		NanoObject nanoObject = _list[pIndex];
		IBanner next = pButton.getPool().getNext(nanoObject);
		next.load(nanoObject);
		Button val = default(Button);
		if (next.gameObject.TryGetComponent<Button>(ref val))
		{
			((Behaviour)val).enabled = false;
		}
		pButton.setBanner(next);
		Transform transform = next.transform;
		Transform parent = transform.parent;
		parent.localPosition = Vector3.zero;
		parent.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		ColorAsset color = nanoObject.getColor();
		if (color != null)
		{
			string color_text = color.color_text;
			pButton.meta_name.text = nanoObject.name.ColorHex(color_text);
		}
		return next;
	}

	private void toggleControlsPosition(bool pState, bool pCompleteTween = true)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		TweenExtensions.Kill((Tween)(object)_tweener, pCompleteTween);
		float num = (pState ? 0f : (-44f));
		if (pState)
		{
			toggleControls(pState: true);
		}
		if (Mathf.Approximately(((Component)this).transform.localPosition.y, num))
		{
			return;
		}
		_tweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)this).transform, num, 0.35f, false), (Ease)10), (TweenCallback)delegate
		{
			if (!pState)
			{
				toggleControls(pState: false);
			}
			checkRefresh();
		});
	}

	private void toggleControls(bool pState)
	{
		_container.SetActive(pState);
	}

	private void clear()
	{
		_button_left.clear();
		_button_right.clear();
	}

	private NanoObject getElement(Direction pDirection)
	{
		int index = getIndex(pDirection);
		return _list[index];
	}

	private int getIndex(Direction pDirection)
	{
		int currentMetaIndex = getCurrentMetaIndex();
		return Toolbox.loopIndex((pDirection == Direction.Left) ? (currentMetaIndex - 1) : (currentMetaIndex + 1), _list.Count);
	}

	public static bool isAnimationActive()
	{
		return TweenExtensions.IsActive((Tween)(object)_instance._tweener);
	}

	public static bool isSwitcherEnabled()
	{
		return _instance._is_enabled;
	}

	public static MetaSwitchButton getLeftbutton()
	{
		return _instance._button_left;
	}

	public static MetaSwitchButton getRightButton()
	{
		return _instance._button_right;
	}
}
