using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
	[SerializeField]
	private Image _background;

	private ToggleButtonSelectAction _action;

	private ToggleButtonAction _post_action;

	private static Sprite _sprite_on;

	private static Sprite _sprite_off;

	public bool is_on;

	private void Awake()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		if ((Object)(object)_sprite_on == (Object)null)
		{
			_sprite_on = SpriteTextureLoader.getSprite("ui/tab_button_sort_selected");
			_sprite_off = SpriteTextureLoader.getSprite("ui/tab_button_sort");
		}
		_background.sprite = _sprite_off;
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(click));
	}

	public void init(string pIcon, string pTooltip, ToggleButtonSelectAction pAction, ToggleButtonAction pShowAction)
	{
		PowerButton component = ((Component)this).GetComponent<PowerButton>();
		component.icon.sprite = SpriteTextureLoader.getSprite(pIcon);
		((Component)component).GetComponent<TipButton>().textOnClick = pTooltip;
		_action = pAction;
		_post_action = pShowAction;
		((Object)((Component)this).gameObject).name = pTooltip;
	}

	public void click()
	{
		is_on = !is_on;
		checkSprite();
		_action?.Invoke(this);
		_post_action?.Invoke();
	}

	private void checkSprite()
	{
		_background.sprite = (is_on ? _sprite_on : _sprite_off);
	}
}
