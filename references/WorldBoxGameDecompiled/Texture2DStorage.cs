using System.Collections.Generic;
using UnityEngine;

public static class Texture2DStorage
{
	private static Dictionary<string, SpritePool> pools = new Dictionary<string, SpritePool>();

	private static Dictionary<string, Texture2D> prefabs = new Dictionary<string, Texture2D>();

	internal static Sprite getSprite(int pW, int pH)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		string text = pW + "_" + pH;
		if (pools.ContainsKey(text))
		{
			SpritePool spritePool = pools[text];
			if (spritePool.list.Count > 0)
			{
				Sprite result = spritePool.list[spritePool.list.Count - 1];
				spritePool.list.RemoveAt(spritePool.list.Count - 1);
				return result;
			}
		}
		if (!prefabs.ContainsKey(text))
		{
			Texture2D val = new Texture2D(pW, pH, (TextureFormat)4, false)
			{
				filterMode = (FilterMode)0
			};
			((Object)val).name = "Texture2DStorage_" + text;
			prefabs.Add(text, val);
		}
		return Sprite.Create(Object.Instantiate<Texture2D>(prefabs[text]), new Rect(0f, 0f, (float)pW, (float)pH), new Vector2(0f, 0f), 1f);
	}

	internal static void addToStorage(Sprite pSprite, int pW, int pH)
	{
		string key = pW + "_" + pH;
		SpritePool spritePool;
		if (pools.ContainsKey(key))
		{
			spritePool = pools[key];
		}
		else
		{
			spritePool = new SpritePool();
			pools.Add(key, spritePool);
		}
		spritePool.list.Add(pSprite);
	}
}
