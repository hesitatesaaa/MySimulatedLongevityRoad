using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

public class WindowToolbar : MonoBehaviour, IShakable
{
	private const int REFERENCE_HEIGHT = 1080;

	private const int HIDE_DISTANCE_MIN = 225;

	private const int HIDE_DISTANCE_MAX = 440;

	private const float FOCUS_SCROLL_TOP_ERROR = 0.5f;

	private const float FOCUS_SCROLL_BOTTOM_ERROR = 0.1f;

	private const float FOCUS_SCROLL_BUTTON_ERROR = 0.5f;

	private const float FOCUS_SCROLL_DURATION = 0.3f;

	private static WindowToolbar _instance;

	private UiMover _ui_mover;

	private Transform _content;

	private RectTransform _parent_rect;

	private CanvasGroup _canvas_group;

	[SerializeField]
	private ScrollRectExtended _scroll_rect;

	[SerializeField]
	private RectTransform _content_rect;

	[SerializeField]
	private RectTransform _windows_parent;

	[SerializeField]
	private Transform _selector_base_parent;

	[SerializeField]
	private Transform _selector;

	private float _ui_size_min;

	private float _ui_size_max;

	private Dictionary<string, Transform> _window_buttons = new Dictionary<string, Transform>();

	public bool _last_state;

	public float shake_duration { get; } = 0.5f;

	public float shake_strength { get; } = 8f;

	public Tweener shake_tween { get; set; }

