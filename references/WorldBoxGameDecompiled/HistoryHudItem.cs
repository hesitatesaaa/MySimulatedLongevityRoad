using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HistoryHudItem : MonoBehaviour
{
	private bool _creating = true;

	private float _remove_timer = 8f;

	private CanvasGroup _canvas_group;

	private Button _button;

	private WorldLogMessage _message;

	public Text textField;

	public Image icon;

	private RectTransform _rect_transform;

	public Image background;

	private bool _removing;

	private HistoryHud _history_hud;

	private float _time_limit;

	internal float target_bottom;

	private void Start()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		_history_hud = ((Component)this).GetComponentInParent<HistoryHud>();
		_canvas_group = ((Component)this).GetComponent<CanvasGroup>();
		_canvas_group.alpha = 0f;
		_button = ((Component)this).GetComponent<Button>();
		_rect_transform = ((Component)this).GetComponent<RectTransform>();
		((UnityEvent)_button.onClick).AddListener((UnityAction)delegate
		{
			if (!MapBox.controlsLocked() && !MapBox.isControllingUnit() && !World.world.isAnyPowerSelected())
			{
				_remove_timer = 0f;
				_message.jumpToLocation();
			}
		});
	}

	private void OnEnable()
	{
		_creating = true;
		_remove_timer = 8f;
		_removing = false;
		((Component)this).GetComponent<CanvasGroup>().alpha = 0f;
	}

	public bool isRemoving()
	{
		return _removing;
	}

	public void setMessage(WorldLogMessage pMessage)
	{
		textField.text = pMessage.getFormatedText(textField);
		((Component)textField).GetComponent<LocalizedText>().checkTextFont();
		((Component)textField).GetComponent<LocalizedText>().checkSpecialLanguages();
		if (pMessage.getAsset().path_icon != "")
		{
			Sprite sprite = SpriteTextureLoader.getSprite(pMessage.getAsset().path_icon);
			icon.sprite = sprite;
		}
		else
		{
			((Component)icon).gameObject.SetActive(false);
		}
		_message = pMessage;
	}

	public void moveTo(float newBottom)
	{
		_time_limit = 0f;
		target_bottom = newBottom;
	}

	public void moveToAndDestroy(float newBottom)
	{
		_time_limit = 0f;
		target_bottom = newBottom;
		_remove_timer = 0.5f;
		_removing = true;
	}

	private void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)background).raycastTarget = _history_hud.raycastOn;
		_rect_transform.sizeDelta = new Vector2(_rect_transform.sizeDelta.x, 10f);
		if (_creating)
		{
			if (_canvas_group.alpha < 1f)
			{
				CanvasGroup canvas_group = _canvas_group;
				canvas_group.alpha += Time.deltaTime * Config.time_scale_asset.multiplier * 2f;
			}
			else
			{
				_creating = false;
			}
		}
		else
		{
			if (Config.paused || ScrollWindow.isWindowActive() || RewardedAds.isShowing())
			{
				return;
			}
			if (_time_limit <= 2f)
			{
				_time_limit += Time.deltaTime;
				_rect_transform.SetTop(0f - Mathf.Lerp(_rect_transform.offsetMax.y, 0f - target_bottom, _time_limit / 2f));
			}
			if (_removing && _rect_transform.offsetMax.y > 10f)
			{
				_history_hud.makeInactive(this);
				return;
			}
			_remove_timer -= Time.deltaTime;
			if (_remove_timer <= 0f)
			{
				CanvasGroup canvas_group2 = _canvas_group;
				canvas_group2.alpha -= Time.deltaTime * 2f;
				if (_canvas_group.alpha <= 0f)
				{
					_history_hud.makeInactive(this);
				}
			}
		}
	}

	private void OnDisable()
	{
		_message.clear();
	}
}
