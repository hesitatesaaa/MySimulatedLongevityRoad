using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectedNanoBase : MonoBehaviour
{
	public StatsIconContainer[] stats_icons;

	protected PowersTab _powers_tab;

	[SerializeField]
	private Image _favorite_icon;

	[SerializeField]
	protected Image icon_left;

	[SerializeField]
	protected Image icon_right;

	[SerializeField]
	protected Text name_field;

	protected virtual void Awake()
	{
		_powers_tab = ((Component)this).GetComponent<PowersTab>();
		if (stats_icons == null || stats_icons.Length == 0)
		{
			Debug.LogError((object)("SelectedNano: No StatsIconContainer found in children of " + ((Object)((Component)this).gameObject).name));
		}
	}

	public virtual void update()
	{
	}

	protected PowerTabAsset getPowerTabAsset()
	{
		return AssetManager.power_tab_library.get(getPowerTabAssetID());
	}

	protected virtual string getPowerTabAssetID()
	{
		throw new NotImplementedException();
	}

	public void clickFavoriteMeta()
	{
		ICoreObject coreObject = getPowerTabAsset().meta_type.getAsset().get_selected() as ICoreObject;
		coreObject.switchFavorite();
		if (coreObject.isFavorite())
		{
			string text = LocalizedTextManager.getText("favorited");
			WorldTip.instance.showToolbarText(text);
		}
		updateFavoriteIcon(coreObject.isFavorite());
	}

	public void clickFavoriteUnit()
	{
		Actor actor = (Actor)getPowerTabAsset().meta_type.getAsset().get_selected();
		actor.switchFavorite();
		if (actor.isFavorite())
		{
			string text = LocalizedTextManager.getText("tip_favorite_icon");
			WorldTip.instance.showToolbarText(text);
		}
		updateFavoriteIcon(actor.isFavorite());
	}

	protected void updateFavoriteIcon(bool pStatus)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_favorite_icon == (Object)null))
		{
			if (pStatus)
			{
				((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_selected;
			}
			else
			{
				((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_not_selected;
			}
		}
	}

	protected void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		StatsIconContainer[] array = stats_icons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].setIconValue(pName, pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
		}
	}
}
