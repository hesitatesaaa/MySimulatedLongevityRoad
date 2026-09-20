using System.Collections.Generic;
using UnityEngine.UI;

public static class BaseStatsHelper
{
	public delegate KeyValueField KeyValueFieldGetter(string pID);

	public static BaseStats _base_stats_tooltip_helper = new BaseStats();

	private static List<BaseStatsContainer> _stats_container_positive = new List<BaseStatsContainer>();

	private static List<BaseStatsContainer> _stats_container_negative = new List<BaseStatsContainer>();

	public static BaseStats getTotalStatsFrom(BaseStats pBaseStats, BaseStats pBaseStatsMeta)
	{
		_base_stats_tooltip_helper.clear();
		_base_stats_tooltip_helper.mergeStats(pBaseStats);
		_base_stats_tooltip_helper.mergeStats(pBaseStatsMeta);
		return _base_stats_tooltip_helper;
	}

	public static void showItemMods(Text pTextFieldDescription, Text pTextFieldValues, Item pItem)
	{
		using ListPool<TooltipModContainerInfo> listPool = getItemModsBase(pItem);
		foreach (ref TooltipModContainerInfo item in listPool)
		{
			TooltipModContainerInfo current = item;
			addStatValues(pTextFieldDescription, pTextFieldValues, Toolbox.coloredText("+" + LocalizedTextManager.getText(current.asset.getLocaleID()), "#45FFFE"), Toolbox.coloredText(current.string_pluses, "#45FFFE"));
			addLineBreak(pTextFieldDescription, pTextFieldValues);
		}
	}

	public static void showItemModsRows(KeyValueFieldGetter pFieldsFabric, Item pItem)
	{
		using ListPool<TooltipModContainerInfo> listPool = getItemModsBase(pItem);
		foreach (ref TooltipModContainerInfo item in listPool)
		{
			TooltipModContainerInfo current = item;
			string text = Toolbox.coloredText("+" + LocalizedTextManager.getText(current.asset.getLocaleID()), "#45FFFE");
			string text2 = Toolbox.coloredText(current.string_pluses, "#45FFFE");
			KeyValueField keyValueField = pFieldsFabric(current.asset.getLocaleID());
			keyValueField.name_text.text = text;
			keyValueField.value.text = text2;
		}
	}

	private static ListPool<TooltipModContainerInfo> getItemModsBase(Item pItem)
	{
		ListPool<TooltipModContainerInfo> listPool = new ListPool<TooltipModContainerInfo>(pItem.data.modifiers.Count);
		foreach (ref string modifier in pItem.data.modifiers)
		{
			string current = modifier;
			ItemModAsset itemModAsset = AssetManager.items_modifiers.get(current);
			string text = "";
			for (int i = 0; i < itemModAsset.mod_rank; i++)
			{
				text += "+";
			}
			listPool.Add(new TooltipModContainerInfo(itemModAsset, itemModAsset.mod_rank, text));
		}
		listPool.Sort(sortByPluses);
		return listPool;
	}

	private static void addStatValues(Text pStatsField, Text pValuesField, string pStats, string pValues)
	{
		pStatsField.text += pStats;
		pValuesField.text += pValues;
	}

	private static void addLineBreak(Text pStatsField, Text pValuesField)
	{
		pStatsField.text += "\n";
		pValuesField.text += "\n";
	}

	private static int sortByPluses(TooltipModContainerInfo pContainer1, TooltipModContainerInfo pContainer2)
	{
		return pContainer2.pluses.CompareTo(pContainer1.pluses);
	}

	public static void showBaseStats(Text pStatsField, Text pValuesField, BaseStats pBaseStats, bool pAddPlus = true)
	{
		calcBaseStatsBase(pBaseStats);
		foreach (BaseStatsContainer item in _stats_container_positive)
		{
			showBaseStatLine(pStatsField, pValuesField, item, pAddColor: true, pAddPlus);
		}
		foreach (BaseStatsContainer item2 in _stats_container_negative)
		{
			showBaseStatLine(pStatsField, pValuesField, item2, pAddColor: true, pAddPlus);
		}
	}

	public static void showBaseStatsRows(KeyValueFieldGetter pFieldsFabric, BaseStats pBaseStats, bool pAddPlus = true)
	{
		calcBaseStatsBase(pBaseStats);
		foreach (BaseStatsContainer item in _stats_container_positive)
		{
			showBaseStatRow(pFieldsFabric, item, pAddColor: true, pAddPlus);
		}
		foreach (BaseStatsContainer item2 in _stats_container_negative)
		{
			showBaseStatRow(pFieldsFabric, item2, pAddColor: true, pAddPlus);
		}
	}

	private static void calcBaseStatsBase(BaseStats pBaseStats)
	{
		_stats_container_positive.Clear();
		_stats_container_negative.Clear();
		foreach (BaseStatsContainer item in pBaseStats.getList())
		{
			if (!item.asset.hidden || DebugConfig.isOn(DebugOption.ShowHiddenStats))
			{
				queueStatContainer(item);
			}
		}
		_stats_container_positive.Sort(sortByRank);
	}

	private static int sortByRank(BaseStatsContainer pContainerA, BaseStatsContainer pContainerB)
	{
		BaseStatAsset asset = pContainerA.asset;
		return pContainerB.asset.sort_rank.CompareTo(asset.sort_rank);
	}

	private static void queueStatContainer(BaseStatsContainer pContainer)
	{
		if (pContainer.value > 0f)
		{
			_stats_container_positive.Add(pContainer);
		}
		if (pContainer.value < 0f)
		{
			_stats_container_negative.Add(pContainer);
		}
	}

