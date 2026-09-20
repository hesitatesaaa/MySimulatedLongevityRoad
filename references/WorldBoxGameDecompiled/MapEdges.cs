using UnityEngine;

public class MapEdges
{
	private static Texture2D textureLeft;

	private static Texture2D textureRight;

	private static Texture2D textureUp;

	private static Texture2D textureDown;

	private static Texture2D textureTempUp;

	private static Texture2D textureTempDown;

	private static int edgeSize;

	internal static void AddEdgeGradientCircle(WorldTile[,] pMap, string pWhat)
	{
		WorldTile pT = pMap[MapBox.width / 2, MapBox.height / 2];
		float num = 0.99f;
		float num2 = 0.85f;
		float num3 = (float)(MapBox.width / 2) * num;
		float num4 = (float)(MapBox.width / 2) * num2;
		float num5 = num3 - num4;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			float num6 = Toolbox.DistTile(worldTile, pT);
			if (num6 > num3)
			{
				worldTile.Height = 0;
			}
			else if (num6 < num3 && num6 > num4)
			{
				float num7 = (num3 - num6) / num5;
				int height = (int)((float)worldTile.Height * num7);
				worldTile.Height = height;
			}
		}
	}

	internal static void AddEdgeSquare(WorldTile[,] pMap, string pWhat)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		edgeSize = 64;
		if ((Object)(object)textureLeft == (Object)null)
		{
			textureLeft = (Texture2D)Resources.Load("edges/edge100xLeft");
			textureRight = (Texture2D)Resources.Load("edges/edge100xRight");
			textureUp = (Texture2D)Resources.Load("edges/edge100xUp");
			textureDown = (Texture2D)Resources.Load("edges/edge100xDown");
			textureTempUp = (Texture2D)Resources.Load("edges/edgeTempUp");
			textureTempDown = (Texture2D)Resources.Load("edges/edgeTempDown");
		}
		int num = (int)((float)MapBox.width / (float)edgeSize) + 1;
		int num2 = (int)((float)MapBox.height / (float)edgeSize) + 1;
		if (pWhat == "temperature")
		{
			for (int i = 0; i < num; i++)
			{
				fill(i, 0, textureTempDown, pMap, pWhat);
			}
			for (int j = 0; j < num; j++)
			{
				fill(j, num2 - 2, textureTempUp, pMap, pWhat);
			}
			return;
		}
		for (int k = 0; k < num2; k++)
		{
			fill(0, k, textureLeft, pMap, pWhat);
		}
		for (int l = 0; l < num2; l++)
		{
			fill(num - 2, l, textureRight, pMap, pWhat);
		}
		for (int m = 0; m < num; m++)
		{
			fill(m, 0, textureDown, pMap, pWhat);
		}
		for (int n = 0; n < num; n++)
		{
			fill(n, num2 - 2, textureUp, pMap, pWhat);
		}
	}

	internal static void fill(int pX, int pY, Texture2D pTexture, WorldTile[,] tilesMap, string pWhat)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Texture)pTexture).height; i++)
		{
			for (int j = 0; j < ((Texture)pTexture).width; j++)
			{
				int num = (int)(pTexture.GetPixel(j, i).a * 255f);
				int num2 = j + pX * edgeSize;
				int num3 = i + pY * edgeSize;
				if (num2 < MapBox.width && num3 < MapBox.height)
				{
					WorldTile worldTile = tilesMap[num2, num3];
					if (worldTile != null && pWhat == "height")
					{
						worldTile.Height -= num;
					}
				}
			}
		}
	}
}
