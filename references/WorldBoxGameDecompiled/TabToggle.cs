using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabToggle : MonoBehaviour
{
	public Image icon;

	public Image background;

	private TabToggleState _state;

	public TabToggleAction action;

	public TabToggleAction post_action;

	public TabToggleClearAction select_action;

	protected static Sprite _tab_toggle_on;

	protected static Sprite _tab_toggle_off;

	private void Awake()
	{
		if ((Object)(object)_tab_toggle_on == (Object)null)
		{
			_tab_toggle_on = SpriteTextureLoader.getSprite("ui/tab_button_sort_selected");
			_tab_toggle_off = SpriteTextureLoader.getSprite("ui/tab_button_sort");
		}
		unselect();
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(click));
	}

	public TabToggleState getState()
	{
		return _state;
	}

	private void setState(TabToggleState pState)
	{
		_state = pState;
	}

	public void click()
	{
		if (_state != TabToggleState.Selected)
		{
			select_action?.Invoke(this);
			select();
			action?.Invoke();
			post_action?.Invoke();
		}
	}

	public void select()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		setState(TabToggleState.Selected);
		background.sprite = _tab_toggle_on;
		((Graphic)icon).color = Color.white;
	}

	public void unselect()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		setState(TabToggleState.None);
		background.sprite = _tab_toggle_off;
		Color white = Color.white;
		white.a = 0.5f;
		((Graphic)icon).color = white;
	}
}