	private static void showBaseStatLine(Text pStatsField, Text pValuesField, BaseStatsContainer pContainer, bool pAddColor = true, bool pAddPlus = true, string pMainColor = "#43FF43", bool pForceZero = false)
	{
		calcBaseStatLineBase(pContainer, out var tId, out var tValue, out var tAsset);
		if (!tAsset.hidden)
		{
			addItemText(pStatsField, pValuesField, tId, tValue, tAsset.show_as_percents, pAddColor, pAddPlus, pMainColor, pForceZero);
			return;
		}
		if (pStatsField.text.Length > 0)
		{
			addLineBreak(pStatsField, pValuesField);
		}
		string pText = tId;
		string text = tValue.ToText();
		if (tAsset.show_as_percents)
		{
			text += " %";
		}
		pValuesField.text += Toolbox.coloredText(text, ColorStyleLibrary.m.color_text_grey);
		pStatsField.text += Toolbox.coloredText(pText, ColorStyleLibrary.m.color_text_grey);
	}

	private static void showBaseStatRow(KeyValueFieldGetter pFieldsFabric, BaseStatsContainer pContainer, bool pAddColor = true, bool pAddPlus = true, string pMainColor = "#43FF43", bool pForceZero = false)
	{
		calcBaseStatLineBase(pContainer, out var tId, out var tValue, out var tAsset);
		addItemTextRow(pFieldsFabric(tId), tId, tValue, tAsset.show_as_percents, pAddColor, pAddPlus, pMainColor, pForceZero);
	}

	private static void calcBaseStatLineBase(BaseStatsContainer pContainer, out string tId, out float tValue, out BaseStatAsset tAsset)
	{
		tAsset = pContainer.asset;
		tId = tAsset.getLocaleID();
		tValue = pContainer.value;
		if (tAsset.tooltip_multiply_for_visual_number != 1f)
		{
			tValue *= tAsset.tooltip_multiply_for_visual_number;
		}
		if (tAsset.hidden && DebugConfig.isOn(DebugOption.ShowHiddenStats))
		{
			tId = "[HIDDEN] " + tId;
		}
	}

	private static void addItemText(Text pStatsField, Text pValuesField, string pID, float pValue, bool pPercent = false, bool pAddColor = true, bool pAddPlus = true, string pMainColor = "#43FF43", bool pForceZero = false)
	{
		addItemTextBase(pValue, out var pValString, pPercent, pForceZero);
		if (!pAddColor)
		{
			addLineText(pStatsField, pValuesField, pID, pValString, null, pPercent);
		}
		else if (pValue > 0f)
		{
			if (pAddPlus)
			{
				pValString = "+" + pValString;
			}
			addLineText(pStatsField, pValuesField, pID, pValString, pMainColor, pPercent);
		}
		else
		{
			addLineText(pStatsField, pValuesField, pID, pValString, "#FB2C21", pPercent);
		}
	}

	private static void addItemTextRow(KeyValueField pField, string pID, float pValue, bool pPercent = false, bool pAddColor = true, bool pAddPlus = true, string pMainColor = "#43FF43", bool pForceZero = false)
	{
		addItemTextBase(pValue, out var pValString, pPercent, pForceZero);
		if (!pAddColor)
		{
			addRowText(pField, pID, pValString, null, pPercent);
		}
		else if (pValue > 0f)
		{
			if (pAddPlus)
			{
				pValString = "+" + pValString;
			}
			addRowText(pField, pID, pValString, pMainColor, pPercent);
		}
		else
		{
			addRowText(pField, pID, pValString, "#FB2C21", pPercent);
		}
	}

	private static void addItemTextBase(float pValue, out string pValString, bool pPercent = false, bool pForceZero = false)
	{
		pValString = pValue.ToText();
		if ((pValue != 0f || pForceZero) && pPercent)
		{
			pValString += "%";
		}
	}

	private static void addLineIntText(Text pStatsField, Text pValuesField, string pID, int pValue, string pColor = null)
	{
		addLineText(pStatsField, pValuesField, pID, pValue.ToText(), pColor);
	}

	private static void addLineText(Text pStatsField, Text pValuesField, string pID, string pValue, string pColor = null, bool pPercent = false)
	{
		if (pStatsField.text.Length > 0)
		{
			addLineBreak(pStatsField, pValuesField);
		}
		if (pValue.Length > 21)
		{
			pValue = pValue.Substring(0, 20) + "...";
		}
		string text = LocalizedTextManager.getText(pID);
		if (pPercent)
		{
			text += " %";
		}
		if (!string.IsNullOrEmpty(pColor))
		{
			pStatsField.text += text;
			pValuesField.text += Toolbox.coloredText(pValue, pColor);
		}
		else
		{
			pStatsField.text += text;
			pValuesField.text += pValue;
		}
	}

	private static void addRowText(KeyValueField pField, string pID, string pValue, string pColor = null, bool pPercent = false)
	{
		if (pValue.Length > 21)
		{
			pValue = pValue.Substring(0, 20) + "...";
		}
		string text;
		if (pID.Contains("[HIDDEN]"))
		{
			text = pID;
			pColor = ColorStyleLibrary.m.color_text_grey;
		}
		else
		{
			text = LocalizedTextManager.getText(pID);
		}
		if (pPercent)
		{
			text += " %";
		}
		if (!string.IsNullOrEmpty(pColor))
		{
			pField.name_text.text = Toolbox.coloredText(text, pColor);
			pField.value.text = Toolbox.coloredText(pValue, pColor);
		}
		else
		{
			pField.name_text.text = text;
			pField.value.text = pValue;
		}
	}
}
