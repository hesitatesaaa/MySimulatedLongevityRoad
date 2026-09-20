using System.Collections.Generic;
using UnityEngine;

public static class DynamicSpriteCreator
{
	public static Actor debug_actor;

	private static Dictionary<Sprite, int> _int_ids_body;

	private static readonly Color32 _placeholder_color_skin;

	private static readonly List<Vector2Int> _light_colors;

	public static Sprite createNewItemSprite(DynamicSpritesAsset pAsset, Sprite pSource, ColorAsset pKingdomColor)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pAsset.getAtlas();
		Rect rect = pSource.rect;
		int pWidth = (int)((Rect)(ref rect)).width;
		int pHeight = (int)((Rect)(ref rect)).height;
		atlas.checkBounds(pWidth, pHeight);
		int width = ((Texture)atlas.texture).width;
		_ = ((Texture)atlas.texture).height;
		Color32[] pixels = pSource.texture.GetPixels32();
		int width2 = ((Texture)pSource.texture).width;
		for (int i = 0; (float)i < ((Rect)(ref rect)).width; i++)
		{
			for (int j = 0; (float)j < ((Rect)(ref rect)).height; j++)
			{
				int num = i + (int)((Rect)(ref rect)).x;
				int num2 = j + (int)((Rect)(ref rect)).y;
				int num3 = num + num2 * width2;
				Color32 val = pixels[num3];
				if (val.a != 0)
				{
					val = DynamicColorPixelTool.checkSpecialColors(val, pKingdomColor, pCheckForLightColors: true);
					int num4 = i + atlas.last_x;
					int num5 = j + atlas.last_y;
					if (num4 < 0)
					{
						num4 = 0;
					}
					if (num5 < 0)
					{
						num5 = 0;
					}
					num3 = num4 + num5 * width;
					atlas.pixels[num3] = val;
				}
			}
		}
		setAtlasDirty(atlas);
		return createFinalSprite(atlas, pSource, pWidth, pHeight);
	}

	private static Sprite createFinalSprite(UnitSpriteConstructorAtlas pAtlasTexture, Sprite pMain, int pWidth, int pHeight, int pResizeX = 0, int pResizeY = 0)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector((float)pAtlasTexture.last_x, (float)pAtlasTexture.last_y, (float)pWidth, (float)pHeight);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((pMain.pivot.x + (float)pResizeX) / (float)pWidth, pMain.pivot.y / (float)pHeight);
		Sprite obj = Sprite.Create(pAtlasTexture.texture, val, val2, 1f);
		((Object)obj).name = "gen_" + ((Object)pMain).name;
		pAtlasTexture.last_x += pWidth + 1;
		return obj;
	}

	private static Sprite createNewSpriteBuildingShadow(DynamicSpritesAsset pDynamicSpritesAsset, BuildingAsset tAsset, Sprite pSource, bool pIsContructionSprite)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pDynamicSpritesAsset.getAtlas();
		Rect rect = pSource.rect;
		int num = 3;
		int num2 = (int)((Rect)(ref rect)).width;
		int num3 = (int)((Rect)(ref rect)).height;
		int num4 = (int)((Rect)(ref rect)).x;
		int num5 = (int)((Rect)(ref rect)).y;
		atlas.checkBounds(num2 + num, num3);
		int width = ((Texture)atlas.texture).width;
		_ = ((Texture)atlas.texture).height;
		Color32[] pixels = pSource.texture.GetPixels32();
		Vector2 val;
		float num6;
		if (pIsContructionSprite)
		{
			val = BuildingLibrary.shadow_under_construction_bound;
			num6 = BuildingLibrary.shadow_under_construction_distortion;
		}
		else
		{
			val = tAsset.shadow_bound;
			num6 = tAsset.shadow_distortion;
		}
		int num7 = (int)(val.x * (float)num2);
		int num8 = (int)((float)num3 * val.y);
		List<Vector2Int> list = new List<Vector2Int>();
		Color32 val2 = Color32.op_Implicit(Color.black);
		int width2 = ((Texture)pSource.texture).width;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				int num9 = i + num4;
				int num10 = j + num5;
				int num11 = num9 + num10 * width2;
				Color32 val3 = pixels[num11];
				if (val3.a == 0)
				{
					continue;
				}
				val3 = val2;
				if (i >= num7)
				{
					int num12 = i + atlas.last_x;
					int num13 = j + atlas.last_y;
					if (j > num8)
					{
						num13 = (int)((float)j * num6) + atlas.last_y;
					}
					if (num12 < 0)
					{
						num12 = 0;
					}
					if (num13 < 0)
					{
						num13 = 0;
					}
					list.Add(new Vector2Int(num12, num13));
					num11 = num12 + num13 * width;
					atlas.pixels[num11] = val3;
				}
			}
		}
		setAtlasDirty(atlas);
		num2 += num;
		foreach (Vector2Int item in list)
		{
			Vector2Int current = item;
			int num14 = ((Vector2Int)(ref current)).x + 1;
			int y = ((Vector2Int)(ref current)).y;
			int num15 = num14 + y * width;
			atlas.pixels[num15] = val2;
			int num16 = ((Vector2Int)(ref current)).x + 2;
			y = ((Vector2Int)(ref current)).y;
			num15 = num16 + y * width;
			atlas.pixels[num15] = val2;
			int num17 = ((Vector2Int)(ref current)).x + 1;
			y = ((Vector2Int)(ref current)).y + 1;
			num15 = num17 + y * width;
			atlas.pixels[num15] = val2;
		}
		return createFinalSprite(atlas, pSource, num2, num3);
	}

	public static Sprite createNewUnitShadow(DynamicSpritesAsset pAsset, Sprite pSource)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pAsset.getAtlas();
		Rect rect = pSource.rect;
		int num = 1;
		int num2 = (int)((Rect)(ref rect)).width;
		int num3 = (int)((Rect)(ref rect)).height;
		int num4 = (int)((Rect)(ref rect)).x;
		int num5 = (int)((Rect)(ref rect)).y;
		atlas.checkBounds(num2 + num, num3);
		int width = ((Texture)atlas.texture).width;
		_ = ((Texture)atlas.texture).height;
		Color32[] pixels = pSource.texture.GetPixels32();
		int width2 = ((Texture)pSource.texture).width;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				int num6 = i + num4;
				int num7 = j + num5;
				int num8 = num6 + num7 * width2;
				Color32 val = pixels[num8];
				if (val.a != 0)
				{
					int num9 = i + atlas.last_x;
					int num10 = j + atlas.last_y;
					if (num9 < 0)
					{
						num9 = 0;
					}
					if (num10 < 0)
					{
						num10 = 0;
					}
					num8 = num9 + num10 * width;
					atlas.pixels[num8] = val;
				}
			}
		}
		num2 += num;
		setAtlasDirty(atlas);
		return createFinalSprite(atlas, pSource, num2, num3);
	}

	public static void createBuildingShadow(BuildingAsset pAsset, Sprite pSprite, bool pIsContructionSprite)
	{
		DynamicSpritesAsset building_shadows = DynamicSpritesLibrary.building_shadows;
		int hashCode = ((object)pSprite).GetHashCode();
		Sprite pSprite2 = createNewSpriteBuildingShadow(building_shadows, pAsset, pSprite, pIsContructionSprite);
		building_shadows.addSprite(hashCode, pSprite2);
	}

	public static Sprite createNewIcon(DynamicSpritesAsset pAsset, Sprite pSource, ColorAsset pKingdomColor, PhenotypeAsset pPhenotype = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pAsset.getAtlas();
		if (pPhenotype != null)
		{
			DynamicColorPixelTool.loadSkinColorsPreview(pPhenotype, 0);
		}
		Rect rect = pSource.rect;
		int pWidth = (int)((Rect)(ref rect)).width;
		int pHeight = (int)((Rect)(ref rect)).height;
		atlas.checkBounds(pWidth, pHeight);
		int width = ((Texture)atlas.texture).width;
		_ = ((Texture)atlas.texture).height;
		Color32[] pixels = pSource.texture.GetPixels32();
		int width2 = ((Texture)pSource.texture).width;
		for (int i = 0; (float)i < ((Rect)(ref rect)).width; i++)
		{
			for (int j = 0; (float)j < ((Rect)(ref rect)).height; j++)
			{
				int num = i + (int)((Rect)(ref rect)).x;
				int num2 = j + (int)((Rect)(ref rect)).y;
				int num3 = num + num2 * width2;
				Color32 val = pixels[num3];
				if (val.a != 0)
				{
					val = DynamicColorPixelTool.checkSpecialColors(val, pKingdomColor, pCheckForLightColors: true);
					int num4 = i + atlas.last_x;
					int num5 = j + atlas.last_y;
					if (num4 < 0)
					{
						num4 = 0;
					}
					if (num5 < 0)
					{
						num5 = 0;
					}
					num3 = num4 + num5 * width;
					atlas.pixels[num3] = val;
				}
			}
		}
		setAtlasDirty(atlas);
		return createFinalSprite(atlas, pSource, pWidth, pHeight);
	}

	public static Sprite createNewSpriteBuilding(DynamicSpritesAsset pAssetAtlas, long pID, Sprite pSource, ColorAsset pKingdomColor)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pAssetAtlas.getAtlas();
		Rect rect = pSource.rect;
		int pWidth = (int)((Rect)(ref rect)).width;
		int pHeight = (int)((Rect)(ref rect)).height;
		atlas.checkBounds(pWidth, pHeight);
		int width = ((Texture)atlas.texture).width;
		_ = ((Texture)atlas.texture).height;
		Color32[] pixels = pSource.texture.GetPixels32();
		_light_colors.Clear();
		int width2 = ((Texture)pSource.texture).width;
		for (int i = 0; (float)i < ((Rect)(ref rect)).width; i++)
		{
			for (int j = 0; (float)j < ((Rect)(ref rect)).height; j++)
			{
				int num = i + (int)((Rect)(ref rect)).x;
				int num2 = j + (int)((Rect)(ref rect)).y;
				int num3 = num + num2 * width2;
				Color32 val = pixels[num3];
				if (val.a != 0)
				{
					if (Toolbox.areColorsEqual(val, Toolbox.color_light))
					{
						_light_colors.Add(new Vector2Int(i, j));
					}
					val = DynamicColorPixelTool.checkSpecialColors(val, pKingdomColor, pCheckForLightColors: true);
					int num4 = i + atlas.last_x;
					int num5 = j + atlas.last_y;
					if (num4 < 0)
					{
						num4 = 0;
					}
					if (num5 < 0)
					{
						num5 = 0;
					}
					num3 = num4 + num5 * width;
					atlas.pixels[num3] = val;
				}
			}
		}
		setAtlasDirty(atlas);
		Sprite result = createFinalSprite(atlas, pSource, pWidth, pHeight);
		if (_light_colors.Count > 0)
		{
			checkBuildingLightSprite(DynamicSpritesLibrary.building_lights, ((object)pSource).GetHashCode(), pSource);
		}
		return result;
	}

	private static void checkBuildingLightSprite(DynamicSpritesAsset pQuantumAsset, long pHashcodeMainSprite, Sprite pSprite)
	{
		Sprite sprite = pQuantumAsset.getSprite(pHashcodeMainSprite);
		if (sprite == null)
		{
			sprite = createNewSpriteBuildingLight(pQuantumAsset, pSprite);
			pQuantumAsset.addSprite(pHashcodeMainSprite, sprite);
		}
	}

	public static Sprite createNewSpriteBuildingLight(DynamicSpritesAsset pAsset, Sprite pSource)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas atlas = pAsset.getAtlas();
		Rect rect = pSource.rect;
		int pWidth = (int)((Rect)(ref rect)).width;
		int pHeight = (int)((Rect)(ref rect)).height;
		atlas.checkBounds(pWidth, pHeight);
		int width = ((Texture)pSource.texture).width;
		for (int i = 0; i < _light_colors.Count; i++)
		{
			Vector2Int val = _light_colors[i];
			drawLightPixel(atlas, ((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y, pWidth, pHeight, width, Toolbox.color_light_100);
			drawLightPixel(atlas, ((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y - 1, pWidth, pHeight, width, Toolbox.color_light_10);
			drawLightPixel(atlas, ((Vector2Int)(ref val)).x - 1, ((Vector2Int)(ref val)).y, pWidth, pHeight, width, Toolbox.color_light_10);
			drawLightPixel(atlas, ((Vector2Int)(ref val)).x + 1, ((Vector2Int)(ref val)).y, pWidth, pHeight, width, Toolbox.color_light_10);
			drawLightPixel(atlas, ((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y + 1, pWidth, pHeight, width, Toolbox.color_light_10);
		}
		setAtlasDirty(atlas);
		return createFinalSprite(atlas, pSource, pWidth, pHeight);
	}

	private static void drawLightPixel(UnitSpriteConstructorAtlas pAtlas, int pColorCoordsX, int pColorCoordsY, int pWidth, int pHeight, int pBodyTextureWidth, Color32 pColor)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		int num = pColorCoordsX + pAtlas.last_x;
		int num2 = pColorCoordsY + pAtlas.last_y;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		int num3 = num + num2 * ((Texture)pAtlas.texture).width;
		if (pAtlas.pixels[num3].a < pColor.a)
		{
			pAtlas.pixels[num3] = pColor;
		}
	}

	public static Sprite createNewSpriteForDebug(Sprite pSpriteSource, ColorAsset pKingdomColor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = pSpriteSource.rect;
		int num = (int)((Rect)(ref rect)).width;
		int num2 = (int)((Rect)(ref rect)).height;
		Color32[] pixels = pSpriteSource.texture.GetPixels32();
		Texture2D val = new Texture2D(num, num2);
		((Texture)val).filterMode = (FilterMode)0;
		((Texture)val).wrapMode = (TextureWrapMode)1;
		Color32[] pixels2 = val.GetPixels32();
		int width = ((Texture)pSpriteSource.texture).width;
		for (int i = 0; (float)i < ((Rect)(ref rect)).width; i++)
		{
			for (int j = 0; (float)j < ((Rect)(ref rect)).height; j++)
			{
				int num3 = i + (int)((Rect)(ref rect)).x;
				int num4 = j + (int)((Rect)(ref rect)).y;
				int num5 = num3 + num4 * width;
				Color32 val2 = pixels[num5];
				if (val2.a == 0)
				{
					pixels2[num5] = val2;
					continue;
				}
				val2 = DynamicColorPixelTool.checkSpecialColors(val2, pKingdomColor, pCheckForLightColors: true);
				pixels2[num5] = val2;
			}
		}
		val.SetPixels32(pixels2);
		val.Apply();
		Sprite obj = Sprite.Create(val, rect, pSpriteSource.pivot, 1f);
		((Object)obj).name = "gen_" + ((Object)pSpriteSource).name;
		return obj;
	}

	public static Sprite createNewSpriteUnit(AnimationFrameData pFrameData, Sprite pSourceBody, Sprite pSourceHead, ColorAsset pKingdomColor, ActorAsset pAsset, int pPhenotypeIndex, int pPhenotypeShade, UnitTextureAtlasID pAtlasID)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		UnitSpriteConstructorAtlas unitSpriteConstructorAtlas = null;
		switch (pAtlasID)
		{
		case UnitTextureAtlasID.Units:
			unitSpriteConstructorAtlas = DynamicSpritesLibrary.units.getAtlas();
			break;
		case UnitTextureAtlasID.Boats:
			unitSpriteConstructorAtlas = DynamicSpritesLibrary.boats.getAtlas();
			break;
		}
		PixelBag pixelBag = PixelBagManager.getPixelBag(pSourceBody, pCheckPhenotypes: true);
		int texture_rect_width = pixelBag.texture_rect_width;
		int texture_rect_height = pixelBag.texture_rect_height;
		int num = 0;
		int num2 = 0;
		DynamicColorPixelTool.setPlaceholderSkinColor(_placeholder_color_skin);
		DynamicColorPixelTool.resetSkinColors();
		if (pPhenotypeIndex != 0)
		{
			DynamicColorPixelTool.loadPhenotype(pPhenotypeIndex, pPhenotypeShade);
		}
		if (pSourceHead != null && pFrameData != null)
		{
			Rect rect = pSourceHead.rect;
			Vector2 pos_head_new = pFrameData.pos_head_new;
			int num3 = (int)pos_head_new.y + (int)((Rect)(ref rect)).height - texture_rect_height;
			if (num3 > 0)
			{
				num2 = num3;
			}
			int num4 = (int)pos_head_new.x + (int)((Rect)(ref rect)).width - texture_rect_width;
			if (num4 > 0)
			{
				num = num4;
			}
			else if (pos_head_new.x < 0f)
			{
				num = -(int)pos_head_new.x;
			}
		}
		int num5 = num;
		int num6 = num2;
		texture_rect_width += num5;
		texture_rect_height += num6;
		unitSpriteConstructorAtlas.checkBounds(texture_rect_width, texture_rect_height);
		fillDebugColor(texture_rect_width, texture_rect_height, unitSpriteConstructorAtlas);
		bool dynamic_sprite_zombie = pAsset.dynamic_sprite_zombie;
		int num7 = num5 + unitSpriteConstructorAtlas.last_x;
		int last_y = unitSpriteConstructorAtlas.last_y;
		drawPixelsAll(pixelBag, unitSpriteConstructorAtlas, pKingdomColor, num7, last_y, dynamic_sprite_zombie, pAsset);
		if (pSourceHead != null && pFrameData != null)
		{
			PixelBag pixelBag2 = PixelBagManager.getPixelBag(pSourceHead, pCheckPhenotypes: true);
			Vector2 pos_head_new2 = pFrameData.pos_head_new;
			Vector2 pivot = pSourceHead.pivot;
			int num8 = (int)pos_head_new2.x - (int)pivot.x;
			int num9 = (int)pos_head_new2.y - (int)pivot.y;
			num7 += num8;
			last_y += num9;
			drawPixelsAll(pixelBag2, unitSpriteConstructorAtlas, pKingdomColor, num7, last_y, dynamic_sprite_zombie, pAsset, pHead: true);
		}
		setAtlasDirty(unitSpriteConstructorAtlas);
		return createFinalSprite(unitSpriteConstructorAtlas, pSourceBody, texture_rect_width, texture_rect_height, num5);
	}

	private static void fillDebugColor(int pWidth, int pHeight, UnitSpriteConstructorAtlas pAtlas)
	{
	}

	private static void drawPixelsAll(PixelBag pBag, UnitSpriteConstructorAtlas pAtlas, ColorAsset pKingdomColor, int pPartX, int pPartY, bool pDynamicZombie, ActorAsset pActorAsset, bool pHead = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		Color32[] pixels = pAtlas.pixels;
		int width = ((Texture)pAtlas.texture).width;
		drawPixels(pixels, width, pBag.arr_pixels_k1_0, pKingdomColor.k_color_0, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k1_1, pKingdomColor.k_color_1, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k1_2, pKingdomColor.k_color_2, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k1_3, pKingdomColor.k_color_3, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k1_4, pKingdomColor.k_color_4, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k2_0, pKingdomColor.k2_color_0, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k2_1, pKingdomColor.k2_color_1, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k2_2, pKingdomColor.k2_color_2, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k2_3, pKingdomColor.k2_color_3, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_k2_4, pKingdomColor.k2_color_4, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_light, Toolbox.color_light_replace, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_normal, Toolbox.color_magenta_1, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: true, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_phenotype_shade_0, DynamicColorPixelTool.phenotype_shade_0, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_phenotype_shade_1, DynamicColorPixelTool.phenotype_shade_1, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_phenotype_shade_2, DynamicColorPixelTool.phenotype_shade_2, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
		drawPixels(pixels, width, pBag.arr_pixels_phenotype_shade_3, DynamicColorPixelTool.phenotype_shade_3, pPartX, pPartY, pDynamicZombie, pActorAsset, pUseNormal: false, pHead);
	}

	private static void drawPixels(Color32[] pPixels, int pAtlasWidth, Pixel[] pListSourcePixels, Color32 pNewColor, int pPartX, int pPartY, bool pDrawDynamicZombie, ActorAsset pActorAsset, bool pUseNormal = false, bool pHead = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (pListSourcePixels == null)
		{
			return;
		}
		for (int i = 0; i < pListSourcePixels.Length; i++)
		{
			Pixel pixel = pListSourcePixels[i];
			Color32 val = pNewColor;
			int num = pixel.x + pPartX;
			int num2 = pixel.y + pPartY;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			int num3 = num + num2 * pAtlasWidth;
			if (pUseNormal)
			{
				val = pixel.color;
			}
			if (pDrawDynamicZombie)
			{
				val = DynamicColorPixelTool.checkZombieColors(pActorAsset, val, num3 / 3 + num, pHead);
			}
			pPixels[num3] = val;
		}
	}

	public static Sprite getSpriteUnit(AnimationFrameData pFrameData, Sprite pMainSprite, Actor pActor, ColorAsset pKingdomColor, int pPhenotypeIndex, int pPhenotypeShade, UnitTextureAtlasID pTextureAtlasID)
	{
		long num = 0L;
		long num2 = 0L;
		long num3 = pPhenotypeIndex;
		long num4 = 0L;
		long num5 = getBodySpriteSmallID(pMainSprite);
		if (pActor.has_rendered_sprite_head)
		{
			ActorAnimationLoader.int_ids_heads.TryGetValue(pActor.cached_sprite_head, out var value);
			if (value == 0)
			{
				int num6 = ActorAnimationLoader.int_ids_heads.Count + 1;
				ActorAnimationLoader.int_ids_heads.Add(pActor.cached_sprite_head, num6);
				value = num6;
			}
			num4 = value;
		}
		if (num3 != 0L)
		{
			num2 = pPhenotypeShade + 1;
		}
		if (pKingdomColor != null)
		{
			num = pKingdomColor.index_id + 1;
		}
		long num7 = num * 1000000000000L + num4 * 1000000000 + num5 * 1000000 + num3 * 1000 + num2;
		if (debug_actor == pActor)
		{
			AssetManager.dynamic_sprites_library.setDebugActor(num7, num, num4, num5, num3, num2);
		}
		DynamicSpritesAsset units = DynamicSpritesLibrary.units;
		Sprite val = units.getSprite(num7);
		if (val == null)
		{
			val = createNewSpriteUnit(pFrameData, pMainSprite, pActor.cached_sprite_head, pKingdomColor, pActor.asset, pPhenotypeIndex, pPhenotypeShade, pTextureAtlasID);
			units.addSprite(num7, val);
		}
		return val;
	}

	public static void setAtlasDirty(UnitSpriteConstructorAtlas pAtlas)
	{
		AssetManager.dynamic_sprites_library.setDirty();
		pAtlas.dirty = true;
		if (!pAtlas.isBigSpriteSheetAtlas())
		{
			pAtlas.checkDirty();
		}
	}

	public static int getBodySpriteSmallID(Sprite pSprite)
	{
		if (!_int_ids_body.TryGetValue(pSprite, out var value))
		{
			value = _int_ids_body.Count + 1;
			_int_ids_body.Add(pSprite, value);
		}
		return value;
	}

	static DynamicSpriteCreator()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_int_ids_body = new Dictionary<Sprite, int>();
		_placeholder_color_skin = Color32.op_Implicit(Toolbox.makeColor("#00FF00"));
		_light_colors = new List<Vector2Int>();
	}
}
