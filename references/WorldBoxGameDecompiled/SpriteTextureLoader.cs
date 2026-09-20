using System.Collections.Generic;
using UnityEngine;

public static class SpriteTextureLoader
{
	private static readonly Dictionary<string, Sprite> _cached_sprites = new Dictionary<string, Sprite>();

	private static readonly Dictionary<string, Sprite[]> _cached_sprite_list = new Dictionary<string, Sprite[]>();

	private static int _total_sprite_list_single_sprites = 0;

	public static int total_sprites => _cached_sprites.Count;

	public static int total_sprites_list => _cached_sprite_list.Count;

	public static int total_sprites_list_single_sprites => _total_sprite_list_single_sprites;

	public static Sprite getSprite(string pPath)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (!_cached_sprites.TryGetValue(pPath, out var value))
		{
			value = (Sprite)Resources.Load(pPath, typeof(Sprite));
			_cached_sprites[pPath] = value;
		}
		return value;
	}

	public static Sprite[] getSpriteList(string pPath, bool pSkipIfEmpty = false)
	{
		if (!_cached_sprite_list.TryGetValue(pPath, out var value))
		{
			value = Resources.LoadAll<Sprite>(pPath);
			if (pSkipIfEmpty && value.Length == 0)
			{
				return null;
			}
			_cached_sprite_list.Add(pPath, value);
			_total_sprite_list_single_sprites += value.Length;
		}
		return value;
	}

	public static void addSprite(string pPathID, byte[] pBytes)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(1, 1);
		((Texture)val).filterMode = (FilterMode)0;
		if (ImageConversion.LoadImage(val, pBytes))
		{
			Rect val2 = default(Rect);
			((Rect)(ref val2))._002Ector(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height);
			Vector2 val3 = default(Vector2);
			((Vector2)(ref val3))._002Ector(0.5f, 0.5f);
			Sprite value = Sprite.Create(val, val2, val3, 1f);
			_cached_sprites.Add(pPathID, value);
		}
	}
}
