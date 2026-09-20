using UnityEngine;
using UnityEngine.UI;

public class SwitchStateButton : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Sprite _sprite_enabled;

	[SerializeField]
	private Sprite _sprite_disabled;

	private bool _state = true;

	private PowerButton _power_button;

	private void Awake()
	{
		_power_button = ((Component)this).GetComponent<PowerButton>();
	}

	public void setState(bool pState)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		_state = pState;
		if (_state)
		{
			_background.sprite = _sprite_enabled;
			((Graphic)_icon).color = Color.white;
		}
		else
		{
			_background.sprite = _sprite_disabled;
			((Graphic)_icon).color = Color32.op_Implicit(Toolbox.color_grey_dark);
		}
		((Behaviour)_button).enabled = _state;
		if ((Object)(object)_power_button != (Object)null)
		{
			_power_button.is_selectable = _state;
		}
	}
}
