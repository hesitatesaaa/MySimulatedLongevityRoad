using UnityEngine;
using UnityEngine.UI;

public class SelectedPowerTabTopButtons : MonoBehaviour
{
	[SerializeField]
	private Image _favorite_icon;

	[SerializeField]
	private GameObject _button_possession;

	[SerializeField]
	private GameObject _button_spectate;

	[SerializeField]
	private GameObject _button_open_window;

	[SerializeField]
	private GameObject _button_favorite;

	[SerializeField]
	private UiMover _button_tab_back;

	[SerializeField]
	private Image _button_tab_back_icon;

	[SerializeField]
	private Text _button_tab_back_counter;

	private void Update()
	{
		if (SelectedUnit.isSet() || SelectedObjects.isNanoObjectSet())
		{
			bool active = Screen.width < Screen.height;
			_button_open_window.SetActive(active);
			bool flag = SelectedTabsHistory.count() > 1;
			_button_tab_back.setVisible(flag);
			if (flag)
			{
				updateTabBackButton();
			}
		}
		else
		{
			_button_open_window.SetActive(false);
			_button_tab_back.setVisible(pVisible: false);
		}
	}

	private void updateTabBackButton()
	{
		TabHistoryData? prevData = SelectedTabsHistory.getPrevData();
		if (prevData.HasValue)
		{
			MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(prevData.Value.meta_type);
			NanoObject nanoObject = prevData.Value.getNanoObject();
			Sprite sprite = ((nanoObject.getMetaType() != MetaType.Unit) ? asset.getIconSprite() : ((Actor)nanoObject).asset.getSpriteIcon());
			_button_tab_back_icon.sprite = sprite;
			_button_tab_back_counter.text = (SelectedTabsHistory.count() - 1).ToString();
		}
	}

	public void clickButtonFavorite()
	{
		if (SelectedObjects.isNanoObjectSet())
		{
			if (SelectedUnit.isSet())
			{
				clickFavoriteUnit();
			}
			else
			{
				clickFavoriteMeta();
			}
		}
	}

	public void clickLocate()
	{
	}

	public void clickOpenMain()
	{
		PowerTabAsset powerTabAsset = getPowerTabAsset();
		powerTabAsset.on_main_info_click(powerTabAsset);
	}

	private void clickFavoriteMeta()
	{
		ICoreObject coreObject = getPowerTabAsset().meta_type.getAsset().get_selected() as ICoreObject;
		coreObject.switchFavorite();
		if (coreObject.isFavorite())
		{
			WorldTip.showNowTop("tip_favorite_icon");
		}
		updateFavoriteIcon(coreObject.isFavorite());
	}

	private void clickFavoriteUnit()
	{
		Actor actor = (Actor)getPowerTabAsset().meta_type.getAsset().get_selected();
		actor.switchFavorite();
		if (actor.isFavorite())
		{
			WorldTip.showNowTop("tip_favorite_icon");
		}
		updateFavoriteIcon(actor.isFavorite());
	}

	private void updateFavoriteIcon(bool pStatus)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (pStatus)
		{
			((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_selected;
		}
		else
		{
			((Graphic)_favorite_icon).color = ColorStyleLibrary.m.favorite_not_selected;
		}
	}

	private PowerTabAsset getPowerTabAsset()
	{
		return PowersTab.getActiveTab().getAsset();
	}
}
