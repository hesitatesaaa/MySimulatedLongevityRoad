using System.Collections.Generic;
using UnityEngine;

public class ActorAssetWindow : BaseDebugAssetWindow<ActorAsset, ActorDebugAssetElement>
{
	public void clickRandomKingdomColor()
	{
		AssetsDebugManager.setRandomKingdomColor(asset.kingdom_id_wild);
		asset_debug_element.setData(asset);
	}

	public void clickRandomSkinColor()
	{
		AssetsDebugManager.setRandomSkinColor(asset);
		asset_debug_element.setData(asset);
	}

	public void clickChangeSex()
	{
		AssetsDebugManager.changeSex();
		asset_debug_element.setData(asset);
	}

	protected override void initSprites()
	{
		base.initSprites();
		string pPath = asset.texture_asset.texture_path_base;
		if (new List<string> { "dragon", "zombie_dragon", "worm" }.Contains(asset.id))
		{
			pPath = "actors_special/t_" + asset.id;
		}
		if (asset.is_boat)
		{
			pPath = "actors/boats/" + asset.id;
		}
		switch (asset.id)
		{
		case "UFO":
			pPath = "actors/special/t_ufo";
			break;
		case "crabzilla":
			pPath = "actors/special/crab";
			break;
		case "god_finger":
			pPath = "actors/species/other/god_finger";
			break;
		}
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pPath);
		foreach (Sprite val in spriteList)
		{
			SpriteElement spriteElement = Object.Instantiate<SpriteElement>(sprite_element_prefab, sprite_elements_parent);
			spriteElement.image.sprite = val;
			spriteElement.text_name.text = ((Object)val).name;
		}
	}
}
