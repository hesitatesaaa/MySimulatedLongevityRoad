using System;
using UnityEngine;

public class StatsWindow : TabbedWindow
{
	[SerializeField]
	protected StatsMetaRowsContainer meta_rows_container;

	private StatsRowsContainer _stats_rows_container;

	private IRefreshElement[] _refreshable_elements;

	private const string EMPTY_META_OBJECT_STRING = "???";

	public virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal StatsRowsContainer stats_rows_container
	{
		get
		{
			if ((Object)(object)_stats_rows_container == (Object)null)
			{
				_stats_rows_container = ((Component)((Component)this).transform.FindRecursive("content_stats")).GetComponent<StatsRowsContainer>();
				if ((Object)(object)_stats_rows_container == (Object)null)
				{
					Debug.LogError((object)("No StatsRowsContainer 'content_stats' found in " + ((Object)this).name));
				}
			}
			return _stats_rows_container;
		}
	}

	protected virtual void OnEnable()
	{
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(meta_type);
		if (!string.IsNullOrEmpty(asset.power_tab_id))
		{
			SelectedObjects.setNanoObject(asset.get_selected());
			PowerTabController.showWorldTipSelected(asset.power_tab_id, pBar: false);
			ScrollWindow.skip_worldtip_hide = true;
		}
	}

	protected override void create()
	{
		base.create();
		_refreshable_elements = ((Component)this).GetComponentsInChildren<IRefreshElement>(true);
	}

	protected void refreshMetaList()
	{
		MetaSwitchManager.refresh();
	}

	public void updateStats()
	{
		IRefreshElement[] refreshable_elements = _refreshable_elements;
		for (int i = 0; i < refreshable_elements.Length; i++)
		{
			refreshable_elements[i].refresh();
		}
	}

	internal KeyValueField getStatRow(string pID)
	{
		return stats_rows_container.getStatRow(pID);
	}

	internal virtual void showStatsRows()
	{
		throw new NotImplementedException("showStatsRows not implemented");
	}

	public virtual void showMetaRows()
	{
	}

	internal KeyValueField showStatRow(string pId, object pValue, MetaType pMetaType = MetaType.None, long pMetaId = -1L, string pIconPath = null, string pTooltipId = null, TooltipDataGetter pTooltipData = null)
	{
		return stats_rows_container.showStatRow(pId, pValue, null, pMetaType, pMetaId, pColorText: false, pIconPath, pTooltipId, pTooltipData);
	}

	internal KeyValueField showStatRow(string pId, object pValue, string pColor, MetaType pMetaType = MetaType.None, long pMetaId = -1L, bool pColorText = false, string pIconPath = null, string pTooltipId = null, TooltipDataGetter pTooltipData = null, bool pLocalize = true)
	{
		return stats_rows_container.showStatRow(pId, pValue, pColor, pMetaType, pMetaId, pColorText, pIconPath, pTooltipId, pTooltipData, pLocalize);
	}

	internal void updateStatsRows()
	{
		((Component)stats_rows_container).gameObject.SetActive(false);
		((Component)stats_rows_container).gameObject.SetActive(true);
	}

	protected void showStatRowMeta(string pId, object pValue, string pColor, MetaType pMetaType, long pMetaId, bool pColorText = false, string pIconPath = null, string pTooltipId = null, TooltipDataGetter pTooltipData = null, bool pLocalize = true)
	{
		showStatRow(pId, pValue, pColor, pMetaType, pMetaId, pColorText: true, pIconPath, pTooltipId, pTooltipData, pLocalize);
	}

	private void showStatsMetaCity(City pCity, string pTitle)
	{
		string text = "???";
		string text2 = pCity.kingdom.getColor()?.color_text;
		text = pCity.name;
		text += Toolbox.coloredGreyPart(pCity.getPopulationPeople(), text2);
		showStatRowMeta(pTitle, text, text2, MetaType.City, pCity.getID(), pColorText: false, "iconCity");
	}

