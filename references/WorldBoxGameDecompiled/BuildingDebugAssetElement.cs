using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingDebugAssetElement : BaseDebugAssetElement<BuildingAsset>
{
	public BuildingDebugAnimationElement spawn;

	public BuildingDebugAnimationElement main;

	public BuildingDebugAnimationElement disabled;

	public BuildingDebugAnimationElement ruin;

	public BuildingDebugAnimationElement special;

	public Image construction;

	public Image mini;

	public override void setData(BuildingAsset pAsset)
	{
		asset = pAsset;
		title.text = asset.id;
		initAnimations();
		initStats();
	}

	protected override void initAnimations()
	{
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		BuildingSprites building_sprites = asset.building_sprites;
		spawn.setData(asset);
		main.setData(asset);
		disabled.setData(asset);
		ruin.setData(asset);
		special.setData(asset);
		List<DebugAnimatedVariation> list = new List<DebugAnimatedVariation>();
		List<DebugAnimatedVariation> list2 = new List<DebugAnimatedVariation>();
		List<DebugAnimatedVariation> list3 = new List<DebugAnimatedVariation>();
		List<DebugAnimatedVariation> list4 = new List<DebugAnimatedVariation>();
		List<DebugAnimatedVariation> list5 = new List<DebugAnimatedVariation>();
		foreach (BuildingAnimationData animation_datum in asset.building_sprites.animation_data)
		{
			list.Add(new DebugAnimatedVariation(getBuildingColoredSprites(animation_datum.spawn), animation_datum.animated));
			list2.Add(new DebugAnimatedVariation(getBuildingColoredSprites(animation_datum.main), animation_datum.animated));
			list3.Add(new DebugAnimatedVariation(getBuildingColoredSprites(animation_datum.main_disabled), animation_datum.animated));
			list4.Add(new DebugAnimatedVariation(getBuildingColoredSprites(animation_datum.ruins), animation_datum.animated));
			list5.Add(new DebugAnimatedVariation(getBuildingColoredSprites(animation_datum.special), animation_datum.animated));
		}
		spawn.setFrames(list, asset.has_sprites_spawn);
		main.setFrames(list2, asset.has_sprites_main);
		disabled.setFrames(list3, asset.has_sprites_main_disabled);
		ruin.setFrames(list4, asset.has_sprites_ruin);
		special.setFrames(list5, asset.has_sprites_special);
		if ((Object)(object)building_sprites.construction != (Object)null)
		{
			construction.sprite = building_sprites.construction;
		}
		else if (asset.has_sprite_construction)
		{
			construction.sprite = no_animation;
		}
		else
		{
			((Graphic)construction).color = Color.clear;
		}
		mini.sprite = loadMini();
	}

	private Sprite loadMini()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		string text = asset.sprite_path;
		if (string.IsNullOrEmpty(text))
		{
			text = asset.main_path + asset.id;
		}
		text += "/mini_0";
		Sprite sprite = SpriteTextureLoader.getSprite(text);
		if ((Object)(object)sprite == (Object)null)
		{
			Debug.LogError((object)("Not found mini sprite for building: " + asset.id));
			return sprite;
		}
		KingdomAsset kingdomAsset = AssetManager.kingdoms.get("mad");
		if (!asset.has_kingdom_color)
		{
			return sprite;
		}
		ColorAsset debug_color_asset = kingdomAsset.debug_color_asset;
		Texture2D val = new Texture2D(((Texture)sprite.texture).width, ((Texture)sprite.texture).height);
		((Texture)val).filterMode = ((Texture)sprite.texture).filterMode;
		for (int i = 0; i < ((Texture)val).width; i++)
		{
			for (int j = 0; j < ((Texture)val).height; j++)
			{
				Color pixel = sprite.texture.GetPixel(i, j);
				Color val2 = Color32.op_Implicit(getColor(pixel, debug_color_asset));
				val.SetPixel(i, j, val2);
			}
		}
		val.Apply();
		return Sprite.Create(val, new Rect(Vector2.zero, new Vector2((float)((Texture)val).width, (float)((Texture)val).height)), new Vector2(0.5f, 0.5f), 1f);
	}

	private Color32 getColor(Color pOrigColor, ColorAsset pKingdomColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (Toolbox.areColorsEqual(Color32.op_Implicit(pOrigColor), Toolbox.color_magenta_0))
		{
			pOrigColor = Color32.op_Implicit(pKingdomColor.k_color_0);
		}
		else if (Toolbox.areColorsEqual(Color32.op_Implicit(pOrigColor), Toolbox.color_magenta_1))
		{
			pOrigColor = Color32.op_Implicit(pKingdomColor.k_color_1);
		}
		else if (Toolbox.areColorsEqual(Color32.op_Implicit(pOrigColor), Toolbox.color_magenta_2))
		{
			pOrigColor = Color32.op_Implicit(pKingdomColor.k_color_2);
		}
		else if (Toolbox.areColorsEqual(Color32.op_Implicit(pOrigColor), Toolbox.color_magenta_3))
		{
			pOrigColor = Color32.op_Implicit(pKingdomColor.k_color_3);
		}
		else if (Toolbox.areColorsEqual(Color32.op_Implicit(pOrigColor), Toolbox.color_magenta_4))
		{
			pOrigColor = Color32.op_Implicit(pKingdomColor.k_color_4);
		}
		return Color32.op_Implicit(pOrigColor);
	}

	public override void update()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			spawn.update();
			main.update();
			disabled.update();
			ruin.update();
			special.update();
		}
	}

	public override void stopAnimations()
	{
		spawn.stopAnimations();
		main.stopAnimations();
		disabled.stopAnimations();
		ruin.stopAnimations();
		special.stopAnimations();
	}

	public override void startAnimations()
	{
		spawn.startAnimations();
		main.startAnimations();
		disabled.startAnimations();
		ruin.startAnimations();
		special.startAnimations();
	}

	private Sprite[] getBuildingColoredSprites(Sprite[] pSprites)
	{
		if (pSprites == null)
		{
			return (Sprite[])(object)new Sprite[0];
		}
		Sprite[] array = (Sprite[])(object)new Sprite[pSprites.Length];
		for (int i = 0; i < pSprites.Length; i++)
		{
			array[i] = getBuildingColoredSprite(pSprites[i]);
		}
		return array;
	}

	private Sprite getBuildingColoredSprite(Sprite pMainSprite)
	{
		ColorAsset pColor = null;
		if (asset.has_kingdom_color)
		{
			pColor = AssetManager.kingdoms.get("mad").debug_color_asset;
		}
		return DynamicSprites.getRecoloredBuilding(pMainSprite, pColor, asset.atlas_asset);
	}

	protected override void initStats()
	{
		base.initStats();
		showStat("health", asset.base_stats["health"]);
		showStat("damage", asset.base_stats["damage"]);
		showStat("targets", asset.base_stats["targets"]);
		showStat("area_of_effect", asset.base_stats["area_of_effect"]);
	}

	protected override void showAssetWindow()
	{
		base.showAssetWindow();
		ScrollWindow.showWindow("building_asset");
	}
}
