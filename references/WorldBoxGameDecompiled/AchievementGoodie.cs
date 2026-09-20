using UnityEngine;
using UnityEngine.UI;

public class AchievementGoodie : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Text _name;

	public void load(BaseUnlockableAsset pAsset, bool pUnlocked)
	{
		if (pUnlocked)
		{
			loadUnlocked(pAsset);
		}
		else
		{
			loadLocked(pAsset);
		}
	}

	private void loadLocked(BaseUnlockableAsset pAsset)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_icon.sprite = pAsset.getSprite();
		((Graphic)_icon).color = Toolbox.color_black;
	}

	private void loadUnlocked(BaseUnlockableAsset pAssets)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		_icon.sprite = pAssets.getSprite();
		((Component)_name).GetComponent<LocalizedText>().setKeyAndUpdate(pAssets.getLocaleID());
		if (!(pAssets is ActorAsset actorAsset))
		{
			if (pAssets is BaseAugmentationAsset baseAugmentationAsset)
			{
				BaseCategoryAsset baseCategoryAsset = baseAugmentationAsset.getGroup();
				((Graphic)_name).color = baseCategoryAsset?.getColor() ?? Toolbox.color_white;
			}
			else
			{
				((Graphic)_name).color = Toolbox.color_white;
			}
		}
		else
		{
			KingdomAsset kingdomAsset = AssetManager.kingdoms.get(actorAsset.kingdom_id_wild);
			((Graphic)_name).color = kingdomAsset.default_kingdom_color.getColorText();
		}
	}
}