	private void showStatsMetaKingdom(Kingdom pKingdom, string pTitle = "kingdom")
	{
		string pValue = "???";
		string text = pKingdom?.getColor().color_text;
		if (pKingdom != null)
		{
			pValue = pKingdom.data.name;
			pValue += Toolbox.coloredGreyPart(pKingdom.getPopulationPeople(), text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Kingdom, pKingdom?.getID() ?? (-1), pColorText: false, "iconKingdom");
	}

	private void showStatsMetaUnit(Actor pActor, string pTitle, string pIconPath = null)
	{
		string pValue = "???";
		string text = pActor?.kingdom.getColor().color_text;
		if (pActor != null)
		{
			pValue = pActor.getName();
			pValue += Toolbox.coloredGreyPart(pActor.getAge(), text, pUnit: true);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Unit, pActor?.getID() ?? (-1), pColorText: false, pIconPath);
	}

	private void showStatsMetaCulture(Culture pCulture, string pTitle = "culture")
	{
		string pValue = "???";
		string text = pCulture?.getColor().color_text;
		if (pCulture != null)
		{
			pValue = pCulture.data.name;
			pValue += Toolbox.coloredGreyPart(pCulture.units.Count, text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Culture, pCulture?.getID() ?? (-1), pColorText: false, "iconCulture");
	}

	private void showStatsMetaLanguage(Language pLanguage, string pTitle = "language")
	{
		string pValue = "???";
		string text = pLanguage?.getColor().color_text;
		if (pLanguage != null)
		{
			pValue = pLanguage.data.name;
			pValue += Toolbox.coloredGreyPart(pLanguage.units.Count, text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Language, pLanguage?.getID() ?? (-1), pColorText: false, "iconLanguage");
	}

	private void showStatsMetaReligion(Religion pReligion, string pTitle = "religion")
	{
		string pValue = "???";
		string text = pReligion?.getColor().color_text;
		if (pReligion != null)
		{
			pValue = pReligion.data.name;
			pValue += Toolbox.coloredGreyPart(pReligion.units.Count, text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Religion, pReligion?.getID() ?? (-1), pColorText: false, "iconReligion");
	}

	private void showStatsMetaClan(Clan pClan, string pTitle = "clan")
	{
		string pValue = "???";
		string text = pClan?.getColor().color_text;
		if (pClan != null)
		{
			pValue = pClan.data.name;
			pValue += Toolbox.coloredGreyPart(pClan.units.Count, text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Clan, pClan?.getID() ?? (-1), pColorText: false, "iconClan");
	}

	private void showStatsMetaSubspecies(Subspecies pSubspecies, string pTitle = "subspecies")
	{
		string pValue = "???";
		string text = pSubspecies?.getColor().color_text;
		if (pSubspecies != null)
		{
			pValue = pSubspecies.data.name;
			pValue += Toolbox.coloredGreyPart(pSubspecies.units.Count, text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Subspecies, pSubspecies?.getID() ?? (-1), pColorText: false, "iconSpecies");
	}

	private void showStatsMetaFamily(Family pFamily, string pTitle = "family")
	{
		string color_text = pFamily.getColor().color_text;
		string name = pFamily.name;
		name += Toolbox.coloredGreyPart(pFamily.units.Count, color_text);
		showStatRowMeta(pTitle, name, color_text, MetaType.Family, pFamily.getID(), pColorText: false, "iconFamily");
	}

	private void showStatsMetaAlliance(Alliance pAlliance, string pTitle = "alliance")
	{
		string pValue = "???";
		string text = pAlliance?.getColor().color_text;
		if (pAlliance != null)
		{
			pValue = pAlliance.data.name;
			pValue += Toolbox.coloredGreyPart(pAlliance.countPopulation(), text);
		}
		showStatRowMeta(pTitle, pValue, text, MetaType.Alliance, pAlliance?.getID() ?? (-1), pColorText: false, "iconAlliance");
	}

	protected void tryToShowMetaFamily(string pTitle = "family", long pID = -1L, string pName = null, Family pObject = null)
	{
		Family family = ((pObject != null) ? pObject : World.world.families.get(pID));
		if (!family.isRekt())
		{
			showStatsMetaFamily(family, pTitle);
		}
		else
		{
			showEmptyStatRow(pTitle, pName, "iconFamily");
		}
	}

	protected void tryToShowMetaCulture(string pTitle = "culture", long pID = -1L, string pName = null, Culture pObject = null)
	{
		Culture culture = ((pObject != null) ? pObject : World.world.cultures.get(pID));
		if (!culture.isRekt())
		{
			showStatsMetaCulture(culture, pTitle);
		}
		else
		{
			showEmptyStatRow(pTitle, pName, "iconCulture");
		}
	}

	protected void tryToShowMetaLanguage(string pTitle = "language", long pID = -1L, string pName = null, Language pObject = null)
	{
		Language language = ((pObject != null) ? pObject : World.world.languages.get(pID));
		if (!language.isRekt())
		{
			showStatsMetaLanguage(language, pTitle);
		}
		else
		{
			showEmptyStatRow(pTitle, pName, "iconLanguage");
		}
	}

	protected void tryToShowMetaReligion(string pTitle = "religion", long pID = -1L, string pName = null, Religion pObject = null)
	{
		Religion religion = ((pObject != null) ? pObject : World.world.religions.get(pID));
		if (!religion.isRekt())
		{
			showStatsMetaReligion(religion, pTitle);
		}
		else
		{
			showEmptyStatRow(pTitle, pName, "iconReligion");
		}
	}

	protected void tryToShowMetaAlliance(string pTitle = "alliance", long pID = -1L, string pName = null, Alliance pObject = null)
	{
		Alliance alliance = ((pObject != null) ? pObject : World.world.alliances.get(pID));
		if (!alliance.isRekt())
		{
			showStatsMetaAlliance(alliance, pTitle);
		}
		else
		{
			showEmptyStatRow(pTitle, pName, "iconAlliance");
		}
	}

	protected void tryToShowMetaCity(string pTitle, long pID = -1L, string pName = null, City pObject = null, string pIconPath = "iconCity")
	{
		City city = ((pObject != null) ? pObject : World.world.cities.get(pID));
		if (!city.isRekt())
		{
			showStatsMetaCity(city, pTitle);
			return;
		}
		string pContent = ((pID != -1 && !string.IsNullOrEmpty(pName)) ? ("† " + pName) : pName);
		showEmptyStatRow(pTitle, pContent, pIconPath);
	}

	protected void tryToShowMetaSubspecies(string pTitle = "main_subspecies", long pID = -1L, string pName = null, Subspecies pObject = null)
	{
		Subspecies subspecies = ((pObject != null) ? pObject : World.world.subspecies.get(pID));
		if (!subspecies.isRekt())
		{
			showStatsMetaSubspecies(subspecies, pTitle);
			return;
		}
		string pContent = ((pID != -1 && !string.IsNullOrEmpty(pName)) ? ("† " + pName) : pName);
		showEmptyStatRow(pTitle, pContent, "iconSpecies");
	}

	protected void tryToShowMetaKingdom(string pTitle = "kingdom", long pID = -1L, string pName = null, Kingdom pObject = null)
	{
		Kingdom kingdom = ((pObject != null) ? pObject : (World.world.kingdoms.get(pID) ?? World.world.kingdoms.db_get(pID)));
		if (!kingdom.isRekt())
		{
			showStatsMetaKingdom(kingdom, pTitle);
			return;
		}
		string pContent = ((pID != -1 && !string.IsNullOrEmpty(pName)) ? ("† " + pName) : pName);
		showEmptyStatRow(pTitle, pContent, "iconKingdom");
	}

	protected void tryToShowMetaClan(string pTitle = "clan", long pID = -1L, string pName = null, Clan pObject = null)
	{
		Clan clan = ((pObject != null) ? pObject : World.world.clans.get(pID));
		if (!clan.isRekt())
		{
			showStatsMetaClan(clan, pTitle);
			return;
		}
		string pContent = ((pID != -1 && !string.IsNullOrEmpty(pName)) ? ("† " + pName) : pName);
		showEmptyStatRow(pTitle, pContent, "iconClan");
	}

	protected void tryToShowActor(string pTitle, long pID = -1L, string pName = null, Actor pObject = null, string pIconPath = null)
	{
		Actor actor = ((pObject != null) ? pObject : World.world.units.get(pID));
		if (!actor.isRekt())
		{
			showStatsMetaUnit(actor, pTitle, pIconPath);
			return;
		}
		string pContent = ((pID != -1 && !string.IsNullOrEmpty(pName)) ? ("† " + pName) : pName);
		showEmptyStatRow(pTitle, pContent, pIconPath);
	}

	private void showEmptyStatRow(string pTitle, string pContent = null, string pIconPath = null)
	{
		showEmptyStatRow(pTitle, stats_rows_container, pContent, pIconPath);
	}

	private static void showEmptyStatRow(string pTitle, StatsRowsContainer pContainer, string pContent = null, string pIconPath = null)
	{
		if (string.IsNullOrEmpty(pContent))
		{
			pContent = "???";
		}
		if (pContent == "neutral")
		{
			pContent = "???";
		}
		pContainer.showStatRow(pTitle, pContent, ColorStyleLibrary.m.color_dead_text, MetaType.None, -1L, pColorText: false, pIconPath);
	}

	protected void tryToShowMetaSpecies(string pTitle, string pId)
	{
		tryToShowMetaSpecies(pTitle, pId, stats_rows_container);
	}

	public static void tryToShowMetaSpecies(string pTitle, string pId, StatsRowsContainer pContainer)
	{
		string text = "???";
		if (!string.IsNullOrEmpty(pId))
		{
			ActorAsset actorAsset = AssetManager.actor_library.get(pId);
			if (actorAsset != null)
			{
				text = actorAsset.getTranslatedName();
				pContainer.showStatRow(pTitle, text, null, MetaType.None, -1L, pColorText: false, "iconGene", "unit_species", actorAsset.getTooltip);
				return;
			}
		}
		showEmptyStatRow(pTitle, pContainer);
	}
}
