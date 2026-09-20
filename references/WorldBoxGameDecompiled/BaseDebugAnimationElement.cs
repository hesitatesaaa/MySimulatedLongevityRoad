using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseDebugAnimationElement<TAsset> : MonoBehaviour where TAsset : Asset
{
	protected TAsset asset;

	public Button play_pause_button;

	public Image play_pause_icon;

	public Sprite sprite_play;

	public Sprite sprite_pause;

	public Button frame_number_button;

	public Text frame_number_text;

	protected bool is_playing;

	protected virtual void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		((UnityEvent)play_pause_button.onClick).AddListener(new UnityAction(clickToggleState));
		((UnityEvent)frame_number_button.onClick).AddListener(new UnityAction(clickNextFrame));
	}

	public virtual void update()
	{
		throw new NotImplementedException();
	}

	public virtual void setData(TAsset pAsset)
	{
		asset = pAsset;
		clear();
		is_playing = true;
	}

	protected virtual void clear()
	{
		throw new NotImplementedException();
	}

	public virtual void stopAnimations()
	{
		is_playing = false;
		checkButtons();
	}

	public virtual void startAnimations()
	{
		is_playing = true;
		checkButtons();
	}

	private void clickToggleState()
	{
		is_playing = !is_playing;
		if (is_playing)
		{
			startAnimations();
		}
		else
		{
			stopAnimations();
		}
	}

	private void checkButtons()
	{
		if (is_playing)
		{
			play_pause_icon.sprite = sprite_pause;
			((Selectable)frame_number_button).interactable = false;
		}
		else
		{
			play_pause_icon.sprite = sprite_play;
			((Selectable)frame_number_button).interactable = true;
		}
	}

	protected virtual void clickNextFrame()
	{
		throw new NotImplementedException();
	}
}
