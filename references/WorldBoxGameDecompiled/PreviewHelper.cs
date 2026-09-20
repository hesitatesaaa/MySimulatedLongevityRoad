using System.IO;
using UnityEngine;

public static class PreviewHelper
{
	public static Sprite loadWorkshopMapPreview()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		string text = SaveManager.generatePngPreviewPath(SaveManager.currentWorkshopMapData.main_path);
		if (string.IsNullOrEmpty(text) || !File.Exists(text))
		{
			return null;
		}
		byte[] array = File.ReadAllBytes(text);
		Texture2D val = new Texture2D(64, 64);
		if (ImageConversion.LoadImage(val, array))
		{
			return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
		}
		return null;
	}

	public static Sprite getCurrentWorldPreview()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		World.world.redrawMiniMap(pForce: true);
		Texture2D val = Toolbox.ScaleTexture(World.world.world_layer.texture, 512, 512);
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0f, 0f));
	}

	public static Texture2D convertMapToTexture()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		Texture2D texture = World.world.world_layer.texture;
		Texture2D val = new Texture2D(((Texture)texture).width, ((Texture)texture).height);
		Color32[] pixels = texture.GetPixels32();
		val.SetPixels32(pixels);
		val.Apply();
		return val;
	}

	public static int getMaxAdSlots()
	{
		int num = 1;
		if (World.world.game_stats.data.gameLaunches > 10 && World.world.game_stats.data.gameTime > 36000.0)
		{
			num = 3;
		}
		if (World.world.game_stats.data.gameLaunches > 30 && World.world.game_stats.data.gameTime > 72000.0)
		{
			num = 6;
		}
		for (int i = num + 1; i <= 6; i++)
		{
			if (SaveManager.slotExists(i))
			{
				return 6;
			}
		}
		return num;
	}
}
