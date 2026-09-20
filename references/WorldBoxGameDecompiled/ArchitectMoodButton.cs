using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArchitectMoodButton : MonoBehaviour
{
	[SerializeField]
	protected Button button;

	[SerializeField]
	protected TipButton _tip_button;

	[SerializeField]
	protected Image _icon;

	[SerializeField]
	private Image _selected;

	private ArchitectMood _asset;

	private ArchitectMoodAction _click_callback;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)button.onClick).AddListener((UnityAction)delegate
		{
			_click_callback?.Invoke(this);
		});
	}

	public ArchitectMood getAsset()
	{
		return _asset;
	}

	public virtual void setAsset(ArchitectMood pAsset)
	{
		_asset = pAsset;
		_icon.sprite = _asset.getSprite();
		_tip_button.textOnClick = pAsset.getLocaleID();
	}

	public void toggleSelectedButton(bool pState)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_selected != (Object)null)
		{
			((Graphic)_selected).color = Toolbox.makeColor(_asset.color_main);
			((Behaviour)_selected).enabled = pState;
		}
	}

	public void setIconActiveColor(bool pState)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!pState) ? 0.55f : 1f);
		Color color = default(Color);
		((Color)(ref color))._002Ector(num, num, num);
		((Graphic)_icon).color = color;
	}

	public void addClickCallback(ArchitectMoodAction pAction)
	{
		_click_callback = (ArchitectMoodAction)Delegate.Combine(_click_callback, pAction);
	}
}
