using System;
using UnityEngine;

[Serializable]
public class ArchitectMood : Asset, ILocalizedAsset
{
	public string color_main;

	public string color_text;

	public string path_icon;

	private Color _cached_color;

	private Color _cached_color_text;

	private Sprite _cached_sprite;

	public Sprite getSprite()
	{
		if ((Object)(object)_cached_sprite == (Object)null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cached_sprite;
	}

	public string getLocaleID()
	{
		return "architect_mood_" + id;
	}

	public Color getColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_cached_color == Color.clear)
		{
			_cached_color = Toolbox.makeColor(color_main);
		}
		return _cached_color;
	}

	public Color getColorText()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_cached_color_text == Color.clear)
		{
			_cached_color_text = Toolbox.makeColor(color_text);
		}
		return _cached_color_text;
	}

	public ArchitectMood()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_cached_color = Color.clear;
		_cached_color_text = Color.clear;
		base._002Ector();
	}
}
