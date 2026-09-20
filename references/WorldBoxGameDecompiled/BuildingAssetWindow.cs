using UnityEngine;

public class BuildingAssetWindow : BaseDebugAssetWindow<BuildingAsset, BuildingDebugAssetElement>
{
	public void clickRandomKingdomColor()
	{
		AssetsDebugManager.setRandomKingdomColor(asset.civ_kingdom);
		asset_debug_element.setData(asset);
	}

	protected override void initSprites()
	{
		base.initSprites();
		string text = asset.sprite_path;
		if (string.IsNullOrEmpty(text))
		{
			text = asset.main_path + asset.id;
		}
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(text);
		foreach (Sprite val in spriteList)
		{
			SpriteElement spriteElement = Object.Instantiate<SpriteElement>(sprite_element_prefab, sprite_elements_parent);
			spriteElement.image.sprite = val;
			spriteElement.text_name.text = ((Object)val).name;
		}
	}

	public static void reloadSprites()
	{
		BaseDebugAssetWindow<BuildingAsset, BuildingDebugAssetElement>.current_element.setData(BaseDebugAssetWindow<BuildingAsset, BuildingDebugAssetElement>.current_element.asset);
	}
}
