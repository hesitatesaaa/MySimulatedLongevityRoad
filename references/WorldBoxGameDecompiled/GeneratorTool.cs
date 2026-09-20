using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GeneratorTool : ScriptableObject
{
	private static WorldTile[,] _tiles_map;

	private static Texture2D[] _textures;

	private static List<WorldTile> _neighbours = new List<WorldTile>(4);

	private static List<WorldTile> _neighbours_all = new List<WorldTile>(8);

	internal static void Setup(WorldTile[,] pTilesMap)
	{
		_tiles_map = pTilesMap;
	}

	public static void Init()
	{
		LoadGenShapeTextures();
	}

	internal static void applyTemplate(string pID, float pMod = 1f)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Texture2D random = Resources.LoadAll<Texture2D>("map_gen/" + pID).GetRandom();
		random = TextureRotator.Rotate(random, Randy.randomInt(0, 360), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		TextureScale.Bilinear(random, MapBox.width, MapBox.height);
		float num = 255f * pMod;
		for (int i = 0; i < ((Texture)random).width; i++)
		{
			for (int j = 0; j < ((Texture)random).height; j++)
			{
				WorldTile tile = World.world.GetTile(i, j);
				if (tile != null)
				{
					random.GetPixel(i, j);
					int num2 = (int)((1f - random.GetPixel(i, j).g) * num);
					tile.Height += num2;
				}
			}
		}
	}

	internal static void ApplyRandomShape(string pWhat = "height", float tDistMax = 2f, float pMod = 0.7f, bool pSubtract = false)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = null;
		int num = 0;
		int num2 = 0;
		val = Object.Instantiate<Texture2D>(_textures.GetRandom());
		((Object)val).name = "random_shape";
		num = (int)((float)((Texture)val).width * Randy.randomFloat(0.3f, 2f));
		num2 = (int)((float)((Texture)val).height * Randy.randomFloat(0.3f, 2f));
		val = TextureRotator.Rotate(val, Randy.randomInt(0, 360), new Color32((byte)0, (byte)0, (byte)0, (byte)0));
		TextureScale.Bilinear(val, num, num2);
		num = ((Texture)val).width;
		num2 = ((Texture)val).height;
		int num3 = MapBox.width / 2 - num / 2 - (int)Randy.randomFloat((float)(-num) * tDistMax, (float)num * tDistMax);
		int num4 = MapBox.height / 2 - num2 / 2 - (int)Randy.randomFloat((float)(-num2) * tDistMax, (float)num2 * tDistMax);
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num4 < 0)
		{
			num4 = 0;
		}
		if (num3 + num > MapBox.width)
		{
			num3 = MapBox.width - num;
		}
		if (num4 + num2 > MapBox.height)
		{
			num4 = MapBox.height - num2;
		}
		float num5 = 255f * pMod;
		for (int i = 0; i < ((Texture)val).width; i++)
		{
			for (int j = 0; j < ((Texture)val).height; j++)
			{
				WorldTile tile = World.world.GetTile(num3 + i, num4 + j);
				if (tile != null)
				{
					int num6 = (int)(val.GetPixel(i, j).a * num5);
					if (pSubtract)
					{
						num6 = -num6;
					}
					tile.Height += num6;
				}
			}
		}
		Object.Destroy((Object)(object)val);
	}

	private static void LoadGenShapeTextures()
	{
		if (_textures == null)
		{
			_textures = Resources.LoadAll<Texture2D>("gen_shapes");
		}
	}

	public static void ApplyWaterLevel(WorldTile[,] tilesMap, int width, int height, int pVal)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				tilesMap[i, j].Height -= pVal;
			}
		}
	}

	public static void ApplyPerlinNoise(WorldTile[,] tilesMap, int width, int height, float pPosX, float pPosY, float pAlphaMod, float pScaleMod, bool pSubtract = false, GeneratorTarget pTarget = GeneratorTarget.Height)
	{
		float num = 255f * pAlphaMod;
		float num2 = 1f;
		float num3 = 1f;
		if (width > height)
		{
			num2 = width / height;
		}
		else
		{
			num3 = height / width;
		}
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				float num4 = (pPosX + (float)i) / (float)width;
				float num5 = (pPosY + (float)j) / (float)height;
				int num6 = (int)(Mathf.PerlinNoise(num4 * pScaleMod * num2, num5 * pScaleMod * num3) * num);
				if (pSubtract)
				{
					num6 = -num6;
				}
				if (pTarget == GeneratorTarget.Height)
				{
					tilesMap[i, j].Height += num6;
				}
			}
		}
	}

	public static void ApplyPerlinReplace(PerlinReplaceContainer pContainer)
	{
		float num = Randy.randomInt(0, 15000);
		float num2 = Randy.randomInt(0, 15000);
		int width = MapBox.width;
		int height = MapBox.height;
		float num3 = pContainer.scale;
		float num4 = 255f;
		float num5 = 1f;
		float num6 = 1f;
		if (width > height)
		{
			num5 = width / height;
		}
		else
		{
			num6 = height / width;
		}
		WorldTile[,] tiles_map = _tiles_map;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				WorldTile worldTile = tiles_map[i, j];
				float num7 = (num + (float)i) / (float)width;
				float num8 = (num2 + (float)j) / (float)height;
				int num9 = (int)(Mathf.PerlinNoise(num7 * num3 * num5, num8 * num3 * num6) * num4);
				for (int k = 0; k < pContainer.options.Count; k++)
				{
					PerlinReplaceOption perlinReplaceOption = pContainer.options[k];
					if (num9 > perlinReplaceOption.replace_height_value && worldTile.main_type.IsType(perlinReplaceOption.from))
					{
						worldTile.setTileType(perlinReplaceOption.to);
					}
				}
			}
		}
	}

	public static void UpdateTileTypes(bool pGeneratorStage = false, int pStartIndex = 0, int pAmount = 0)
	{
		int num = pStartIndex + pAmount;
		for (int i = pStartIndex; i < num; i++)
		{
			WorldTile worldTile = World.world.tiles_list[i];
			TileType typeByDepth = AssetManager.tiles.getTypeByDepth(worldTile);
			worldTile.setTileType(typeByDepth);
		}
	}

	public static void GenerateTileNeighbours(WorldTile[] pTilesList)
	{
		int num = pTilesList.Length;
		for (int i = 0; i < num; i++)
		{
			generateTileNeighbours(pTilesList[i]);
		}
	}

	public static void generateTileNeighbours(WorldTile pTile)
	{
		WorldTile tile = getTile(pTile.x - 1, pTile.y);
		pTile.addNeighbour(tile, TileDirection.Left, _neighbours, _neighbours_all);
		tile = getTile(pTile.x + 1, pTile.y);
		pTile.addNeighbour(tile, TileDirection.Right, _neighbours, _neighbours_all);
		tile = getTile(pTile.x, pTile.y - 1);
		pTile.addNeighbour(tile, TileDirection.Down, _neighbours, _neighbours_all);
		tile = getTile(pTile.x, pTile.y + 1);
		pTile.addNeighbour(tile, TileDirection.Up, _neighbours, _neighbours_all);
		tile = getTile(pTile.x - 1, pTile.y - 1);
		pTile.addNeighbour(tile, TileDirection.Null, _neighbours, _neighbours_all, pDiagonal: true);
		tile = getTile(pTile.x - 1, pTile.y + 1);
		pTile.addNeighbour(tile, TileDirection.Null, _neighbours, _neighbours_all, pDiagonal: true);
		tile = getTile(pTile.x + 1, pTile.y - 1);
		pTile.addNeighbour(tile, TileDirection.Null, _neighbours, _neighbours_all, pDiagonal: true);
		tile = getTile(pTile.x + 1, pTile.y + 1);
		pTile.addNeighbour(tile, TileDirection.Null, _neighbours, _neighbours_all, pDiagonal: true);
		pTile.neighbours = _neighbours.ToArray();
		pTile.neighboursAll = _neighbours_all.ToArray();
		_neighbours.Clear();
		_neighbours_all.Clear();
	}

	public static void ApplyRingEffect()
	{
		WorldTile[,] tiles_map = _tiles_map;
		for (int i = 0; i < MapBox.width; i++)
		{
			for (int j = 0; j < MapBox.height; j++)
			{
				for (int k = 0; k < AssetManager.tiles.list.Count; k++)
				{
					TileType tileType = AssetManager.tiles.list[k];
					if (tileType.additional_height == null)
					{
						continue;
					}
					bool flag = false;
					for (int l = 0; l < tileType.additional_height.Length; l++)
					{
						WorldTile worldTile = tiles_map[i, j];
						if (worldTile.Height == tileType.height_min - tileType.additional_height[l])
						{
							worldTile.Height = tileType.height_min;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static WorldTile getTile(int pX, int pY)
	{
		if (pX < 0 || pX >= MapBox.width)
		{
			return null;
		}
		if (pY < 0 || pY >= MapBox.height)
		{
			return null;
		}
		return _tiles_map[pX, pY];
	}
}
