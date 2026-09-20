using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SortButton : MonoBehaviour
{
	public Image arrow_sprite;

	public Image icon;

	public Image background;

	private SortButtonState _state;

	public SortButtonAction action;

	public SortButtonAction post_action;

	public SortButtonClearAction select_action;

	protected static Sprite _tab_button_on;

	protected static Sprite _tab_button_off;

	private void Awake()
	{
		if ((Object)(object)_tab_button_on == (Object)null)
		{
			_tab_button_on = SpriteTextureLoader.getSprite("ui/tab_button_sort_selected");
			_tab_button_off = SpriteTextureLoader.getSprite("ui/tab_button_sort");
		}
		((Component)arrow_sprite).gameObject.SetActive(false);
		setState(SortButtonState.None);
		background.sprite = _tab_button_off;
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(click));
	}

	public SortButtonState getState()
	{
		return _state;
	}

	private void setState(SortButtonState pState)
	{
		_state = pState;
	}

	internal void turnOff()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		setState(SortButtonState.None);
		((Component)arrow_sprite).gameObject.SetActive(false);
		background.sprite = _tab_button_off;
		Color white = Color.white;
		white.a = 0.5f;
		((Graphic)icon).color = white;
		((Component)((Component)this).transform.parent).GetComponent<RectTransform>().sizeDelta = new Vector2(27f, 37f);
	}

	public void click()
	{
		select_action?.Invoke(this);
		switch (_state)
		{
		case SortButtonState.None:
			setSortUP();
			break;
		case SortButtonState.Up:
			setSortDOWN();
			break;
		case SortButtonState.Down:
			setSortUP();
			break;
		}
		action?.Invoke();
		post_action?.Invoke();
	}

	public void callAction()
	{
	}

	public void setSortUP()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		setState(SortButtonState.Up);
		((Component)arrow_sprite).gameObject.SetActive(true);
		arrow_sprite.sprite = SpriteTextureLoader.getSprite("ui/Icons/iconArrowUP");
		background.sprite = _tab_button_on;
		((Graphic)icon).color = Color.white;
		((Component)((Component)this).transform.parent).GetComponent<RectTransform>().sizeDelta = new Vector2(33f, 37f);
	}

	public void setSortDOWN()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		setState(SortButtonState.Down);
		((Component)arrow_sprite).gameObject.SetActive(true);
		arrow_sprite.sprite = SpriteTextureLoader.getSprite("ui/Icons/iconArrowDOWN");
		background.sprite = _tab_button_on;
		((Graphic)icon).color = Color.white;
		((Component)((Component)this).transform.parent).GetComponent<RectTransform>().sizeDelta = new Vector2(33f, 37f);
	}
}
