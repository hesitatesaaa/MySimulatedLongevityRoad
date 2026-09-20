using System.Collections.Generic;
using UnityEngine;

public class ListWindowLibrary : AssetLibrary<ListWindowAsset>
{
	private Dictionary<MetaType, ListWindowAsset> _dict = new Dictionary<MetaType, ListWindowAsset>();

	public override void init()
	{
		add(new ListWindowAsset
		{
			id = "list_alliances",
			meta_type = MetaType.Alliance,
			no_items_locale = "list_empty_alliances",
			art_path = "ui/illustrations/art_alliances",
			icon_path = "ui/Icons/iconAllianceList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<AllianceListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_clans",
			meta_type = MetaType.Clan,
			no_items_locale = "list_empty_clans",
			art_path = "ui/illustrations/art_clans",
			icon_path = "ui/Icons/iconClanList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<ClanListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_cultures",
			meta_type = MetaType.Culture,
			no_items_locale = "list_empty_cultures",
			art_path = "ui/illustrations/art_cultures",
			icon_path = "ui/Icons/iconCultureList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<CultureListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_cities",
			meta_type = MetaType.City,
			no_items_locale = "list_empty_villages",
			art_path = "ui/illustrations/art_cities",
			icon_path = "ui/Icons/iconCityList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<CityListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_families",
			meta_type = MetaType.Family,
			no_items_locale = "list_empty_families",
			art_path = "ui/illustrations/art_families",
			icon_path = "ui/Icons/iconFamilyList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<FamilyListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_favorite_items",
			meta_type = MetaType.Item,
			no_items_locale = "list_empty_favorites_items",
			art_path = "ui/illustrations/art_favorite_items",
			icon_path = "ui/Icons/iconFavoriteItemsList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<FavoriteItemListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_favorite_units",
			meta_type = MetaType.Unit,
			no_items_locale = "list_empty_favorite_units",
			art_path = "ui/illustrations/art_favorite_units",
			icon_path = "ui/Icons/iconFavoritesList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<WindowFavorites>()
		});
		add(new ListWindowAsset
		{
			id = "list_kingdoms",
			meta_type = MetaType.Kingdom,
			no_items_locale = "list_empty_kingdoms",
			art_path = "ui/illustrations/art_kingdoms",
			icon_path = "ui/Icons/iconKingdomList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<KingdomListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_languages",
			meta_type = MetaType.Language,
			no_items_locale = "list_empty_languages",
			art_path = "ui/illustrations/art_languages",
			icon_path = "ui/Icons/iconLanguageList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<LanguageListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_plots",
			meta_type = MetaType.Plot,
			no_items_locale = "list_empty_plots",
			art_path = "ui/illustrations/art_plots",
			icon_path = "ui/Icons/iconPlotList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<PlotListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_religions",
			meta_type = MetaType.Religion,
			no_items_locale = "list_empty_religions",
			art_path = "ui/illustrations/art_religions",
			icon_path = "ui/Icons/iconReligionList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<ReligionListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_subspecies",
			meta_type = MetaType.Subspecies,
			no_items_locale = "list_empty_subspecies",
			art_path = "ui/illustrations/art_subspecies",
			icon_path = "ui/Icons/iconSubspeciesList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<SubspeciesListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_wars",
			meta_type = MetaType.War,
			no_items_locale = "list_empty_wars",
			no_dead_items_locale = "empty_past_wars_list",
			art_path = "ui/illustrations/art_wars",
			icon_path = "ui/Icons/iconWarList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<WarListComponent>()
		});
		add(new ListWindowAsset
		{
			id = "list_armies",
			meta_type = MetaType.Army,
			no_items_locale = "list_empty_armies",
			art_path = "ui/illustrations/art_armies",
			icon_path = "ui/Icons/iconArmyList",
			set_list_component = (Transform pTransform) => ((Component)(object)pTransform).AddComponent<ArmyListComponent>()
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (ListWindowAsset item in list)
		{
			_dict.Add(item.meta_type, item);
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (ListWindowAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
	}

	public ListWindowAsset getByMetaType(MetaType pType)
	{
		return _dict[pType];
	}
}
