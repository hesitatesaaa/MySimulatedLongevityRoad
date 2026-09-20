public class PowerTabLibrary : AssetLibrary<PowerTabAsset>
{
	public override void init()
	{
		addMainTabs();
		addSelectionTabs();
		add(new PowerTabAsset
		{
			id = "selected_unit",
			meta_type = MetaType.Unit,
			window_id = "unit",
			on_main_tab_select = delegate
			{
				if (SelectedUnit.isSet())
				{
					SelectedUnit.clear();
				}
			},
			on_main_info_click = delegate
			{
				ActionLibrary.openUnitWindow(SelectedUnit.unit);
				ScrollWindow.getCurrentWindow().tabs.showTab("MainTab");
			},
			on_update_check_active = (PowerTabAsset _) => SelectedUnit.isSet(),
			get_localized_worldtip = getWorldTipTextMetaName,
			get_power_tab = () => PowerTabController.instance.tab_selected_unit
		});
		add(new PowerTabAsset
		{
			id = "multiple_units",
			on_update_check_active = (PowerTabAsset _) => SelectedUnit.isSet(),
			on_main_tab_select = delegate
			{
				if (SelectedUnit.isSet())
				{
					SelectedUnit.clear();
				}
			},
			on_main_info_click = delegate
			{
				ActionLibrary.openUnitWindow(SelectedUnit.unit);
				ScrollWindow.getCurrentWindow().tabs.showTab("MainTab");
			},
			get_localized_worldtip = getWorldTipTextAmount,
			get_power_tab = () => PowerTabController.instance.tab_multiple_units
		});
		add(new PowerTabAsset
		{
			id = "selected_building"
		});
	}

	private void addMainTabs()
	{
		add(new PowerTabAsset
		{
			id = "main",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "creation",
			locale_key = "tab_world_creation",
			icon_path = "ui/Icons/power_tabs/icon_tab_drawings",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "noosphere",
			locale_key = "tab_noosphere",
			icon_path = "ui/Icons/power_tabs/icon_tab_noosphere",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "units",
			locale_key = "tab_world_creatures",
			icon_path = "ui/Icons/power_tabs/icon_tab_creatures",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "nature",
			locale_key = "tab_nature",
			icon_path = "ui/Icons/power_tabs/icon_tab_nature",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "destruction",
			locale_key = "tab_explosions",
			icon_path = "ui/Icons/power_tabs/icon_tab_bombs",
			tab_type_main = true
		});
		add(new PowerTabAsset
		{
			id = "other",
			locale_key = "tab_other",
			icon_path = "ui/Icons/power_tabs/icon_tab_other",
			tab_type_main = true
		});
	}

	private void addSelectionTabs()
	{
		add(new PowerTabAsset
		{
			id = "selected_army",
			meta_type = MetaType.Army,
			window_id = "army",
			get_power_tab = () => PowerTabController.instance.tab_selected_army,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_family",
			meta_type = MetaType.Family,
			window_id = "family",
			get_power_tab = () => PowerTabController.instance.tab_selected_family,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_subspecies",
			meta_type = MetaType.Subspecies,
			window_id = "subspecies",
			get_power_tab = () => PowerTabController.instance.tab_selected_subspecies,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_language",
			meta_type = MetaType.Language,
			window_id = "language",
			get_power_tab = () => PowerTabController.instance.tab_selected_language,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_culture",
			meta_type = MetaType.Culture,
			window_id = "culture",
			get_power_tab = () => PowerTabController.instance.tab_selected_culture,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_religion",
			meta_type = MetaType.Religion,
			window_id = "religion",
			get_power_tab = () => PowerTabController.instance.tab_selected_religion,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_clan",
			meta_type = MetaType.Clan,
			window_id = "clan",
			get_power_tab = () => PowerTabController.instance.tab_selected_clan,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_city",
			meta_type = MetaType.City,
			window_id = "city",
			get_power_tab = () => PowerTabController.instance.tab_selected_city,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_kingdom",
			meta_type = MetaType.Kingdom,
			window_id = "kingdom",
			get_power_tab = () => PowerTabController.instance.tab_selected_kingdom,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
		add(new PowerTabAsset
		{
			id = "selected_alliance",
			meta_type = MetaType.Alliance,
			window_id = "alliance",
			get_power_tab = () => PowerTabController.instance.tab_selected_alliance,
			on_update_check_active = defaultOnUpdateCheckActive,
			on_main_tab_select = defaultMainTabSelect,
			on_main_info_click = defaultMainInfoClick,
			get_localized_worldtip = getWorldTipTextMetaName
		});
	}

	private void defaultMainTabSelect(PowerTabAsset pAsset)
	{
		SelectedObjects.unselectNanoObject();
		pAsset.meta_type.getAsset().window_action_clear();
	}

	private bool defaultOnUpdateCheckActive(PowerTabAsset pAsset)
	{
		return SelectedObjects.isNanoObjectSet();
	}

	private void defaultMainInfoClick(PowerTabAsset pAsset)
	{
		ScrollWindow.showWindow(pAsset.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("MainTab");
	}

	private string getWorldTipTextMetaName(PowerTabAsset pAsset)
	{
		NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
		string text = LocalizedTextManager.getText("show_tab_" + pAsset.id);
		string newValue = StringExtension.ColorHex(pColorHex: selectedNanoObject.getColor().color_text, pString: selectedNanoObject.name);
		return text.Replace("$name$", newValue);
	}

	private string getWorldTipTextAmount(PowerTabAsset pAsset)
	{
		int num = SelectedUnit.countSelected();
		return LocalizedTextManager.getText("show_tab_" + pAsset.id).Replace("$amount$", num.ToString());
	}

	public override void editorDiagnosticLocales()
	{
		foreach (PowerTabAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
		base.editorDiagnosticLocales();
	}
}
