using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityBuildingIcons : CityElement
{
	public Image top;

	public Image topLeft;

	public Image topRight;

	public Image left;

	public Image right;

	public Image bottom;

	public Image bottomLeft;

	public Image bottomRight;

	public Image centerBonfire;

	private Image[] _list_house_images;

	private List<SpriteAnimation> _sprite_animations = new List<SpriteAnimation>();

	protected override void Awake()
	{
		_list_house_images = (Image[])(object)new Image[8] { top, topRight, right, bottomRight, bottom, bottomLeft, left, topLeft };
		base.Awake();
	}

	private void Update()
	{
		foreach (SpriteAnimation sprite_animation in _sprite_animations)
		{
			sprite_animation.update(Time.deltaTime);
		}
	}

	protected override void clear()
	{
		Image[] list_house_images = _list_house_images;
		for (int i = 0; i < list_house_images.Length; i++)
		{
			((Component)list_house_images[i]).gameObject.SetActive(false);
		}
		_sprite_animations.Clear();
	}

	protected override IEnumerator showContent()
	{
		((Graphic)centerBonfire).SetNativeSize();
		_sprite_animations.Add(((Component)centerBonfire).GetComponent<SpriteAnimation>());
		using ListPool<string> pPossibleIDs = getCityBuildingTypes();
		if (pPossibleIDs.Count != 0)
		{
			Image[] list_house_images = _list_house_images;
			foreach (Image tImage in list_house_images)
			{
				string tID = pPossibleIDs.GetRandom();
				yield return (object)new WaitForEndOfFrame();
				showSprite(tImage, tID);
			}
		}
	}

	private void showSprite(Image pImage, string pAssetID)
	{
		BuildingAsset buildingAsset = AssetManager.buildings.get(pAssetID);
		Sprite recoloredBuilding = DynamicSprites.getRecoloredBuilding(buildingAsset.building_sprites.animation_data.GetRandom().main.GetRandom(), base.city.kingdom?.getColor(), buildingAsset.atlas_asset);
		pImage.sprite = recoloredBuilding;
		((Graphic)pImage).SetNativeSize();
		((Component)pImage).gameObject.SetActive(true);
	}

	private ListPool<string> getCityBuildingTypes()
	{
		ListPool<string> listPool = new ListPool<string>(base.city.buildings.Count);
		foreach (Building building in base.city.buildings)
		{
			if (building.asset.hasHousingSlots() && building.asset.id.Contains("house"))
			{
				listPool.Add(building.asset.id);
			}
		}
		return listPool;
	}
}
