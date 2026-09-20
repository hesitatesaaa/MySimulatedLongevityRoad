using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitHouseElement : UnitElement
{
	[SerializeField]
	private GameObject _title;

	[SerializeField]
	private GameObject _house_container;

	[SerializeField]
	private Image _house_image;

	protected override IEnumerator showContent()
	{
		if (actor.hasHomeBuilding())
		{
			Building homeBuilding = actor.getHomeBuilding();
			track_objects.Add(homeBuilding);
			_title.SetActive(true);
			_house_container.gameObject.SetActive(true);
			showSprite(actor.kingdom, _house_image, homeBuilding);
			setIconValue("i_house_health", homeBuilding.getHealth(), homeBuilding.getMaxHealth());
			setIconValue("i_house_people", homeBuilding.countResidents(), homeBuilding.asset.housing_slots);
		}
		yield break;
	}

	private void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		Transform val = ((Component)this).transform.FindRecursive(pName);
		if (!((Object)(object)val == (Object)null))
		{
			StatsIcon component = ((Component)val).GetComponent<StatsIcon>();
			((Component)component).gameObject.SetActive(true);
			component.setValue(pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
		}
	}

	private void showSprite(Kingdom pKingdom, Image pImage, Building pBuilding)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		BuildingAsset asset = pBuilding.asset;
		Sprite recoloredBuilding = DynamicSprites.getRecoloredBuilding(asset.building_sprites.animation_data[pBuilding.animData_index].main.GetRandom(), pKingdom.getColor(), asset.atlas_asset);
		pImage.sprite = recoloredBuilding;
		((Graphic)pImage).SetNativeSize();
		float num = 28f / ((Graphic)pImage).rectTransform.sizeDelta.x;
		float num2 = 28f / ((Graphic)pImage).rectTransform.sizeDelta.y;
		float num3 = Mathf.Min(num, num2);
		((Graphic)pImage).rectTransform.sizeDelta = new Vector2(((Graphic)pImage).rectTransform.sizeDelta.x * num3, ((Graphic)pImage).rectTransform.sizeDelta.y * num3);
	}

	protected override void clear()
	{
		_title.SetActive(false);
		_house_container.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (_house_container.activeSelf && !actor.hasHomeBuilding())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
