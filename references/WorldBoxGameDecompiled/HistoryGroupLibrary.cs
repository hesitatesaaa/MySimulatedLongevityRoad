public class HistoryGroupLibrary : AssetLibrary<HistoryGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new HistoryGroupAsset
		{
			id = "kings",
			icon_path = "ui/Icons/history_group/icon_kings"
		});
		add(new HistoryGroupAsset
		{
			id = "favorite_units",
			icon_path = "ui/Icons/iconFavoriteStar"
		});
		add(new HistoryGroupAsset
		{
			id = "cities",
			icon_path = "ui/Icons/iconCityList"
		});
		add(new HistoryGroupAsset
		{
			id = "kingdoms",
			icon_path = "ui/Icons/iconKingdomList"
		});
		add(new HistoryGroupAsset
		{
			id = "wars",
			icon_path = "ui/Icons/iconWarList"
		});
		add(new HistoryGroupAsset
		{
			id = "clans",
			icon_path = "ui/Icons/iconClanList"
		});
		add(new HistoryGroupAsset
		{
			id = "disasters",
			icon_path = "ui/Icons/worldrules/icon_disasters"
		});
	}

	public override void post_init()
	{
		foreach (HistoryGroupAsset item in list)
		{
			if (string.IsNullOrEmpty(item.icon_path))
			{
				item.icon_path = "ui/Icons/history_group/icon_" + item.id;
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (HistoryGroupAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