	private void Awake()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		_instance = this;
		_ui_mover = ((Component)this).GetComponent<UiMover>();
		_content = ((Component)this).transform.FindRecursive("Content");
		_canvas_group = ((Component)this).GetComponent<CanvasGroup>();
		_parent_rect = ((Component)((Component)this).transform.parent).GetComponent<RectTransform>();
		ScrollWindow.addCallbackShow(delegate
		{
			toggleShow(pState: true);
		});
		ScrollWindow.addCallbackClose(delegate
		{
			toggleShow(pState: false);
		});
		OptionAsset optionAsset = AssetManager.options_library.get("ui_size_windows");
		_ui_size_min = (float)optionAsset.min_value / 100f;
		_ui_size_max = (float)optionAsset.max_value / 100f;
		_ui_mover.initPos.y = -6f;
		_ui_mover.hidePos.y = -6f;
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.y = -6f;
		((Component)this).transform.localPosition = localPosition;
		PowerButton[] componentsInChildren = ((Component)this).GetComponentsInChildren<PowerButton>();
		foreach (PowerButton powerButton in componentsInChildren)
		{
			if (powerButton.type == PowerButtonType.Window)
			{
				_window_buttons.Add(powerButton.open_window_id, ((Component)powerButton).transform);
			}
		}
		ScrollWindow.addCallbackShow(checkSelectOnWindowShow);
		ScrollWindow.addCallbackHide(checkDeselectOnWindowHide);
	}

	private void Start()
	{
		CanvasMain.instance.addCallbackResize(onResize);
		CanvasMain.instance.addCallbackResizeUI(checkShow);
	}

	private void OnDisable()
	{
		((IShakable)this).killShakeTween();
	}

	private void onResize(float pWidth, float pHeight)
	{
		float pUISize = (float)PlayerConfig.getOptionInt("ui_size_windows") / 100f;
		checkShow(pUISize);
	}

	private void checkShow(float pUISize)
	{
		if (isHideDistance(pUISize))
		{
			toggleShow(pState: false);
		}
		else if (ScrollWindow.isWindowActive())
		{
			toggleShow(pState: true);
		}
	}

	public static void toggleActive(bool pState)
	{
		_instance.toggleActiveInstance(pState);
	}

	private void toggleActiveInstance(bool pState)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (pState)
		{
			((Component)this).gameObject.SetActive(true);
			if (ScrollWindow.isWindowActive())
			{
				toggleShow(pState: true);
			}
			else
			{
				_ui_mover.setVisible(pVisible: false, pNow: true);
			}
		}
		else
		{
			_ui_mover.setVisible(pVisible: false, pNow: false, (TweenCallback)delegate
			{
				((Component)this).gameObject.SetActive(false);
			});
		}
	}

	private void toggleShow(bool pState)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		if (!((Component)this).gameObject.activeSelf)
		{
			return;
		}
		if (pState)
		{
			float pUISize = (float)PlayerConfig.getIntValue("ui_size_windows") / 100f;
			if (isHideDistance(pUISize))
			{
				pState = false;
			}
			else if (!AssetManager.window_library.get(ScrollWindow.getCurrentWindow().screen_id).window_toolbar_enabled)
			{
				pState = false;
			}
		}
		_last_state = pState;
		_canvas_group.blocksRaycasts = pState;
		if (pState)
		{
			((Component)_content).gameObject.SetActive(true);
		}
		TweenCallback pCompleteCallback = null;
		if (!pState)
		{
			pCompleteCallback = (TweenCallback)delegate
			{
				((Component)_content).gameObject.SetActive(_last_state);
			};
		}
		_ui_mover.setVisible(pState, pNow: false, pCompleteCallback);
	}

	public static void shake()
	{
		((IShakable)_instance).shake();
	}

	private bool isPortrait()
	{
		return false;
	}

	private bool isHideDistance(float pUISize)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!ScrollWindow.isWindowActive())
		{
			return true;
		}
		float num = Mathf.InverseLerp(_ui_size_min, _ui_size_max, pUISize);
		float num2 = (float)Screen.height / 1080f;
		float num3 = Mathf.Lerp(225f, 440f, num) * num2;
		Rect val = _windows_parent.WorldRect();
		float xMin = ((Rect)(ref val)).xMin;
		val = _parent_rect.WorldRect();
		float xMin2 = ((Rect)(ref val)).xMin;
		return xMin - xMin2 < num3;
	}

	private void checkSelectOnWindowShow(string pWindowId)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!_window_buttons.TryGetValue(pWindowId, out var value))
		{
			pWindowId = AssetManager.window_library.get(pWindowId).related_parent_window;
			if (string.IsNullOrEmpty(pWindowId) || !_window_buttons.TryGetValue(pWindowId, out value))
			{
				return;
			}
		}
		_selector.SetParent(value);
		_selector.localPosition = Vector3.zero;
		_selector.localScale = Vector3.one;
		scrollToButton(value);
	}

	private void scrollToButton(Transform pButtonTransform)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Rect worldRect = _scroll_rect.rectTransform.GetWorldRect();
		float yMax = ((Rect)(ref worldRect)).yMax;
		Rect worldRect2 = _content_rect.GetWorldRect();
		float height = ((Rect)(ref worldRect2)).height;
		Rect worldRect3 = RectTransformExtensions.GetWorldRect((RectTransform)pButtonTransform);
		float num = pButtonTransform.position.y - ((Rect)(ref worldRect2)).yMax + yMax;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f, ((Rect)(ref worldRect3)).height * 0.5f);
		if (!((Rect)(ref worldRect)).Contains(((Rect)(ref worldRect3)).position - val) || !((Rect)(ref worldRect)).Contains(((Rect)(ref worldRect3)).max + val))
		{
			float num2 = (num - ((Rect)(ref worldRect3)).height / 0.5f) / height;
			float num3 = (float)PlayerConfig.getOptionInt("ui_size_windows") / 100f;
			float num4 = 1f - Mathf.InverseLerp(_ui_size_min, _ui_size_max, num3);
			if (num2 > 1f - (0.5f - num4))
			{
				num2 = 1f;
			}
			else if (num2 < 0.1f + num4)
			{
				num2 = 0f;
			}
			DOTween.To((DOGetter<float>)(() => _scroll_rect.verticalScrollbar.value), (DOSetter<float>)delegate(float pValue)
			{
				_scroll_rect.verticalScrollbar.value = pValue;
			}, num2, 0.3f);
		}
	}

	private void checkDeselectOnWindowHide(string pWindowId)
	{
		_selector.SetParent(_selector_base_parent);
	}
}
