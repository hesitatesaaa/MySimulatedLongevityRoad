using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WindowMetaTab : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvas_group;

	public List<Transform> tab_elements = new List<Transform>();

	public WindowMetaTabEvent tab_action;

	internal WindowMetaTabButtonsContainer container;

	internal bool destroyed;

	private TipButton _tip_button;

	private string _worldtip_text;

	private bool _state = true;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			doAction();
		});
		_tip_button = ((Component)this).GetComponent<TipButton>();
		_worldtip_text = getWorldTipText();
		_tip_button.setHoverAction(checkShowTooltip);
	}

	public void doAction()
	{
		((UnityEvent<WindowMetaTab>)tab_action).Invoke(this);
		checkShowWorldTip();
	}

	public void checkShowWorldTip()
	{
		if (!((Object)(object)_tip_button == (Object)null) && !InputHelpers.mouseSupported)
		{
			WorldTip.showNowTop(_worldtip_text, pTranslate: false);
		}
	}

	private void checkShowTooltip()
	{
		if (InputHelpers.mouseSupported)
		{
			Tooltip.show(this, "tip", new TooltipData
			{
				tip_name = _tip_button.textOnClick,
				tip_description = _tip_button.textOnClickDescription,
				tip_description_2 = _tip_button.text_description_2
			});
		}
	}

	private void OnDestroy()
	{
		destroyed = true;
		if (((Component)this).gameObject.HasComponent<PlatformRemover>())
		{
			container.removeTab(this);
		}
	}

	public bool getState()
	{
		return _state;
	}

	public void toggleActive(bool pState)
	{
		_state = pState;
		if (_state)
		{
			_canvas_group.alpha = 1f;
		}
		else
		{
			_canvas_group.alpha = 0f;
		}
		_canvas_group.interactable = _state;
		_canvas_group.blocksRaycasts = _state;
	}

	public string getWorldTipText()
	{
		string text = LocalizedTextManager.getText(_tip_button.textOnClick);
		if (!string.IsNullOrEmpty(_tip_button.textOnClickDescription))
		{
			text = text + "\n<size=9>" + LocalizedTextManager.getText(_tip_button.textOnClickDescription) + "</size>";
		}
		return text;
	}
}
