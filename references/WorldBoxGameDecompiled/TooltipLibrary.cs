using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[ObfuscateLiterals]
public class TooltipLibrary : AssetLibrary<TooltipAsset>
{
	private BaseStats _base_stats_temp = new BaseStats();

	private const string ARROW_UP_1 = " <size=4>↗</size>";

	private const string ARROW_UP_2 = " <size=4>↗↗</size>";

	private const string ARROW_UP_3 = " <size=4>↗↗↗</size>";

	private const string ARROW_DOWN_1 = " <size=4>↘</size>";

	private const string ARROW_DOWN_2 = " <size=4>↘↘</size>";

	private const string ARROW_DOWN_3 = " <size=4>↘↘↘</size>";

	public override void init()
	{
		base.init();
		add(new TooltipAsset
		{
			id = "normal",
			callback = showNormal
		});
		add(new TooltipAsset
		{
			id = "neuron",
			callback = showNeuron,
			callback_text_animated = showNeuron
		});
		add(new TooltipAsset
		{
			id = "biome_seed",
			prefab_id = "tooltips/tooltip_biome_seed",
			callback = showBiomeSeed
		});
		add(new TooltipAsset
		{
			id = "unit_spawn",
			prefab_id = "tooltips/tooltip_species_spawn",
			callback = showUnitSpawn
		});
		add(new TooltipAsset
		{
			id = "unit_species",
			prefab_id = "tooltips/tooltip_species_spawn",
			callback = showUnitSpecies
		});
		add(new TooltipAsset
		{
			id = "unit_button",
			prefab_id = "tooltips/tooltip_species_spawn",
			callback = showUnitButton
		});
		add(new TooltipAsset
		{
			id = "onomastics_asset",
			callback = showOnomastics
		});
		add(new TooltipAsset
		{
			id = "trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showTrait
		});
		add(new TooltipAsset
		{
			id = "culture_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showCultureTrait
		});
		add(new TooltipAsset
		{
			id = "language_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showLanguageTrait
		});
		add(new TooltipAsset
		{
			id = "subspecies_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showSubspeciesTrait
		});
		add(new TooltipAsset
		{
			id = "clan_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showClanTrait
		});
		add(new TooltipAsset
		{
			id = "religion_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showReligionTrait
		});
		add(new TooltipAsset
		{
			id = "kingdom_trait",
			prefab_id = "tooltips/tooltip_trait",
			callback = showKingdomTrait
		});
		add(new TooltipAsset
		{
			id = "chromosome",
			callback = showChromosome
		});
		add(new TooltipAsset
		{
			id = "gene",
			callback = showGene,
			callback_text_animated = showGeneDNASequence
		});
		add(new TooltipAsset
		{
			id = "status",
			prefab_id = "tooltips/tooltip_status",
			callback = showStatus
		});
		add(new TooltipAsset
		{
			id = "status_updatable",
			prefab_id = "tooltips/tooltip_status",
			callback = showStatus,
			callback_text_animated = showStatus
		});
		add(new TooltipAsset
		{
			id = "culture",
			prefab_id = "tooltips/tooltip_culture",
			callback = showCulture
		});
		add(new TooltipAsset
		{
			id = "subspecies",
			prefab_id = "tooltips/tooltip_subspecies",
			callback = showSubspecies
		});
		add(new TooltipAsset
		{
			id = "family",
			prefab_id = "tooltips/tooltip_family",
			callback = showFamily
		});
		add(new TooltipAsset
		{
			id = "language",
			prefab_id = "tooltips/tooltip_language",
			callback = showLanguage
		});
		add(new TooltipAsset
		{
			id = "religion",
			prefab_id = "tooltips/tooltip_religion",
			callback = showReligion
		});
		add(new TooltipAsset
		{
			id = "book",
			prefab_id = "tooltips/tooltip_book",
			callback = showBook
		});
		add(new TooltipAsset
		{
			id = "clan",
			prefab_id = "tooltips/tooltip_clan",
			callback = showClan
		});
		add(new TooltipAsset
		{
			id = "army",
			prefab_id = "tooltips/tooltip_army",
			callback = showArmy
		});
		add(new TooltipAsset
		{
			id = "alliance",
			prefab_id = "tooltips/tooltip_alliance",
			callback = showAlliance
		});
		add(new TooltipAsset
		{
			id = "kingdom",
			prefab_id = "tooltips/tooltip_kingdom",
			callback = showKingdom
		});
		add(new TooltipAsset
		{
			id = "kingdom_dead",
			prefab_id = "tooltips/tooltip_kingdom_dead",
			callback = showDeadKingdom
		});
		add(new TooltipAsset
		{
			id = "kingdom_diplo",
			callback = showKingdom,
			prefab_id = "tooltips/tooltip_kingdom_opinion"
		});
		TooltipAsset tooltipAsset = t;
		tooltipAsset.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset.callback, new TooltipShowAction(opinionListToStatsDiplomacy));
		add(new TooltipAsset
		{
			id = "city",
			prefab_id = "tooltips/tooltip_city",
			callback = showCity
		});
		add(new TooltipAsset
		{
			id = "plot",
			prefab_id = "tooltips/tooltip_plot",
			callback = showPlot
		});
		add(new TooltipAsset
		{
			id = "plot_in_editor",
			prefab_id = "tooltips/tooltip_plot_editor",
			callback = showPlotInEditor
		});
		add(new TooltipAsset
		{
			id = "happiness",
			prefab_id = "tooltips/tooltip_happiness",
			callback = showHappiness
		});
		add(new TooltipAsset
		{
			id = "city_capital",
			prefab_id = "tooltips/tooltip_city",
			callback = showCityCapital
		});
		add(new TooltipAsset
		{
			id = "city_home",
			prefab_id = "tooltips/tooltip_city",
			callback = showCityHome
		});
		add(new TooltipAsset
		{
			id = "actor_king",
			prefab_id = "tooltips/tooltip_actor",
			callback = showKing
		});
		add(new TooltipAsset
		{
			id = "actor",
			prefab_id = "tooltips/tooltip_actor",
			callback = showActorNormal
		});
		add(new TooltipAsset
		{
			id = "actor_leader",
			prefab_id = "tooltips/tooltip_actor",
			callback = showLeader
		});
		add(new TooltipAsset
		{
			id = "map_meta",
			callback = showMapMeta
		});
		add(new TooltipAsset
		{
			id = "equipment",
			prefab_id = "tooltips/tooltip_equipment",
			callback = showEquipment
		});
		add(new TooltipAsset
		{
			id = "equipment_in_editor",
			prefab_id = "tooltips/tooltip_equipment_in_editor",
			callback = showEquipmentInEditor
		});
		add(new TooltipAsset
		{
			id = "city_resource",
			callback = showCityResource,
			callback_text_animated = showCityResource
		});
		add(new TooltipAsset
		{
			id = "city_resource_food",
			callback = showCityResourceFood,
			callback_text_animated = showCityResourceFood
		});
		add(new TooltipAsset
		{
			id = "graph_resource",
			callback = showGraphResource
		});
		add(new TooltipAsset
		{
			id = "graph_multi_resource",
			callback = showGraphMultiResource
		});
		add(new TooltipAsset
		{
			id = "gender_data",
			callback = showGenderData
		});
		add(new TooltipAsset
		{
			id = "war",
			prefab_id = "tooltips/tooltip_war",
			callback = showWar
		});
		TooltipAsset tooltipAsset2 = t;
		tooltipAsset2.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset2.callback, new TooltipShowAction(showWarSides));
		add(new TooltipAsset
		{
			id = "world_law",
			callback = showWorldLaw
		});
		add(new TooltipAsset
		{
			id = "world_age",
			prefab_id = "tooltips/tooltip_world_age",
			callback = showWorldAge
		});
		add(new TooltipAsset
		{
			id = "tip",
			callback = showTip
		});
		add(new TooltipAsset
		{
			id = "tip_zone_mode",
			callback = showTipZoneMode
		});
		add(new TooltipAsset
		{
			id = "stats_icon",
			callback = showTip
		});
		TooltipAsset tooltipAsset3 = t;
		tooltipAsset3.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset3.callback, new TooltipShowAction(showStatsData));
		add(new TooltipAsset
		{
			id = "debug_kingdom_assets",
			callback = showKingdomAsset
		});
		add(new TooltipAsset
		{
			id = "mass",
			callback = showMass
		});
		add(new TooltipAsset
		{
			id = "past_rulers",
			prefab_id = "tooltips/tooltip_past_rulers",
			callback = showPastRulers
		});
		add(new TooltipAsset
		{
			id = "past_names",
			prefab_id = "tooltips/tooltip_past_rulers",
			callback = showPastNames
		});
		add(new TooltipAsset
		{
			id = "carrying_resources",
			callback = showCarryingResources
		});
		add(new TooltipAsset
		{
			id = "passengers",
			prefab_id = "tooltips/tooltip_passengers",
			callback = showPassengers
		});
		add(new TooltipAsset
		{
			id = "loyalty",
			callback = showNormal
		});
		TooltipAsset tooltipAsset4 = t;
		tooltipAsset4.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset4.callback, new TooltipShowAction(showLoyalty));
		TooltipAsset tooltipAsset5 = t;
		tooltipAsset5.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset5.callback, new TooltipShowAction(opinionListToStatsLoyalty));
		add(new TooltipAsset
		{
			id = "taxonomy",
			prefab_id = "tooltips/tooltip_taxonomy",
			callback = showTaxonomy
		});
		add(new TooltipAsset
		{
			id = "achievement",
			prefab_id = "tooltips/tooltip_achievement",
			callback = showAchievement
		});
		add(new TooltipAsset
		{
			id = "color_counter",
			callback = showNormal
		});
		TooltipAsset tooltipAsset6 = t;
		tooltipAsset6.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset6.callback, new TooltipShowAction(showColorCounter));
		add(new TooltipAsset
		{
			id = "game_language",
			callback = showGameLanguage
		});
		addMetaListButtonTooltips();
		initDebug();
	}

	private void showMetaInfo(Tooltip pTooltip, string pAssetId, string pStatisticID)
	{
		MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.get(pAssetId);
		int num = 0;
		foreach (NanoObject item in metaTypeAsset.get_list())
		{
			if (!item.isRekt() && !item.hasDied())
			{
				num++;
			}
		}
		setIconValue(pTooltip, "i_total", num);
		setIconValue(pTooltip, "i_destroyed", StatsHelper.getStat(pStatisticID));
		setIconSprite(pTooltip, "i_total", metaTypeAsset.icon_list);
	}

	private void addMetaListButtonTooltips()
	{
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_subspecies",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset = t;
		tooltipAsset.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "subspecies", "world_statistics_subspecies_extinct");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_languages",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset2 = t;
		tooltipAsset2.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset2.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "language", "world_statistics_languages_forgotten");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_families",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset3 = t;
		tooltipAsset3.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset3.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "family", "world_statistics_families_destroyed");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_cultures",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset4 = t;
		tooltipAsset4.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset4.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "culture", "world_statistics_cultures_forgotten");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_religions",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset5 = t;
		tooltipAsset5.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset5.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "religion", "world_statistics_religions_forgotten");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_clans",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset6 = t;
		tooltipAsset6.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset6.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "clan", "world_statistics_clans_destroyed");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_cities",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset7 = t;
		tooltipAsset7.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset7.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "city", "world_statistics_cities_destroyed");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_kingdoms",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset8 = t;
		tooltipAsset8.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset8.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "kingdom", "world_statistics_kingdoms_destroyed");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_armies",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset9 = t;
		tooltipAsset9.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset9.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "army", "world_statistics_armies_destroyed");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_alliances",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset10 = t;
		tooltipAsset10.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset10.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "alliance", "world_statistics_alliances_made");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_wars",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset11 = t;
		tooltipAsset11.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset11.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "war", "world_statistics_peaces_made");
		});
		add(new TooltipAsset
		{
			id = "tooltip_meta_list_plots",
			prefab_id = "tooltips/tooltip_meta_list",
			callback = showNormal
		});
		TooltipAsset tooltipAsset12 = t;
		tooltipAsset12.callback = (TooltipShowAction)Delegate.Combine(tooltipAsset12.callback, (TooltipShowAction)delegate(Tooltip pTooltip, string _, TooltipData _)
		{
			showMetaInfo(pTooltip, "plot", "world_statistics_plots_succeeded");
		});
	}

	private void showNormal(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (!string.IsNullOrEmpty(pData.tip_name))
		{
			pTooltip.name.text = pData.tip_name.Localize();
		}
		if (!string.IsNullOrEmpty(pData.tip_description))
		{
			string pDescription = pData.tip_description.Localize();
			pTooltip.setDescription(pDescription);
		}
		if (Config.isComputer || Config.isEditor)
		{
			string text = pData.tip_description_2;
			if (string.IsNullOrEmpty(text))
			{
				text = pData.tip_description + "_2";
			}
			if (!string.IsNullOrEmpty(text) && LocalizedTextManager.stringExists(text))
			{
				string pText = text.Localize();
				pText = AssetManager.hotkey_library.replaceSpecialTextKeys(pText);
				pTooltip.setBottomDescription(pText);
			}
		}
	}

	private void showNeuron(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		NeuronElement neuron = pData.neuron;
		DecisionAsset decision = neuron.decision;
		Actor actor = neuron.actor;
		NeuralLayerAsset asset = decision.priority.GetAsset();
		pTooltip.clearTextRows();
		pTooltip.setTitle(decision.getLocalizedText(), "neuron", asset.color_hex);
		if (decision.unique)
		{
			((Graphic)pTooltip.name).color = RarityLibrary.legendary.color_container.color;
		}
		else
		{
			((Graphic)pTooltip.name).color = RarityLibrary.rare.color_container.color;
		}
		pTooltip.setDescription("neuron_description".Localize());
		bool flag = actor.isDecisionEnabled(decision.decision_index);
		pTooltip.addLineText("neuron_state", flag ? LocalizedTextManager.getText("neuron_active") : LocalizedTextManager.getText("neuron_silenced"), flag ? "#43FF43" : "#FB2C21");
		pTooltip.addLineBreak();
		pTooltip.addLineText("neuro_layer", asset.getLocaleID().Localize(), asset.color_hex);
		pTooltip.addLineText("neuro_layer_priority", asset.getDescriptionID().Localize(), asset.color_hex);
		pTooltip.addLineText("neuron_firing_rate", decision.getFiringRate());
		pTooltip.addLineText("neuron_cooldown", neuron.getSimulatedTimer().ToText() + "s");
		if (actor.isDecisionOnCooldown(decision.decision_index, decision.cooldown))
		{
			pTooltip.resetBottomDescription();
			pTooltip.addBottomDescription("neuron_on_refractory_period".Localize());
		}
	}

	private void showBiomeSeed(Tooltip pTooltip, string pType, TooltipData pData)
	{
		GodPower power = pData.power;
		string biome_id = AssetManager.drops.get(power.drop_id).cached_drop_type_low.biome_id;
		BiomeAsset biomeAsset = AssetManager.biome_library.get(biome_id);
		using ListPool<string> listPool = new ListPool<string>();
		TooltipIconsRow component = ((Component)((Component)pTooltip).transform.FindRecursive("Traits")).GetComponent<TooltipIconsRow>();
		bool flag = false;
		flag |= showBiomeTraits(biomeAsset.spawn_trait_actor, AssetManager.traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_subspecies, AssetManager.subspecies_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.evolution_trait_subspecies, AssetManager.subspecies_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_subspecies_always, AssetManager.subspecies_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_culture, AssetManager.culture_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_language, AssetManager.language_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_religion, AssetManager.religion_traits, component, pTooltip, pData);
		flag |= showBiomeTraits(biomeAsset.spawn_trait_clan, AssetManager.clan_traits, component, pTooltip, pData);
		((Component)component).gameObject.SetActive(flag);
		if (flag)
		{
			component.init(pTooltip, pData);
		}
		if (pTooltip.pool_icons == null)
		{
			Transform pParentTransform = ((Component)pTooltip).transform.FindRecursive("Species");
			StatsIcon pPrefab = Resources.Load<StatsIcon>("ui/PrefabTextIconTooltipBig");
			pTooltip.pool_icons = new ObjectPoolGenericMono<StatsIcon>(pPrefab, pParentTransform);
		}
		listPool.Clear();
		if (biomeAsset.pot_units_spawn != null)
		{
			foreach (string item in biomeAsset.pot_units_spawn)
			{
				if (!listPool.Contains(item))
				{
					listPool.Add(item);
					showBiomeSeedUnit(item, pTooltip);
				}
			}
		}
		if (WorldLawLibrary.world_law_drop_of_thoughts.isEnabled() && biomeAsset.pot_sapient_units_spawn != null)
		{
			foreach (string item2 in biomeAsset.pot_sapient_units_spawn)
			{
				if (!listPool.Contains(item2))
				{
					listPool.Add(item2);
					showBiomeSeedUnit(item2, pTooltip);
				}
			}
		}
		showNormal(pTooltip, pType, pData);
	}

	private void showBiomeSeedUnit(string pId, Tooltip pTooltip)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		StatsIcon next = pTooltip.pool_icons.getNext();
		Image icon = next.getIcon();
		ActorAsset actorAsset = AssetManager.actor_library.get(pId);
		icon.sprite = actorAsset.getSpriteIcon();
		next.text.text = actorAsset.getTranslatedName();
		if (actorAsset.isAvailable())
		{
			((Graphic)icon).color = Toolbox.color_white;
		}
		else
		{
			((Graphic)icon).color = Toolbox.color_black;
		}
	}

	private bool showBiomeTraits<T>(List<string> pTraits, BaseTraitLibrary<T> pLibrary, TooltipIconsRow pRow, Tooltip pTooltip, TooltipData pData) where T : BaseTrait<T>
	{
		if (pTraits != null)
		{
			_ = pTraits.Count;
			if (0 == 0)
			{
				foreach (string pTrait in pTraits)
				{
					T val = pLibrary.get(pTrait);
					string pColor = (val.isAvailable() ? "#FFFFFF" : "#000000");
					Sprite sprite = val.getSprite();
					pRow.addIcon(sprite, pColor);
				}
				return true;
			}
		}
		return false;
	}

	private void showUnitSpawn(Tooltip pTooltip, string pType, TooltipData pData)
	{
		GodPower power = pData.power;
		string actorAssetID = power.getActorAssetID();
		bool show_unit_stats_overview = power.show_unit_stats_overview;
		showUnitGeneric(pTooltip, pData, actorAssetID, show_unit_stats_overview);
		checkDebugSpeciesRows(pTooltip, pData, actorAssetID);
	}

	private void checkDebugSpeciesRows(Tooltip pTooltip, TooltipData pData, string pActorAssetID)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get(pActorAssetID);
		showDebugRowsIcons(pTooltip, pData, "IconsRowActor", actorAsset.traits, AssetManager.traits);
		showDebugRowsIcons(pTooltip, pData, "IconsRowSubspecies", actorAsset.default_subspecies_traits, AssetManager.subspecies_traits);
		showDebugRowsIcons(pTooltip, pData, "IconsRowClan", actorAsset.default_clan_traits, AssetManager.clan_traits);
		showDebugRowsIcons(pTooltip, pData, "IconsRowLanguage", actorAsset.default_language_traits, AssetManager.language_traits);
		showDebugRowsIcons(pTooltip, pData, "IconsRowCulture", actorAsset.default_culture_traits, AssetManager.culture_traits);
		showDebugRowsIcons(pTooltip, pData, "IconsRowReligion", actorAsset.default_religion_traits, AssetManager.religion_traits);
	}

	private void showDebugRowsIcons<TTraitType>(Tooltip pTooltip, TooltipData pData, string pRowName, List<string> pTraitsList, BaseTraitLibrary<TTraitType> pTraitLibrary) where TTraitType : BaseTrait<TTraitType>
	{
		TooltipIconsRow component = ((Component)((Component)pTooltip).transform.FindRecursive(pRowName)).GetComponent<TooltipIconsRow>();
		bool flag = DebugConfig.isOn(DebugOption.DebugPowerBarTooltipSpeciesTraits);
		if ((pTraitsList != null) & flag)
		{
			foreach (string pTraits in pTraitsList)
			{
				TTraitType val = pTraitLibrary.get(pTraits);
				component.addIcon(val.getSprite());
			}
		}
		component.init(pTooltip, pData);
	}

	private void showUnitSpecies(Tooltip pTooltip, string pType, TooltipData pData)
	{
		string actorAssetID = pData.power.getActorAssetID();
		showUnitGeneric(pTooltip, pData, actorAssetID, pShowStatsOverview: true, pShowStats: false);
		checkDebugSpeciesRows(pTooltip, pData, actorAssetID);
	}

	private void showUnitButton(Tooltip pTooltip, string pType, TooltipData pData)
	{
		string text = pData.actor_asset.id;
		showUnitGeneric(pTooltip, pData, text, pShowStatsOverview: true);
		checkDebugSpeciesRows(pTooltip, pData, text);
	}

	private void showUnitGeneric(Tooltip pTooltip, TooltipData pData, string pActorAssetId, bool pShowStatsOverview, bool pShowStats = true)
	{
		Transform val = ((Component)pTooltip).transform.FindRecursive("Stats");
		bool active = false;
		if (pShowStatsOverview && !string.IsNullOrEmpty(pActorAssetId))
		{
			ActorAsset actorAsset = AssetManager.actor_library.get(pActorAssetId);
			if (actorAsset != null)
			{
				pTooltip.name.text = actorAsset.getLocalizedName();
				pTooltip.setDescription(actorAsset.getLocalizedDescription());
				if (!actorAsset.isAvailable())
				{
					((Component)val).gameObject.SetActive(false);
					return;
				}
				if (pShowStats && DebugConfig.isOn(DebugOption.DebugPowerBarTooltipSpeciesTraits))
				{
					BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, actorAsset.getStatsForOverview(), pAddPlus: false);
				}
				if (actorAsset.can_have_subspecies)
				{
					active = true;
					setIconValue(pTooltip, "i_population", actorAsset.countPopulation());
					setIconValue(pTooltip, "i_subspecies", actorAsset.countSubspecies());
					setIconValue(pTooltip, "i_families", actorAsset.countFamilies());
				}
				if (DebugConfig.isOn(DebugOption.DebugPowerBarTooltipTaxonomy))
				{
					showDebugTaxonomy(pTooltip, actorAsset);
				}
				if (DebugConfig.isOn(DebugOption.DebugPowerBarTooltipStartingCivMetas))
				{
					showDebugTraits(pTooltip, actorAsset);
				}
			}
		}
		((Component)val).gameObject.SetActive(active);
		if (!string.IsNullOrEmpty(pData.tip_description))
		{
			string text = LocalizedTextManager.getText(pData.tip_description);
			text = text.Replace("$lifeissimhours$", 24f.ToText());
			pTooltip.setDescription(text);
		}
		if ((Config.isComputer || Config.isEditor) && !string.IsNullOrEmpty(pData.tip_description_2))
		{
			string text2 = LocalizedTextManager.getText(pData.tip_description_2);
			text2 = AssetManager.hotkey_library.replaceSpecialTextKeys(text2);
			pTooltip.setBottomDescription(text2);
		}
	}

	private void showDebugTraits(Tooltip pTooltip, ActorAsset pAsset)
	{
		pTooltip.addLineBreak();
		if (pAsset.default_language_traits != null)
		{
			pTooltip.addLineIntText("language_traits", pAsset.default_language_traits.Count, "#4CCFFF", pLocalize: false);
			foreach (string default_language_trait in pAsset.default_language_traits)
			{
				pTooltip.addLineText("trait", default_language_trait, "#4CCFFF", pPercent: false, pLocalize: false);
			}
		}
		else
		{
			pTooltip.addLineText("language_traits", "-----", "#4CCFFF", pPercent: false, pLocalize: false);
		}
		if (pAsset.default_clan_traits != null)
		{
			pTooltip.addLineIntText("clan_traits", pAsset.default_clan_traits.Count, "#FF637D", pLocalize: false);
			foreach (string default_clan_trait in pAsset.default_clan_traits)
			{
				pTooltip.addLineText("trait", default_clan_trait, "#4CCFFF", pPercent: false, pLocalize: false);
			}
		}
		else
		{
			pTooltip.addLineText("clan_traits", "-----", "#FF637D", pPercent: false, pLocalize: false);
		}
		if (pAsset.default_culture_traits != null)
		{
			pTooltip.addLineIntText("culture_traits", pAsset.default_culture_traits.Count, "#35CC6E", pLocalize: false);
			foreach (string default_culture_trait in pAsset.default_culture_traits)
			{
				pTooltip.addLineText("trait", default_culture_trait, "#35CC6E", pPercent: false, pLocalize: false);
			}
		}
		else
		{
			pTooltip.addLineText("culture_traits", "-----", "#35CC6E", pPercent: false, pLocalize: false);
		}
		if (pAsset.default_religion_traits != null)
		{
			pTooltip.addLineIntText("religions_traits", pAsset.default_religion_traits.Count, "#8CFF99", pLocalize: false);
			{
				foreach (string default_religion_trait in pAsset.default_religion_traits)
				{
					pTooltip.addLineText("trait", default_religion_trait, "#8CFF99", pPercent: false, pLocalize: false);
				}
				return;
			}
		}
		pTooltip.addLineText("religions_traits", "-----", "#8CFF99", pPercent: false, pLocalize: false);
	}

	private void showDebugTaxonomy(Tooltip pTooltip, ActorAsset pAsset)
	{
		pTooltip.addLineBreak();
		pTooltip.addLineText("kingdom", pAsset.getTaxonomyRank("taxonomy_kingdom"), ColorStyleLibrary.m.taxonomy_kingdom, pPercent: false, pLocalize: false);
		pTooltip.addLineText("phylum", pAsset.getTaxonomyRank("taxonomy_phylum"), ColorStyleLibrary.m.taxonomy_phylum, pPercent: false, pLocalize: false);
		pTooltip.addLineText("class", pAsset.getTaxonomyRank("taxonomy_class"), ColorStyleLibrary.m.taxonomy_class, pPercent: false, pLocalize: false);
		pTooltip.addLineText("order", pAsset.getTaxonomyRank("taxonomy_order"), ColorStyleLibrary.m.taxonomy_order, pPercent: false, pLocalize: false);
		pTooltip.addLineText("family", pAsset.getTaxonomyRank("taxonomy_family"), ColorStyleLibrary.m.taxonomy_family, pPercent: false, pLocalize: false);
		pTooltip.addLineText("genus", pAsset.getTaxonomyRank("taxonomy_genus"), ColorStyleLibrary.m.taxonomy_genus, pPercent: false, pLocalize: false);
		pTooltip.addLineText("species", pAsset.getTaxonomyRank("taxonomy_species"), ColorStyleLibrary.m.taxonomy_genus, pPercent: false, pLocalize: false);
	}

	private void showDeadKingdom(Tooltip pTooltip, string pType, TooltipData pData)
	{
		DeadKingdom deadKingdom = (DeadKingdom)pData.kingdom;
		pTooltip.setSpeciesIcon(deadKingdom.getSpeciesIcon());
		pTooltip.setTitle(deadKingdom.name, "kingdom", deadKingdom.getColor().color_text);
		setIconValue(pTooltip, "i_age", deadKingdom.getAge(), null, "#FF637D");
		setIconValue(pTooltip, "i_population", deadKingdom.getPopulationPeople(), null, "#FF637D");
		setIconValue(pTooltip, "i_army", deadKingdom.countTotalWarriors(), null, "#FF637D");
		pTooltip.setDescription(deadKingdom.getMotto());
		pTooltip.addLineText("founded", deadKingdom.getFoundedYear());
		pTooltip.addLineText("kingdom_died_at", deadKingdom.getDiedYear(), "#FF637D");
		pTooltip.addLineIntText("age", deadKingdom.getAge());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("births", deadKingdom.getTotalBirths());
		pTooltip.addLineIntText("deaths", deadKingdom.getTotalDeaths());
		pTooltip.addLineIntText("kills", deadKingdom.getTotalKills());
		pTooltip.addLineBreak();
		pTooltip.addLineText("species", deadKingdom.getActorAsset().getTranslatedName());
		KingdomBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<KingdomBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(deadKingdom);
		}
	}

	private void showKingdom(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Kingdom kingdom = pData.kingdom;
		pTooltip.setSpeciesIcon(kingdom.getSpeciesIcon());
		string color_text = kingdom.getColor().color_text;
		pTooltip.setTitle(kingdom.name, "kingdom", kingdom.getColor().color_text);
		((Component)((Component)pTooltip).transform.FindRecursive("Stats")).gameObject.SetActive(true);
		setIconValue(pTooltip, "i_age", kingdom.getAge());
		setIconValue(pTooltip, "i_population", kingdom.getPopulationPeople());
		setIconValue(pTooltip, "i_army", kingdom.countTotalWarriors());
		pTooltip.setDescription(kingdom.getMotto());
		string pValue = "-";
		if (kingdom.hasKing())
		{
			pValue = kingdom.king.getName();
		}
		pTooltip.addLineText("village_statistics_king", pValue, color_text);
		if (kingdom.hasKing())
		{
			pTooltip.addLineIntText("ruler_money", kingdom.king.money);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineText("villages", kingdom.cities.Count.ToText() + "/" + kingdom.getMaxCities().ToText());
		pTooltip.addLineIntText("adults", kingdom.countAdults());
		pTooltip.addLineIntText("children", kingdom.countChildren());
		pTooltip.addLineIntText("families", kingdom.countFamilies());
		pTooltip.addLineIntText("happy", kingdom.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("food", kingdom.countTotalFood());
		pTooltip.addLineBreak();
		string pValue2 = "-";
		if (kingdom.hasCapital())
		{
			pValue2 = kingdom.capital.name;
		}
		pTooltip.addLineText("kingdom_statistics_capital", pValue2, color_text);
		if (kingdom.hasKing() && kingdom.king.hasClan())
		{
			pTooltip.addLineText("clan", kingdom.king.clan.data.name, kingdom.king.clan.getColor().color_text);
		}
		if (kingdom.hasCulture())
		{
			pTooltip.addLineText("culture", kingdom.culture.data.name, kingdom.culture.getColor().color_text);
		}
		if (kingdom.hasLanguage())
		{
			pTooltip.addLineText("language", kingdom.language.data.name, kingdom.language.getColor().color_text);
		}
		if (kingdom.hasReligion())
		{
			pTooltip.addLineText("religion", kingdom.religion.data.name, kingdom.religion.getColor().color_text);
		}
		Alliance alliance = kingdom.getAlliance();
		if (alliance != null)
		{
			int yearsSince = Date.getYearsSince(kingdom.data.timestamp_alliance);
			pTooltip.addLineText("alliance", alliance.data.name, alliance.getColor().color_text);
			pTooltip.addLineIntText("kingdom_time_in_alliance", yearsSince, alliance.getColor().color_text);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("births", kingdom.getTotalBirths());
		pTooltip.addLineIntText("deaths", kingdom.getTotalDeaths());
		pTooltip.addLineIntText("kills", kingdom.getTotalKills());
		pTooltip.addLineBreak();
		pTooltip.addLineText("species", kingdom.getActorAsset().getTranslatedName());
		KingdomBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<KingdomBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(kingdom);
		}
		TooltipKingdomTraitsRow componentInChildren = ((Component)pTooltip).GetComponentInChildren<TooltipKingdomTraitsRow>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.init(pTooltip, pData);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showStatus(Tooltip pTooltip, string pType, TooltipData pData)
	{
		StatBar component = ((Component)((Component)pTooltip).transform.FindRecursive("TimeBar")).GetComponent<StatBar>();
		StatusAsset asset = pData.status.asset;
		if (!string.IsNullOrEmpty(pData.tip_name))
		{
			pTooltip.name.text = LocalizedTextManager.getText(pData.tip_name);
		}
		if (!string.IsNullOrEmpty(pData.tip_description))
		{
			pTooltip.setDescription(LocalizedTextManager.getText(pData.tip_description));
		}
		if (asset != null)
		{
			Status status = pData.status;
			component.setBar((int)status.getRemainingTime(), status.duration, "s", pReset: false);
			pTooltip.clearTextRows();
			BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, asset.base_stats);
		}
	}

	private void showOnomastics(Tooltip pTooltip, string pType, TooltipData pData)
	{
		OnomasticsAsset onomastics_asset = pData.onomastics_asset;
		string localeID = onomastics_asset.getLocaleID();
		string descriptionID = onomastics_asset.getDescriptionID();
		string iDSubname = onomastics_asset.getIDSubname();
		string text = LocalizedTextManager.getText(localeID);
		pTooltip.setTitle(text, iDSubname, onomastics_asset.color_text);
		string text2 = "";
		if (onomastics_asset.isGroupType() && !pData.onomastics_data.isGroupEmpty(onomastics_asset.id))
		{
			string characters_string = pData.onomastics_data.getGroup(onomastics_asset.id).characters_string;
			characters_string = characters_string.ToLower();
			text2 = text2 + "[ " + Toolbox.coloredText(characters_string, onomastics_asset.color_text) + " ]\n\n";
		}
		text2 += LocalizedTextManager.getText(descriptionID);
		pTooltip.setDescription(text2);
		string descriptionID2 = onomastics_asset.getDescriptionID2();
		if (!string.IsNullOrEmpty(descriptionID2))
		{
			string text3 = LocalizedTextManager.getText(descriptionID2);
			pTooltip.setBottomDescription(text3);
		}
	}

	private void showTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		ActorTrait trait = pData.trait;
		showGenericInfoForTrait(pTooltip, pData, trait);
	}

	private void showKingdomTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		KingdomTrait kingdom_trait = pData.kingdom_trait;
		showGenericInfoForTrait(pTooltip, pData, kingdom_trait);
	}

	private void showCultureTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		CultureTrait culture_trait = pData.culture_trait;
		showGenericInfoForTrait(pTooltip, pData, culture_trait);
	}

	private void showLanguageTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		LanguageTrait language_trait = pData.language_trait;
		showGenericInfoForTrait(pTooltip, pData, language_trait);
	}

	private void showSubspeciesTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		SubspeciesTrait subspecies_trait = pData.subspecies_trait;
		showGenericInfoForTrait(pTooltip, pData, subspecies_trait);
	}

	private void showClanTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		ClanTrait clan_trait = pData.clan_trait;
		showGenericInfoForTrait(pTooltip, pData, clan_trait, clan_trait.base_stats_male, clan_trait.base_stats_female);
	}

	private void showReligionTrait(Tooltip pTooltip, string pType, TooltipData pData)
	{
		ReligionTrait religion_trait = pData.religion_trait;
		showGenericInfoForTrait(pTooltip, pData, religion_trait);
	}

	private void showGenericInfoForTrait<T>(Tooltip pTooltip, TooltipData pData, T pTrait, params BaseStats[] pAdditionalBaseStats) where T : BaseTrait<T>
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		showTraitOwners(pTooltip, pTrait);
		bool flag = !pData.is_editor_augmentation_button || pTrait.isAvailable();
		Rarity rarity = pTrait.rarity;
		string text = rarity.getAsset().getLocaleID().Localize();
		string text2 = LocalizedTextManager.getText(flag ? pTrait.getLocaleID() : "achievement_tip_hidden");
		pTooltip.name.text = text2;
		((Graphic)pTooltip.name).color = rarity.getRarityColor();
		Text component = ((Component)((Component)pTooltip).transform.Find("Icon and Info/Background/Rarity Type/Rarity Text")).GetComponent<Text>();
		component.text = text;
		((Graphic)component).color = rarity.getRarityColor();
		Image component2 = ((Component)((Component)pTooltip).transform.Find("Icon and Info/IconBG/Icon")).GetComponent<Image>();
		component2.sprite = pTrait.getSprite();
		((Graphic)component2).color = (flag ? Toolbox.color_white : Toolbox.color_black);
		((Component)((Component)pTooltip).transform.Find("Icon and Info/IconBG/LegendaryBG")).gameObject.SetActive(rarity == Rarity.R3_Legendary);
		((Component)((Component)pTooltip).transform.Find("Icon and Info/Background/IconedText")).GetComponent<Text>().text = pTrait.getCountRows();
		GameObject gameObject = ((Component)((Component)pTooltip).transform.Find("Icon and Info/Background/Rarity Type/Rarity Stars")).gameObject;
		int num = (int)rarity;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			Image component3 = ((Component)gameObject.transform.GetChild(i)).gameObject.GetComponent<Image>();
			if (i <= num)
			{
				((Graphic)component3).color = Toolbox.makeColor("#313131");
			}
			else
			{
				((Graphic)component3).color = Color.black;
			}
		}
		string translatedDescription = pTrait.getTranslatedDescription();
		if (!string.IsNullOrEmpty(translatedDescription))
		{
			string text3 = translatedDescription;
			if (!pTrait.isAvailable() && pTrait.show_for_unlockables_ui)
			{
				if (pTrait.unlocked_with_achievement)
				{
					string text4 = LocalizedTextManager.getText("trait_locked_tooltip_text_achievement").ColorHex(ColorStyleLibrary.m.color_text_grey);
					string newValue = "<color=#00ffffff>" + pTrait.getAchievementLocaleID().Localize() + "</color>";
					text4 = text4.Replace("$achievement_id$", newValue);
					text3 = (pData.is_editor_augmentation_button ? text4 : (text3 + "\n\n" + text4));
				}
				else
				{
					text3 = LocalizedTextManager.getText(pTrait.typed_id + "_locked_tooltip_text_exploration");
				}
			}
			pTooltip.setDescription(text3);
		}
		else
		{
			pTooltip.resetDescription();
		}
		if (flag)
		{
			string translatedDescription2 = pTrait.getTranslatedDescription2();
			pTooltip.setBottomDescription(translatedDescription2);
		}
		if (!flag)
		{
			return;
		}
		BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, pTrait.base_stats);
		BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, pTrait.base_stats_meta, pAddPlus: false);
		if (pAdditionalBaseStats != null)
		{
			foreach (BaseStats pBaseStats in pAdditionalBaseStats)
			{
				BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, pBaseStats, pAddPlus: false);
			}
		}
	}

	private void showTraitOwners<T>(Tooltip pTooltip, T pTrait) where T : BaseTrait<T>
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Component)pTooltip).transform.FindRecursive("Species");
		if (pTrait.default_for_actor_assets == null)
		{
			((Component)val).gameObject.SetActive(false);
			return;
		}
		((Component)val).gameObject.SetActive(true);
		if (pTooltip.pool_icons == null)
		{
			StatsIcon pPrefab = Resources.Load<StatsIcon>("ui/PrefabTooltipTraitSpecies");
			pTooltip.pool_icons = new ObjectPoolGenericMono<StatsIcon>(pPrefab, val);
		}
		foreach (ActorAsset default_for_actor_asset in pTrait.default_for_actor_assets)
		{
			if (!default_for_actor_asset.unit_zombie && default_for_actor_asset.show_in_taxonomy_tooltip)
			{
				Image icon = pTooltip.pool_icons.getNext().getIcon();
				icon.sprite = default_for_actor_asset.getSpriteIcon();
				if (default_for_actor_asset.isAvailable())
				{
					((Graphic)icon).color = Toolbox.color_white;
				}
				else
				{
					((Graphic)icon).color = Toolbox.color_black;
				}
			}
		}
	}

	private void showChromosome(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Chromosome chromosome = pData.chromosome;
		ChromosomeTypeAsset asset = chromosome.getAsset();
		string localeID = asset.getLocaleID();
		((Component)pTooltip.name).GetComponent<LocalizedText>().setKeyAndUpdate(localeID);
		string text = LocalizedTextManager.getText(asset.getDescriptionID());
		pTooltip.setDescription(text);
		pTooltip.addLineText("genes", chromosome.countNonEmpty() + "/" + chromosome.genes.Count);
		pTooltip.addLineBreak();
		BaseStats totalStatsFrom = BaseStatsHelper.getTotalStatsFrom(chromosome.getStats(), chromosome.getStatsMeta());
		BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, totalStatsFrom);
	}

	private void showGene(Tooltip pTooltip, string pType, TooltipData pData)
	{
		GeneAsset gene = pData.gene;
		LocusElement locus = pData.locus;
		Chromosome chromosome = pData.chromosome;
		bool flag = (Object)(object)locus != (Object)null && locus.isAmplifier();
		bool num = gene.isAvailable();
		_ = chromosome?.isVoidLocus(locus.locus_index) ?? false;
		string text;
		if (!num)
		{
			text = LocalizedTextManager.getText("achievement_tip_hidden");
			if (gene.unlocked_with_achievement)
			{
				string text2 = LocalizedTextManager.getText("gene_locked_tooltip_text_achievement");
				string newValue = "<color=#00ffffff>" + gene.getAchievementLocaleID().Localize() + "</color>";
				text2 = text2.Replace("$achievement_id$", newValue);
				pTooltip.setDescription(text2);
			}
			else
			{
				pTooltip.setDescription(LocalizedTextManager.getText("gene_locked_tooltip_text_exploration"));
			}
			((Component)((Component)pTooltip).transform.FindRecursive("Stats")).gameObject.SetActive(false);
		}
		else
		{
			text = LocalizedTextManager.getText(gene.getLocaleID());
			string text3 = "";
			string descriptionID = gene.getDescriptionID();
			if (LocalizedTextManager.stringExists(descriptionID))
			{
				string text4 = LocalizedTextManager.getText(descriptionID);
				text3 = text3 + text4 + "\n";
			}
			pTooltip.setDescription(text3);
		}
		if ((Object)(object)locus != (Object)null)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (locus.isAmplifierBad())
			{
				empty = LocalizedTextManager.getText("amplifier_bad");
				string text5 = LocalizedTextManager.getText("amplifier_bad_description");
				pTooltip.setDescription(text5);
			}
			else if (locus.isAmplifier())
			{
				empty = LocalizedTextManager.getText("amplifier");
				string text6 = LocalizedTextManager.getText("amplifier_description");
				pTooltip.setDescription(text6);
			}
			else
			{
				empty = text;
			}
			empty2 = "locus";
			pTooltip.setTitle(empty, empty2);
		}
		else
		{
			pTooltip.setTitle(text, "gene");
		}
		string text7 = "";
		if (num && !gene.is_empty)
		{
			_base_stats_temp.clear();
			if ((Object)(object)locus != (Object)null && chromosome != null)
			{
				chromosome.fillStatsForTooltip(locus, _base_stats_temp);
			}
			else
			{
				_base_stats_temp.mergeStats(gene.base_stats);
			}
			if (chromosome != null && !flag)
			{
				text7 += chromosome.getSynergyTooltipText(locus.locus_index);
			}
			BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, _base_stats_temp);
		}
		if (!flag)
		{
			text7 = text7 + LocalizedTextManager.getText("dna_sequence") + "\n" + gene.getSequence();
		}
		pTooltip.setBottomDescription(text7);
	}

	private void showGeneDNASequence(Tooltip pTooltip, string pType, TooltipData pData)
	{
		GeneAsset gene = pData.gene;
		Chromosome chromosome = pData.chromosome;
		LocusElement locus = pData.locus;
		bool flag = false;
		if ((Object)(object)locus != (Object)null)
		{
			flag = locus.isAmplifier();
		}
		bool num = gene.isAvailable();
		string text = "";
		if (num && !flag && (Object)(object)locus != (Object)null && chromosome != null)
		{
			text += chromosome.getSynergyTooltipText(locus.locus_index);
		}
		if (!flag)
		{
			text = text + LocalizedTextManager.getText("dna_sequence") + "\n" + gene.getSequence();
		}
		pTooltip.setBottomDescription(text);
	}

	private void showKingdomAsset(Tooltip pTooltip, string pType, TooltipData pData)
	{
		KingdomAsset kingdom_asset = pData.kingdom_asset;
		pTooltip.name.text = kingdom_asset.id;
		DebugKingdomButton.getTooltipDescription(kingdom_asset, out var pDescription, out var pDescription2);
		pTooltip.setDescription(pDescription);
		if (!string.IsNullOrEmpty(pDescription2))
		{
			pTooltip.setBottomDescription(pDescription2);
		}
		pTooltip.tryShowBoolDebug("civ", kingdom_asset.civ);
		pTooltip.tryShowBoolDebug("nomads", kingdom_asset.nomads);
		pTooltip.tryShowBoolDebug("nature", kingdom_asset.nature);
		pTooltip.tryShowBoolDebug("mobs", kingdom_asset.mobs);
		pTooltip.tryShowBoolDebug("miniciv", kingdom_asset.group_miniciv);
		pTooltip.tryShowBoolDebug("neutral", kingdom_asset.neutral);
		pTooltip.tryShowBoolDebug("brain", kingdom_asset.brain);
		pTooltip.tryShowBoolDebug("always_attack_each_other", kingdom_asset.always_attack_each_other);
		pTooltip.tryShowBoolDebug("units_always_aggressive", kingdom_asset.units_always_looking_for_enemies);
	}

	private void showTip(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (LocalizedTextManager.stringExists(pData.tip_name))
		{
			pTooltip.name.text = LocalizedTextManager.getText(pData.tip_name);
		}
		else
		{
			pTooltip.name.text = pData.tip_name;
		}
		if (!string.IsNullOrEmpty(pData.tip_description))
		{
			string text = LocalizedTextManager.getText(pData.tip_description);
			if (text.Contains("$favorite_food$"))
			{
				string newValue = "??";
				if (SelectedUnit.unit.hasFavoriteFood())
				{
					newValue = LocalizedTextManager.getText(SelectedUnit.unit.data.favorite_food);
				}
				text = text.Replace("$favorite_food$", newValue);
				text += "\n";
				text += "\n";
				text = text + LocalizedTextManager.getText("food_consumed") + ": " + SelectedUnit.unit.data.food_consumed;
			}
			pTooltip.setDescription(text);
		}
		if ((Config.isComputer || Config.isEditor) && !string.IsNullOrEmpty(pData.tip_description_2))
		{
			string text2 = LocalizedTextManager.getText(pData.tip_description_2);
			text2 = AssetManager.hotkey_library.replaceSpecialTextKeys(text2);
			pTooltip.setBottomDescription(text2);
		}
	}

	private void showTipZoneMode(Tooltip pTooltip, string pType, TooltipData pData)
	{
		OptionAsset option_asset = AssetManager.meta_type_library.getFromPower(pData.tip_name).option_asset;
		string pMainText = pData.tip_name.Localize();
		string pSubText = "";
		if (option_asset.multi_toggle)
		{
			pSubText = option_asset.getOptionLocaleID();
		}
		pTooltip.setTitle(pMainText, pSubText);
		if (!string.IsNullOrEmpty(pData.tip_description))
		{
			string text = LocalizedTextManager.getText(pData.tip_description);
			string stateText = getStateText("borders_state_tip", Zones.isBordersEnabled());
			string stateText2 = getStateText("map_names_state_tip", Zones.showMapNames());
			text = text + "\n\n" + stateText + ", " + stateText2;
			pTooltip.setDescription(text);
		}
		if ((Config.isComputer || Config.isEditor) && !string.IsNullOrEmpty(pData.tip_description_2))
		{
			string text2 = LocalizedTextManager.getText(pData.tip_description_2);
			text2 = AssetManager.hotkey_library.replaceSpecialTextKeys(text2);
			pTooltip.setBottomDescription(text2);
		}
	}

	private string getStateText(string pLocale, bool pState)
	{
		string newValue = (pState ? "short_on" : "short_off").ColorHex(pState ? "#95DD5D" : "#FF8686", pLocalize: true);
		return LocalizedTextManager.getText(pLocale).Replace("$state$", newValue);
	}

	private void showCarryingResources(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Actor actor = pData.actor;
		pTooltip.name.text = pData.tip_name.Localize();
		foreach (KeyValuePair<string, ResourceContainer> resource in actor.inventory.getResources())
		{
			ResourceAsset asset = resource.Value.asset;
			int amount = resource.Value.amount;
			pTooltip.addLineIntText(asset.getLocaleID(), amount, "#43FF43");
		}
	}

	private void showPastNames(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		ListPool<NameEntry> past_names = pData.past_names;
		pTooltip.name.text = pData.tip_name.Localize();
		if (past_names == null || past_names.Count == 0)
		{
			return;
		}
		ColorLibrary colorLibrary = AssetManager.meta_customization_library.getAsset(pData.meta_type).color_library?.Invoke();
		foreach (ref NameEntry item in past_names)
		{
			NameEntry current = item;
			if (!string.IsNullOrEmpty(current.name))
			{
				string text = current.name;
				string text2 = Date.getYearDate(current.timestamp);
				string pColor = null;
				if (current.custom)
				{
					text2 = "* " + text2;
				}
				if (current.color_id > -1 && colorLibrary != null)
				{
					pColor = Toolbox.colorToHex(Color32.op_Implicit(colorLibrary.list[current.color_id].getColorText()));
					text = Toolbox.coloredText(text, pColor);
				}
				pTooltip.addLineText(text, text2, pColor, pPercent: false, pLocalize: false);
			}
		}
	}

	private void showPastRulers(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		ListPool<LeaderEntry> past_rulers = pData.past_rulers;
		pTooltip.name.text = pData.tip_name.Localize();
		if (past_rulers == null || past_rulers.Count == 0)
		{
			return;
		}
		ColorLibrary colorLibrary = AssetManager.meta_customization_library.getAsset(pData.meta_type).color_library?.Invoke();
		int num = Date.getCurrentYear();
		for (int num2 = past_rulers.Count - 1; num2 >= 0; num2--)
		{
			LeaderEntry leaderEntry = past_rulers[num2];
			string text = leaderEntry.name;
			string text2 = null;
			bool flag = false;
			Actor actor = World.world.units.get(leaderEntry.id);
			if (!actor.isRekt())
			{
				text = actor.name;
			}
			else
			{
				flag = true;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = LocalizedTextManager.getText("unknown");
			}
			if (flag)
			{
				text = "† " + text;
			}
			if (leaderEntry.color_id > -1 && colorLibrary != null)
			{
				text2 = Toolbox.colorToHex(Color32.op_Implicit(colorLibrary.list[leaderEntry.color_id].getColorText()), pAlpha: false);
				text = Toolbox.coloredText(text, text2);
			}
			int year = Date.getYear(leaderEntry.timestamp_ago);
			int num3 = Date.getYear(leaderEntry.timestamp_end);
			if (leaderEntry.timestamp_end < leaderEntry.timestamp_ago)
			{
				num3 = num;
			}
			num = year;
			int num4 = num3 - year;
			string pValue = string.Format("{0}–{1} ({2} {3})", year, num3, num4, "y");
			pTooltip.addLineText(text, pValue, null, pPercent: false, pLocalize: false);
		}
	}

	private void showMass(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Actor actor = pData.actor;
		pTooltip.name.text = pData.tip_name.Localize();
		foreach (ResourceContainer item in actor.getResourcesFromActor())
		{
			ResourceAsset resourceAsset = AssetManager.resources.get(item.id);
			pTooltip.addLineIntText(resourceAsset.getLocaleID(), item.amount, "#43FF43");
		}
	}

	private void showPassengers(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Actor actor = pData.actor;
		pTooltip.name.text = LocalizedTextManager.getText("passengers");
		TooltipIconsRow component = ((Component)((Component)pTooltip).transform.FindRecursive("Passengers")).GetComponent<TooltipIconsRow>();
		Boat simpleComponent = actor.getSimpleComponent<Boat>();
		showBoatPassengers(simpleComponent, component, pTooltip, pData);
	}

	private void showLoyalty(Tooltip pTooltip, string pType, TooltipData pData)
	{
		pTooltip.name.text = LocalizedTextManager.getText("loyalty");
		int loyalty = pData.city.getLoyalty(pForceRecalc: true);
		if (loyalty > 0)
		{
			pTooltip.addLineIntText("opinion_total", loyalty, "#43FF43");
		}
		else
		{
			pTooltip.addLineIntText("opinion_total", loyalty, "#FB2C21");
		}
		foreach (LoyaltyAsset key in LoyaltyCalculator.results.Keys)
		{
			int pValue = LoyaltyCalculator.results[key];
			string translationKey = key.getTranslationKey(pValue);
			pTooltip.addOpinion(new TooltipOpinionInfo(translationKey, pValue));
		}
		Text stats_description = pTooltip.stats_description;
		stats_description.text += "\n------------";
		Text stats_values = pTooltip.stats_values;
		stats_values.text += "\n------------";
		pTooltip.addLineBreak();
		pTooltip.addLineBreak();
	}

	private void showArmy(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Army army = pData.army;
		Kingdom kingdom = army.getKingdom();
		City city = army.getCity();
		pTooltip.setTitle(army.name, "army", army.getColor().color_text);
		pTooltip.setSpeciesIcon(army.getActorAsset().getSpriteIcon());
		setIconValue(pTooltip, "i_age", army.getAge());
		setIconValue(pTooltip, "i_population", army.countUnits());
		if (!kingdom.isRekt())
		{
			pTooltip.addLineText("kingdom", kingdom.name, kingdom.getColor().color_text);
		}
		if (!city.isRekt())
		{
			pTooltip.addLineText("villages", city.name, city.getColor().color_text);
		}
		if (army.hasCaptain())
		{
			pTooltip.addLineText("captain", army.getCaptain().getName(), army.getColor().color_text);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("males", army.countMales());
		pTooltip.addLineIntText("females", army.countFemales());
		pTooltip.addLineIntText("happy", army.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kills", army.getTotalKills());
		pTooltip.addLineIntText("deaths", army.getTotalDeaths());
		pTooltip.addLineIntText("renown", army.getRenown());
		KingdomBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<KingdomBanner>();
		foreach (KingdomBanner kingdomBanner in array)
		{
			if (((Component)kingdomBanner).gameObject.activeSelf)
			{
				kingdomBanner.load(army.getKingdom());
			}
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showSubspecies(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Subspecies subspecies = pData.subspecies;
		pTooltip.setTitle(subspecies.name, "subspecies_singular", subspecies.getColor().color_text);
		pTooltip.setSpeciesIcon(subspecies.getActorAsset().getSpriteIcon());
		setIconValue(pTooltip, "i_age", subspecies.getAge());
		setIconValue(pTooltip, "i_population", subspecies.countUnits());
		((Component)pTooltip).GetComponentInChildren<TooltipSubspeciesTraitsRow>(true).init(pTooltip, pData);
		pTooltip.addLineIntText("adults", subspecies.countAdults());
		pTooltip.addLineIntText("children", subspecies.countChildren());
		pTooltip.addLineIntText("happy", subspecies.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kings", subspecies.countKings());
		pTooltip.addLineIntText("leaders", subspecies.countLeaders());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("births", subspecies.getTotalBirths());
		pTooltip.addLineIntText("deaths", subspecies.getTotalDeaths());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("families", subspecies.countCurrentFamilies());
		SubspeciesBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<SubspeciesBanner>();
		foreach (SubspeciesBanner subspeciesBanner in array)
		{
			if (((Component)subspeciesBanner).gameObject.activeSelf)
			{
				subspeciesBanner.load(subspecies);
			}
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showFamily(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Family family = pData.family;
		ActorAsset actorAsset = family.getActorAsset();
		pTooltip.setSpeciesIcon(actorAsset.getSpriteIcon());
		pTooltip.setTitle(family.name, "family", family.getColor().color_text);
		int age = family.getAge();
		setIconValue(pTooltip, "i_age", age);
		setIconValue(pTooltip, "i_population", family.countUnits());
		pTooltip.addLineIntText("adults", family.countAdults());
		pTooltip.addLineIntText("children", family.countChildren());
		pTooltip.addLineIntText("happy", family.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("births", family.getTotalBirths());
		pTooltip.addLineIntText("deaths", family.getTotalDeaths());
		FamilyBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<FamilyBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(family);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showLanguage(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Language language = pData.language;
		pTooltip.setSpeciesIcon(language.getActorAsset().getSpriteIcon());
		pTooltip.setTitle(language.name, "language", language.getColor().color_text);
		setIconValue(pTooltip, "i_age", language.getAge());
		setIconValue(pTooltip, "i_population", language.countUnits());
		((Component)pTooltip).GetComponentInChildren<TooltipLanguageTraitsRow>(true).init(pTooltip, pData);
		if (!string.IsNullOrEmpty(language.data.creator_city_name))
		{
			pTooltip.addLineText("founded_in", language.data.creator_city_name);
			pTooltip.addLineBreak();
		}
		pTooltip.addLineIntText("kingdoms", language.countKingdoms());
		pTooltip.addLineIntText("villages", language.countCities());
		pTooltip.addLineIntText("books", language.books.count());
		pTooltip.addLineIntText("books_written", language.countWrittenBooks());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("adults", language.countAdults());
		pTooltip.addLineIntText("children", language.countChildren());
		pTooltip.addLineIntText("happy", language.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kings", language.countKings());
		pTooltip.addLineIntText("leaders", language.countLeaders());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", language.getTotalDeaths());
		LanguageBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<LanguageBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(language);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showReligion(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Religion religion = pData.religion;
		pTooltip.setSpeciesIcon(religion.getActorAsset().getSpriteIcon());
		pTooltip.setTitle(religion.name, "religion", religion.getColor().color_text);
		int age = religion.getAge();
		setIconValue(pTooltip, "i_age", age);
		setIconValue(pTooltip, "i_population", religion.countUnits());
		if (!string.IsNullOrEmpty(religion.data.creator_city_name))
		{
			pTooltip.addLineText("founded_in", religion.data.creator_city_name);
			pTooltip.addLineBreak();
		}
		pTooltip.addLineIntText("kingdoms", religion.countKingdoms());
		pTooltip.addLineIntText("villages", religion.countCities());
		pTooltip.addLineIntText("books", religion.books.count());
		pTooltip.addLineIntText("happy", religion.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("adults", religion.countAdults());
		pTooltip.addLineIntText("children", religion.countChildren());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kings", religion.countKings());
		pTooltip.addLineIntText("leaders", religion.countLeaders());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", religion.getTotalDeaths());
		ReligionBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<ReligionBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(religion);
		}
		((Component)pTooltip).GetComponentInChildren<TooltipReligionTraitsRow>(true).init(pTooltip, pData);
		showTabBannerTip(pTooltip, pData);
	}

	private void showCulture(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Culture culture = pData.culture;
		pTooltip.setSpeciesIcon(culture.getActorAsset().getSpriteIcon());
		pTooltip.setTitle(culture.name, "culture", culture.getColor().color_text);
		setIconValue(pTooltip, "i_age", culture.getAge());
		setIconValue(pTooltip, "i_population", culture.countUnits());
		((Component)pTooltip).GetComponentInChildren<TooltipCultureTraitsRow>(true).init(pTooltip, pData);
		if (!string.IsNullOrEmpty(culture.data.creator_city_name))
		{
			pTooltip.addLineText("founded_in", culture.data.creator_city_name);
			pTooltip.addLineBreak();
		}
		pTooltip.addLineIntText("kingdoms", culture.countKingdoms());
		pTooltip.addLineIntText("villages", culture.countCities());
		pTooltip.addLineIntText("books", culture.books.count());
		pTooltip.addLineIntText("happy", culture.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("adults", culture.countAdults());
		pTooltip.addLineIntText("children", culture.countChildren());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kings", culture.countKings());
		pTooltip.addLineIntText("leaders", culture.countLeaders());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", culture.getTotalDeaths());
		CultureBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<CultureBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(culture);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showPlot(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Plot plot = pData.plot;
		string text = plot.getColor()?.color_text;
		if (string.IsNullOrEmpty(text))
		{
			text = "#F3961F";
		}
		pTooltip.setTitle(plot.name, "plot", text);
		int progressPercentage = plot.getProgressPercentage();
		int age = plot.getAge();
		string pValue = progressPercentage.ToText() + "%";
		string pValue2 = plot.getSupporters().ToText() + "/" + plot.getMaxSupporters().ToText();
		pTooltip.addDescription(plot.getAsset().get_formatted_description(plot));
		pTooltip.addLineText("started_at", plot.getFoundedDate());
		string pColor = text;
		Actor author = plot.getAuthor();
		if (author != null)
		{
			pColor = author.kingdom.getColor().color_text;
		}
		pTooltip.addLineText("started_by", plot.data.founder_name, pColor);
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("tip_plot_age", age);
		pTooltip.addLineText("tip_plot_progress", pValue);
		pTooltip.addLineText("tip_plot_members", pValue2);
		PlotBanner[] componentsInChildren = ((Component)((Component)pTooltip).transform).GetComponentsInChildren<PlotBanner>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].load(plot);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showPlotInEditor(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		PlotAsset plot_asset = pData.plot_asset;
		string text = "";
		string text2 = "";
		if (plot_asset.isAvailable())
		{
			text2 = plot_asset.getDescriptionID2();
			text = plot_asset.getLocaleID();
		}
		else
		{
			text2 = "plot_locked_tooltip_text_exploration";
			text = "achievement_tip_hidden";
		}
		string text3 = LocalizedTextManager.getText(text);
		pTooltip.setTitle(text3);
		string text4 = LocalizedTextManager.getText(text2);
		pTooltip.addDescription(text4);
		Sprite sprite = plot_asset.getSprite();
		Image[] componentsInChildren = ((Component)((Component)pTooltip).transform.Find("Headline/icons")).GetComponentsInChildren<Image>(true);
		foreach (Image val in componentsInChildren)
		{
			val.sprite = sprite;
			if (plot_asset.isAvailable())
			{
				((Graphic)val).color = Toolbox.color_white;
			}
			else
			{
				((Graphic)val).color = Toolbox.color_black;
			}
		}
	}

	private void showCityHome(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showCity("creature_statistics_home_village", pTooltip, pData);
	}

	private void showCityCapital(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showCity("kingdom_statistics_capital", pTooltip, pData);
	}

	private void showCity(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showCity("village", pTooltip, pData);
	}

	private void showHappiness(Tooltip pTooltip, string pType, TooltipData pData)
	{
		pData.actor = SelectedUnit.unit;
		((Component)pTooltip.name).GetComponent<LocalizedText>().setKeyAndUpdate("happiness");
		if (!pData.actor.hasHappinessHistory())
		{
			return;
		}
		using ListPool<HappinessHistory> listPool = new ListPool<HappinessHistory>(pData.actor.happiness_change_history);
		listPool.Reverse();
		pTooltip.addLineText("happiness_current", pData.actor.getHappiness().ToText() + $" ({pData.actor.getHappinessPercent()}%)");
		pTooltip.addLineBreak();
		for (int i = 0; i < listPool.Count; i++)
		{
			int bonus = listPool[i].bonus;
			HappinessAsset asset = listPool[i].asset;
			int num = bonus + asset.value;
			string text = LocalizedTextManager.getText(asset.getLocaleID());
			text = Toolbox.coloredString(listPool[i].getAgoString(), ColorStyleLibrary.m.color_text_grey_dark) + ": " + text;
			if (num >= 0)
			{
				pTooltip.addLineText(text, num.ToString("+##,#;-##,#;0"), "#43FF43", pPercent: false, pLocalize: false);
			}
			else
			{
				pTooltip.addLineText(text, num.ToString("+##,#;-##,#;0"), "#FB2C21", pPercent: false, pLocalize: false);
			}
		}
	}

	private void showCity(string pTitleID, Tooltip pTooltip, TooltipData pData)
	{
		City city = pData.city;
		pTooltip.setSpeciesIcon(city.getCurrentSpeciesIcon());
		Kingdom kingdom = city.kingdom;
		string color_text = kingdom.getColor().color_text;
		int age = city.getAge();
		setIconValue(pTooltip, "i_age", age);
		setIconValue(pTooltip, "i_population", city.getPopulationPeople());
		setIconValue(pTooltip, "i_army", city.countWarriors());
		pTooltip.addLineText("books", city.countBooks() + "/" + city.countBookSlots());
		pTooltip.setTitle(city.name, pTitleID, color_text);
		string pValue = "-";
		if (kingdom.hasKing())
		{
			pValue = kingdom.king.getName();
		}
		string pValue2 = "-";
		if (city.hasLeader())
		{
			pValue2 = city.leader.getName();
		}
		pTooltip.addLineText("village_statistics_leader", pValue2, color_text);
		if (city.hasLeader())
		{
			pTooltip.addLineIntText("ruler_money", city.leader.money);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("adults", city.countAdults());
		pTooltip.addLineIntText("children", city.countChildren());
		pTooltip.addLineIntText("families", city.countFamilies());
		pTooltip.addLineIntText("happy", city.countHappyUnits());
		pTooltip.addLineBreak();
		if (!city.kingdom.isNeutral())
		{
			pTooltip.addLineText("kingdom", city.kingdom.name, color_text);
		}
		pTooltip.addLineText("village_statistics_king", pValue, color_text);
		if (city.hasLeader() && city.leader.hasClan())
		{
			pTooltip.addLineText("clan", city.leader.clan.name, city.leader.clan.getColor().color_text);
		}
		if (city.hasCulture())
		{
			pTooltip.addLineText("culture", city.culture.name, city.culture.getColor().color_text);
		}
		if (city.hasReligion())
		{
			pTooltip.addLineText("religion", city.religion.name, city.religion.getColor().color_text);
		}
		if (city.hasLanguage())
		{
			pTooltip.addLineText("language", city.language.name, city.language.getColor().color_text);
		}
		Alliance alliance = kingdom.getAlliance();
		if (alliance != null)
		{
			int yearsSince = Date.getYearsSince(kingdom.data.timestamp_alliance);
			pTooltip.addLineText("alliance", alliance.data.name, alliance.getColor().color_text);
			pTooltip.addLineIntText("kingdom_time_in_alliance", yearsSince, alliance.getColor().color_text);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("houses", city.getHouseCurrent());
		pTooltip.addLineIntText("area", city.zones.Count);
		pTooltip.addLineIntText("loyalty", city.getCachedLoyalty());
		pTooltip.addLineIntText("books", city.countBooks());
		((Component)pTooltip).GetComponentInChildren<CityBanner>().load(city);
		if (DebugConfig.isOn(DebugOption.DebugCityReproduction))
		{
			pTooltip.addLineBreak();
			pTooltip.addLineIntText("males_single", city.countSingleMales(), "#4CCFFF");
			pTooltip.addLineIntText("females_single", city.countSingleFemales(), "#FF637D");
			pTooltip.addLineIntText("couples", city.countCouples());
			pTooltip.addLineText("male/female", city.countMales().ToText() + "/" + city.countFemales().ToText(), "#FF637D", pPercent: false, pLocalize: false);
			pTooltip.addLineText("adults/kids", city.countAdults().ToText() + " | " + city.countChildren().ToText(), null, pPercent: false, pLocalize: false);
			pTooltip.addLineIntText("pot_par_males", city.countPotentialParents(ActorSex.Male), null, pLocalize: false);
			pTooltip.addLineIntText("pot_par_females", city.countPotentialParents(ActorSex.Female), null, pLocalize: false);
			pTooltip.addLineBreak();
			pTooltip.addLineIntText("hungry", city.countHungry(), "#FF637D", pLocalize: false);
			pTooltip.addLineIntText("starving", city.countStarving(), "#FF637D", pLocalize: false);
			pTooltip.addLineIntText("food", city.countFoodTotal());
			pTooltip.addLineIntText("afteglows", city.countUnitsWithStatus("afterglow"), null, pLocalize: false);
			pTooltip.addLineIntText("pregnant", city.countUnitsWithStatus("pregnant"), null, pLocalize: false);
			pTooltip.addLineIntText("births", city.getTotalBirths(), null, pLocalize: false);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showActorNormal(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showActor("", pTooltip, pData);
	}

	private void showLeader(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showActor("village_statistics_leader", pTooltip, pData);
	}

	private void showKing(Tooltip pTooltip, string pType, TooltipData pData)
	{
		showActor("village_statistics_king", pTooltip, pData);
	}

	private void showActor(string pSubTitle, Tooltip pTooltip, TooltipData pData)
	{
		Actor actor = pData.actor;
		Image component = ((Component)((Component)pTooltip).transform.FindRecursive("IconSpecial")).GetComponent<Image>();
		if (actor.asset.is_boat)
		{
			component.sprite = actor.asset.getSpriteIcon();
		}
		else if (actor.isSexMale())
		{
			component.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
		}
		else
		{
			component.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
		}
		setIconValue(pTooltip, "i_age", actor.getAge());
		setIconValue(pTooltip, "i_level", actor.data.level);
		setIconValue(pTooltip, "i_kills", actor.data.kills);
		((Component)pTooltip).GetComponentInChildren<TooltipActorTraitsRow>(true).init(pTooltip, pData);
		((Component)pTooltip).GetComponentInChildren<TooltipActorEquipmentsRow>(true).init(pTooltip, pData);
		StatBar component2 = ((Component)((Component)pTooltip).transform.FindRecursive("HealthBar")).GetComponent<StatBar>();
		float pVal = actor.getHealth();
		float num = actor.getMaxHealth();
		component2.setBar(pVal, num, "/" + ((int)num).ToText(4), pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		showActorBars(pTooltip, actor);
		string color_text = actor.kingdom.getColor().color_text;
		pTooltip.setTitle(actor.name, pSubTitle, color_text);
		if (DebugConfig.isOn(DebugOption.DebugTooltipActorAI))
		{
			pTooltip.addLineText("wait_timer", actor.hasTask() ? actor.timer_action.ToText() : "-", "#43FF43", pPercent: false, pLocalize: false);
			pTooltip.addLineText("task", actor.hasTask() ? actor.ai.task.id : "-", "#43FF43", pPercent: false, pLocalize: false);
			pTooltip.addLineText("action", (actor.ai.action != null) ? actor.ai.action.id : "-", "#23F3FF", pPercent: false, pLocalize: false);
			pTooltip.addLineText("job", (actor.ai.job != null) ? actor.ai.job.id : "-", "#FB2C21", pPercent: false, pLocalize: false);
			pTooltip.addLineText("citizen_job", (actor.citizen_job != null) ? actor.citizen_job.id : "-", "#8CFF99", pPercent: false, pLocalize: false);
			pTooltip.addLineIntText("id", actor.data.id);
			pTooltip.addLineIntText("hashset", actor.GetHashCode(), null, pLocalize: false);
			pTooltip.addLineIntText("kingdom_hash", actor.kingdom.GetHashCode(), null, pLocalize: false);
			pTooltip.addLineIntText("kingdom_id", actor.kingdom.data.id, null, pLocalize: false);
			pTooltip.addLineText("profession", actor.profession_asset.id, null, pPercent: false, pLocalize: false);
		}
		if (actor.isSapient() && actor.isKingdomCiv())
		{
			pTooltip.addLineText("kingdom", actor.kingdom.name, color_text);
		}
		if (actor.hasLover())
		{
			pTooltip.addLineText("lover", actor.lover.name, actor.lover.kingdom.getColor().color_text);
		}
		if (actor.asset.inspect_home)
		{
			string pValue = "??";
			if (actor.city != null)
			{
				pValue = actor.city.name;
			}
			pTooltip.addLineText("creature_statistics_home_village", pValue, color_text);
			if (actor.hasClan())
			{
				string color_text2 = actor.clan.getColor().color_text;
				pTooltip.addLineText("clan", actor.clan.data.name, color_text2);
			}
		}
		if (actor.hasFamily())
		{
			pTooltip.addLineText("family", actor.family.name, actor.family.getColor().color_text);
		}
		if (actor.hasCulture())
		{
			pTooltip.addLineText("culture", actor.culture.name, actor.culture.getColor().color_text);
		}
		if (actor.hasLanguage())
		{
			pTooltip.addLineText("language", actor.language.name, actor.language.getColor().color_text);
		}
		if (actor.hasArmy())
		{
			pTooltip.addLineText("army", actor.army.name, actor.army.getColor().color_text);
		}
		pTooltip.addLineBreak();
		if (actor.money > 0)
		{
			pTooltip.addLineIntText("money", actor.money);
		}
		if (actor.loot > 0)
		{
			pTooltip.addLineIntText("loot", actor.loot);
		}
		if (actor.asset.inspect_kills)
		{
			pTooltip.addLineIntText("creature_statistics_kills", actor.data.kills);
		}
		if (actor.asset.inspect_children)
		{
			pTooltip.addLineIntText("creature_statistics_children", actor.current_children_count);
		}
		if (actor.isSapient() && actor.s_personality != null)
		{
			pTooltip.addLineText("creature_statistics_personality", LocalizedTextManager.getText("personality_" + actor.s_personality.id));
		}
		pTooltip.addLineText("task", actor.hasTask() ? actor.ai.task.getLocalizedText() : "-", "#43FF43");
		if (actor.hasSubspecies())
		{
			pTooltip.addLineBreak();
			pTooltip.addLineText("subspecies", actor.subspecies.name, actor.subspecies.getColor().color_text, pPercent: false, pLocalize: true, 15);
		}
		TooltipIconsRow component3 = ((Component)((Component)pTooltip).transform.FindRecursive("Resources")).GetComponent<TooltipIconsRow>();
		bool flag = actor.isCarryingResources();
		((Component)component3).gameObject.SetActive(flag);
		if (flag)
		{
			foreach (ResourceContainer value in actor.inventory.getResources().Values)
			{
				Sprite spriteIcon = AssetManager.resources.get(value.id).getSpriteIcon();
				int amount = value.amount;
				int num2 = 5;
				for (int i = 0; i < amount; i++)
				{
					component3.addIcon(spriteIcon);
					num2--;
					if (num2 <= 0)
					{
						break;
					}
				}
			}
			component3.init(pTooltip, pData);
		}
		TooltipIconsRow component4 = ((Component)((Component)pTooltip).transform.FindRecursive("Passengers")).GetComponent<TooltipIconsRow>();
		if (actor.asset.is_boat)
		{
			Boat simpleComponent = actor.getSimpleComponent<Boat>();
			pTooltip.addLineBreak();
			pTooltip.addLineIntText("passengers", simpleComponent.countPassengers());
			showBoatPassengers(simpleComponent, component4, pTooltip, pData);
		}
		else
		{
			((Component)component4).gameObject.SetActive(false);
		}
		Sprite val = ((!actor.asset.is_boat || !actor.hasCity()) ? actor.asset.getSpriteIcon() : actor.city.getSpriteIcon());
		Image speciesIcon = pTooltip.getSpeciesIcon();
		if ((Object)(object)val != (Object)null)
		{
			speciesIcon.sprite = val;
			((Component)speciesIcon).gameObject.SetActive(true);
		}
		else
		{
			((Component)speciesIcon).gameObject.SetActive(false);
		}
	}

	private void showBoatPassengers(Boat pBoat, TooltipIconsRow pPassengersIcons, Tooltip pTooltip, TooltipData pData)
	{
		if (!pBoat.hasPassengers())
		{
			((Component)pPassengersIcons).gameObject.SetActive(false);
			return;
		}
		((Component)pPassengersIcons).gameObject.SetActive(true);
		int num = 60;
		foreach (Actor passenger in pBoat.getPassengers())
		{
			Sprite spriteIcon = passenger.asset.getSpriteIcon();
			pPassengersIcons.addIcon(spriteIcon);
			num--;
			if (num <= 0)
			{
				break;
			}
		}
		pPassengersIcons.init(pTooltip, pData);
	}

	private void showActorBars(Tooltip pTooltip, Actor pActor)
	{
		bool flag = pActor.hasEmotions();
		if (flag)
		{
			((Component)pTooltip).GetComponentInChildren<HappinessBarIcon>(true).load(pActor);
		}
		checkShowProgressBar(pTooltip, "HappinessBarFitter", "%", pActor.getHappinessPercent(), 100f, flag);
		bool flag2 = pActor.needsFood();
		float pCurrentValue = (float)pActor.getNutrition() / (float)pActor.getMaxNutrition() * 100f;
		checkShowProgressBar(pTooltip, "HungerBarFitter", "%", pCurrentValue, 100f, flag2);
		bool flag3 = !pActor.asset.force_hide_stamina;
		int maxStamina = pActor.getMaxStamina();
		float pCurrentValue2 = Mathf.Clamp(pActor.getStamina(), 0, maxStamina);
		checkShowProgressBar(pTooltip, "StaminaBarFitter", $"/{maxStamina}", pCurrentValue2, maxStamina, flag3);
		bool flag4 = !pActor.asset.force_hide_mana;
		int maxMana = pActor.getMaxMana();
		float pCurrentValue3 = Mathf.Clamp(pActor.getMana(), 0, maxMana);
		checkShowProgressBar(pTooltip, "ManaBarFitter", $"/{maxMana}", pCurrentValue3, maxMana, flag4);
		Transform val = ((Component)pTooltip).transform.FindRecursive("Bars");
		if (!flag && !flag2 && !flag3 && !flag4)
		{
			((Component)val).gameObject.SetActive(false);
		}
		else
		{
			((Component)val).gameObject.SetActive(true);
		}
	}

	private void checkShowProgressBar(Tooltip pTooltip, string pBarName, string pEnding, float pCurrentValue, float pMax, bool pShow)
	{
		Transform val = ((Component)pTooltip).transform.FindRecursive(pBarName);
		((Component)val).gameObject.SetActive(pShow);
		if (pShow)
		{
			((Component)val).GetComponentInChildren<StatBar>(true).setBar(pCurrentValue, pMax, pEnding, pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		}
	}

	private void showClan(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Clan clan = pData.clan;
		pTooltip.setSpeciesIcon(clan.getActorAsset().getSpriteIcon());
		pTooltip.setDescription(clan.getMotto());
		string color_text = clan.getColor().color_text;
		pTooltip.setTitle(clan.name, "clan", color_text);
		setIconValue(pTooltip, "i_age", clan.getAge());
		setIconValue(pTooltip, "i_population", clan.countUnits());
		((Component)pTooltip).GetComponentInChildren<TooltipClanTraitsRow>(true).init(pTooltip, pData);
		pTooltip.addLineText("clan_members_title", clan.getTextMaxMembers());
		if (clan.getChief() != null)
		{
			if (clan.getChief().hasKingdom())
			{
				color_text = clan.getChief().kingdom.getColor().color_text;
			}
			pTooltip.addLineText("clan_chief_title", clan.getChief().getName(), color_text);
			pTooltip.addLineText("species", clan.getChief().asset.getTranslatedName(), color_text);
			pTooltip.addLineBreak();
		}
		pTooltip.addLineIntText("adults", clan.countAdults());
		pTooltip.addLineIntText("children", clan.countChildren());
		pTooltip.addLineIntText("happy", clan.countHappyUnits());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kings", clan.countKings());
		pTooltip.addLineIntText("leaders", clan.countLeaders());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", clan.getTotalDeaths());
		ClanBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<ClanBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(clan);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private void showBook(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Book book = pData.book;
		BookTypeAsset asset = book.getAsset();
		int age = book.getAge();
		string text = LocalizedTextManager.getText("book_author_description");
		text = text.Replace("$author_name$", book.data.author_name);
		text = text.Replace("$author_kingdom$", book.data.author_kingdom_name);
		text = text.Replace("$author_city$", book.data.author_city_name);
		string text2 = asset.getDescriptionTranslated();
		pTooltip.setTitle(book.name, asset.getLocaleID(), asset.color_text);
		pTooltip.addLineIntText("age", age);
		pTooltip.addLineText("book_written_in", book.getBirthday());
		pTooltip.addLineIntText("book_times_read", book.data.times_read);
		pTooltip.addLineBreak();
		showMetaLineActor(pTooltip, "book_author", book.data.author_id, book.data.author_name);
		showMetaLineClan(pTooltip, "clan", book.data.author_clan_id, book.data.author_clan_name);
		showMetaLineCulture(pTooltip, "culture", book.data.culture_id, book.data.culture_name);
		showMetaLineLanguage(pTooltip, "language", book.data.language_id, book.data.language_name);
		showMetaLineVillage(pTooltip, "village", book.data.author_city_id, book.data.author_city_name);
		showMetaLineVillage(pTooltip, "religion", book.data.religion_id, book.data.religion_name);
		pTooltip.addLineBreak();
		string pID = Toolbox.coloredText(LocalizedTextManager.getText("book_action_on_read"), "#FFFFFF");
		pTooltip.addLineText(pID, "", null, pPercent: false, pLocalize: false);
		TooltipIconsRow componentInChildren = ((Component)pTooltip).GetComponentInChildren<TooltipIconsRow>(true);
		BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, book.getBaseStats());
		if (book.getBookTraitActor() != null)
		{
			componentInChildren.addIcon(book.getBookTraitActor().getSprite());
		}
		if (book.getBookTraitCulture() != null)
		{
			componentInChildren.addIcon(book.getBookTraitCulture().getSprite());
		}
		if (book.getBookTraitLanguage() != null)
		{
			componentInChildren.addIcon(book.getBookTraitLanguage().getSprite());
		}
		if (book.getBookTraitReligion() != null)
		{
			componentInChildren.addIcon(book.getBookTraitReligion().getSprite());
		}
		componentInChildren.init(pTooltip, pData);
		if (Config.editor_maxim)
		{
			text2 += "\n\n";
			text2 += StoryLibrary.getTestText(book.getLanguage());
		}
		pTooltip.setDescription(text2);
		pTooltip.setBottomDescription(text);
	}

	private void showMetaLineActor(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		Actor actor = (pID.hasValue() ? World.world.units.get(pID) : null);
		string pValue = "† " + pDefaultName;
		string pColor = null;
		if (!actor.isRekt())
		{
			pColor = actor.kingdom?.getColor().color_text;
			pValue = actor.name;
		}
		pTooltip.addLineText(pTitle, pValue, pColor);
	}

	private void showMetaLineClan(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			Clan clan = World.world.clans.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!clan.isRekt())
			{
				pColor = clan.getColor().color_text;
				pValue = clan.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showMetaLineCulture(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			Culture culture = World.world.cultures.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!culture.isRekt())
			{
				pColor = culture.getColor().color_text;
				pValue = culture.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showMetaLineLanguage(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			Language language = World.world.languages.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!language.isRekt())
			{
				pColor = language.getColor().color_text;
				pValue = language.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showMetaLineVillage(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			City city = World.world.cities.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!city.isRekt())
			{
				pColor = city.getColor().color_text;
				pValue = city.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showMetaLineSubspecies(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			Subspecies subspecies = World.world.subspecies.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!subspecies.isRekt())
			{
				pColor = subspecies.getColor().color_text;
				pValue = subspecies.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showMetaLineReligion(Tooltip pTooltip, string pTitle, long pID, string pDefaultName)
	{
		if (pID.hasValue())
		{
			Religion religion = World.world.religions.get(pID);
			string pValue = "† " + pDefaultName;
			string pColor = null;
			if (!religion.isRekt())
			{
				pColor = religion.getColor().color_text;
				pValue = religion.name;
			}
			pTooltip.addLineText(pTitle, pValue, pColor);
		}
	}

	private void showWar(Tooltip pTooltip, string pType, TooltipData pData)
	{
		War war = pData.war;
		((Component)pTooltip).GetComponentInChildren<WarTooltipBannersContainer>().load(war);
		pTooltip.setTitle(war.name, war.getAsset().localized_war_name, war.getAttackersColorTextString());
		pTooltip.addLineIntText("started_at", war.getYearStarted());
		if (war.hasEnded())
		{
			pTooltip.addLineIntText("war_ended_at", war.getYearEnded());
		}
		pTooltip.addLineIntText("war_duration", war.getDuration());
		string pValue = war.data.winner.getLocaleID().Localize();
		switch (war.data.winner)
		{
		case WarWinner.Attackers:
			pTooltip.addLineText("war_winner", pValue, war.getAttackersColorTextString());
			break;
		case WarWinner.Defenders:
			pTooltip.addLineText("war_winner", pValue, war.getDefendersColorTextString());
			break;
		case WarWinner.Peace:
			pTooltip.addLineText("war_outcome", pValue);
			break;
		case WarWinner.Merged:
			pTooltip.addLineText("war_outcome", pValue);
			break;
		}
		pTooltip.addLineBreak();
		Actor actor = World.world.units.get(war.data.started_by_actor_id);
		string pValue2 = ((actor != null) ? actor.getName() : war.data.started_by_actor_name);
		pTooltip.addLineText("instigator", pValue2);
		long started_by_kingdom_id = war.data.started_by_kingdom_id;
		Kingdom kingdom = World.world.kingdoms.get(started_by_kingdom_id) ?? World.world.kingdoms.db_get(started_by_kingdom_id);
		if (kingdom != null)
		{
			pTooltip.addLineText("instigator_from", kingdom.name, kingdom.getColor().color_text);
		}
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("kingdoms", war.countKingdoms());
		pTooltip.addLineIntText("villages", war.countCities());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", war.getTotalDeaths());
		setIconValue(pTooltip, "a_army", war.countAttackersWarriors());
		setIconValue(pTooltip, "a_population", war.countAttackersPopulation());
		setIconValue(pTooltip, "a_deaths", war.getDeadAttackers());
		setIconValue(pTooltip, "a_cities", war.countAttackersCities());
		setIconValue(pTooltip, "d_army", war.countDefendersWarriors());
		setIconValue(pTooltip, "d_population", war.countDefendersPopulation());
		setIconValue(pTooltip, "d_deaths", war.getDeadDefenders());
		setIconValue(pTooltip, "d_cities", war.countDefendersCities());
		showTabBannerTip(pTooltip, pData);
	}

	private void showWarSides(Tooltip pTooltip, string pType, TooltipData pData)
	{
		War war = pData.war;
		Text component = ((Component)((Component)pTooltip).transform.Find("Sides/Attackers/List")).GetComponent<Text>();
		Text component2 = ((Component)((Component)pTooltip).transform.Find("Sides/Defenders/List")).GetComponent<Text>();
		component.text = "";
		component2.text = "";
		switch (war.data.winner)
		{
		case WarWinner.Attackers:
			component.text = component.text + Toolbox.coloredText("war_winner_won", war.getAttackersColorTextString(), pLocalize: true) + "\n\n";
			component2.text = component2.text + Toolbox.coloredText("war_winner_lost", war.getDefendersColorTextString(), pLocalize: true) + "\n\n";
			break;
		case WarWinner.Defenders:
			component2.text = component2.text + Toolbox.coloredText("war_winner_won", war.getDefendersColorTextString(), pLocalize: true) + "\n\n";
			component.text = component.text + Toolbox.coloredText("war_winner_lost", war.getAttackersColorTextString(), pLocalize: true) + "\n\n";
			break;
		}
		using ListPool<string> listPool = new ListPool<string>();
		using ListPool<string> listPool2 = new ListPool<string>();
		foreach (Kingdom attacker in war.getAttackers())
		{
			addParty(attacker, listPool);
		}
		foreach (Kingdom diedAttacker in war.getDiedAttackers())
		{
			addParty(diedAttacker, listPool, pLeft: false, pDied: true);
		}
		foreach (Kingdom pastAttacker in war.getPastAttackers())
		{
			addParty(pastAttacker, listPool, pLeft: true);
		}
		foreach (Kingdom defender in war.getDefenders())
		{
			addParty(defender, listPool2);
		}
		foreach (Kingdom diedDefender in war.getDiedDefenders())
		{
			addParty(diedDefender, listPool2, pLeft: false, pDied: true);
		}
		foreach (Kingdom pastDefender in war.getPastDefenders())
		{
			addParty(pastDefender, listPool2, pLeft: true);
		}
		if (listPool.Count > 13)
		{
			int num = listPool.Count - 12;
			while (listPool.Count > 12)
			{
				listPool.Pop();
			}
			listPool.Add("... and " + num + " more");
		}
		if (listPool2.Count > 13)
		{
			int num2 = listPool2.Count - 12;
			while (listPool2.Count > 12)
			{
				listPool2.Pop();
			}
			listPool2.Add("... and " + num2 + " more");
		}
		component.text += string.Join("\n", listPool);
		component2.text += string.Join("\n", listPool2);
		showTabBannerTip(pTooltip, pData);
	}

	private static void addParty(Kingdom pKingdom, ListPool<string> pPartyList, bool pLeft = false, bool pDied = false)
	{
		string name = pKingdom.name;
		string color_text = pKingdom.getColor().color_text;
		string text = "";
		if (pLeft)
		{
			text = Toolbox.coloredText(" (left)", ColorStyleLibrary.m.color_text_grey_dark);
		}
		else if (pDied)
		{
			text = Toolbox.coloredText(" (died)", ColorStyleLibrary.m.color_text_grey);
		}
		else
		{
			pKingdom.hasDied();
		}
		pPartyList.Add(Toolbox.coloredText(name, color_text) + text);
	}

	private void showAlliance(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Alliance alliance = pData.alliance;
		pTooltip.setDescription(alliance.getMotto());
		string color_text = alliance.getColor().color_text;
		pTooltip.setTitle(alliance.name, "alliance", color_text);
		int age = alliance.getAge();
		setIconValue(pTooltip, "i_age", age);
		setIconValue(pTooltip, "i_population", alliance.countPopulation());
		setIconValue(pTooltip, "i_army", alliance.countWarriors());
		pTooltip.addLineIntText("adults", alliance.countAdults());
		pTooltip.addLineIntText("children", alliance.countChildren());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("tip_alliance_kingdoms", alliance.countKingdoms());
		pTooltip.addLineIntText("tip_alliance_buildings", alliance.countBuildings());
		pTooltip.addLineIntText("territory", alliance.countZones());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("housed", alliance.countHoused());
		pTooltip.addLineIntText("homeless", alliance.countHomeless());
		pTooltip.addLineBreak();
		pTooltip.addLineIntText("deaths", alliance.getTotalDeaths());
		AllianceBanner[] array = ((Component)pTooltip).transform.FindAllRecursive<AllianceBanner>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].load(alliance);
		}
		showTabBannerTip(pTooltip, pData);
	}

	private KingdomOpinion showKingdomOpinion(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Kingdom kingdom = pData.kingdom;
		pTooltip.name.text = Toolbox.coloredText(kingdom.name, kingdom.getColor().color_text);
		KingdomOpinion opinion = World.world.diplomacy.getRelation(kingdom, SelectedMetas.selected_kingdom).getOpinion(SelectedMetas.selected_kingdom, kingdom);
		foreach (OpinionAsset key in opinion.results.Keys)
		{
			int pValue = opinion.results[key];
			string translationKey = key.getTranslationKey(pValue);
			pTooltip.addOpinion(new TooltipOpinionInfo(translationKey, pValue));
		}
		return opinion;
	}

	private string getArrowUp(long pValue)
	{
		if (pValue < 10)
		{
			return " <size=4>↗</size>";
		}
		if (pValue < 100)
		{
			return " <size=4>↗↗</size>";
		}
		return " <size=4>↗↗↗</size>";
	}

	private string getArrowDown(long pValue)
	{
		pValue = (long)Mathf.Abs((float)pValue);
		if (pValue < 10)
		{
			return " <size=4>↘</size>";
		}
		if (pValue < 100)
		{
			return " <size=4>↘↘</size>";
		}
		return " <size=4>↘↘↘</size>";
	}

	private void showGraphResource(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (pData.nano_object != null)
		{
			NanoObject nano_object = pData.nano_object;
			string pColor = Toolbox.colorToHex(Color32.op_Implicit(nano_object.getColor().getColorText()));
			string text = "";
			MetaCustomizationAsset asset = AssetManager.meta_customization_library.getAsset(nano_object.getMetaType());
			text += LocalizedTextManager.getText(asset.localization_title);
			text += "\n";
			text += nano_object.name;
			pTooltip.name.text = Toolbox.coloredText(text, pColor);
		}
		long num = pData.custom_data_long["year"];
		pTooltip.addLineLongText("year", num);
		long pValue = (num - Date.getCurrentYear()) * -1;
		pTooltip.addLineLongText("years_ago", pValue);
		pTooltip.addLineBreak();
		using ListPool<string> listPool = new ListPool<string>(pData.custom_data_long.Keys);
		listPool.Sort((string pA, string pB) => pData.custom_data_long[pB].CompareTo(pData.custom_data_long[pA]));
		foreach (ref string item in listPool)
		{
			string current = item;
			if (current == "year" || current.Contains("_previous"))
			{
				continue;
			}
			HistoryDataAsset historyDataAsset = AssetManager.history_data_library.get(current);
			long num2 = pData.custom_data_long[current];
			long num3 = pData.custom_data_long[current + "_previous"];
			long num4 = num2 - num3;
			if (num4 != 0L)
			{
				string text2 = Toolbox.formatNumber(num4);
				if (num4 > 0)
				{
					text2 = "+" + text2;
				}
				string pColor2 = ((num4 > 0) ? "#43FF43" : "#FB2C21");
				string text3 = Toolbox.coloredText(text2, pColor2);
				string text4 = historyDataAsset.getLocaleID().Localize();
				text4 = ((num4 <= 0) ? (text4 + getArrowDown(num4).ColorHex("#FB2C21")) : (text4 + getArrowUp(num4).ColorHex("#43FF43")));
				pTooltip.addLineText(text4, "<size=4>" + text3 + "</size> " + num2.ToText(), historyDataAsset.tooltip_color_hex, pPercent: false, pLocalize: false, 500);
			}
			else
			{
				pTooltip.addLineLongText(historyDataAsset.getLocaleID(), num2, historyDataAsset.tooltip_color_hex);
			}
		}
	}

	private void showGraphMultiResource(Tooltip pTooltip, string pType, TooltipData pData)
	{
		string tip_name = pData.tip_name;
		HistoryDataAsset historyDataAsset = AssetManager.history_data_library.get(tip_name);
		pTooltip.name.text = Toolbox.coloredText(historyDataAsset.getLocaleID(), historyDataAsset.tooltip_color_hex, pLocalize: true);
		long num = pData.custom_data_long["year"];
		pTooltip.addLineIntText("year", num);
		long pValue = (num - Date.getCurrentYear()) * -1;
		pTooltip.addLineIntText("years_ago", pValue);
		pTooltip.addLineBreak();
		using ListPool<string> listPool = new ListPool<string>(pData.custom_data_long.Keys);
		listPool.Sort((string pA, string pB) => pData.custom_data_long[pB].CompareTo(pData.custom_data_long[pA]));
		foreach (ref string item in listPool)
		{
			string current = item;
			if (current == "year" || current.Contains("_previous"))
			{
				continue;
			}
			long num2 = pData.custom_data_long[current];
			long num3 = pData.custom_data_long[current + "_previous"];
			long num4 = num2 - num3;
			string pColor = pData.custom_data_string[current];
			if (num4 != 0L)
			{
				string text = Toolbox.formatNumber(num4);
				if (num4 > 0)
				{
					text = "+" + text;
				}
				string pColor2 = ((num4 > 0) ? "#43FF43" : "#FB2C21");
				string text2 = Toolbox.coloredText(text, pColor2);
				pTooltip.addLineText(current, "<size=4>" + text2 + "</size> " + num2.ToText(), pColor, pPercent: false, pLocalize: false, 500);
			}
			else
			{
				pTooltip.addLineLongText(current, num2, pColor, pLocalize: false, 500);
			}
		}
	}

	private void showGenderData(Tooltip pTooltip, string pType, TooltipData pData)
	{
		string text = pData.custom_data_string["age_range"];
		pTooltip.name.text = text;
		HistoryDataAsset historyDataAsset = AssetManager.history_data_library.get("males");
		HistoryDataAsset historyDataAsset2 = AssetManager.history_data_library.get("females");
		pTooltip.addLineText("age_range", text);
		pTooltip.addLineBreak();
		pTooltip.addLineIntText(historyDataAsset.getLocaleID(), pData.custom_data_int["males"], historyDataAsset.tooltip_color_hex);
		pTooltip.addLineIntText(historyDataAsset2.getLocaleID(), pData.custom_data_int["females"], historyDataAsset2.tooltip_color_hex);
	}

	private void showCityResource(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (!SelectedMetas.selected_city.isRekt())
		{
			ResourceAsset resource = pData.resource;
			pTooltip.name.text = resource.getTranslatedName();
			pTooltip.clearTextRows();
			pTooltip.addLineIntText("amount", SelectedMetas.selected_city.getResourcesAmount(resource.id));
		}
	}

	private void showCityResourceFood(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (!SelectedMetas.selected_city.isRekt())
		{
			ResourceAsset resource = pData.resource;
			pTooltip.name.text = resource.getTranslatedName();
			pTooltip.clearTextRows();
			pTooltip.addLineIntText("amount", SelectedMetas.selected_city.getResourcesAmount(resource.id));
			pTooltip.addLineBreak();
			if (resource.restore_health != 0f)
			{
				pTooltip.addLineText("health", resource.restore_health.ToText());
			}
			if (resource.restore_mana != 0)
			{
				pTooltip.addLineIntText("mana", resource.restore_mana);
			}
			if (resource.restore_stamina != 0)
			{
				pTooltip.addLineIntText("stamina", resource.restore_stamina);
			}
			pTooltip.addLineBreak();
			if (resource.restore_nutrition != 0)
			{
				pTooltip.addLineIntText("nutrition", resource.restore_nutrition);
			}
			if (resource.restore_happiness != 0)
			{
				pTooltip.addLineIntText("happiness", resource.restore_happiness);
			}
		}
	}

	private void showMapMeta(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		MapMetaData map_meta = pData.map_meta;
		string text = null;
		if (map_meta.saveVersion > Config.WORLD_SAVE_VERSION)
		{
			pTooltip.setBottomDescription(LocalizedTextManager.getText("future_save_version"), "#FB2C21");
			text = "#FB2C21";
		}
		pTooltip.name.text = map_meta.mapStats.name;
		((Graphic)pTooltip.name).color = map_meta.mapStats.getArchitectMood().getColorText();
		if (map_meta.modded)
		{
			if (text != null)
			{
				pTooltip.addBottomDescription("\n\n");
			}
			if (!Config.MODDED)
			{
				pTooltip.addBottomDescription(LocalizedTextManager.getText("modded_world_no_mod_active"), "#FB2C21");
				text = "#FB2C21";
			}
			else
			{
				pTooltip.addBottomDescription(LocalizedTextManager.getText("modded_world"), "#45FFFE");
				if (text == null)
				{
					text = "#45FFFE";
				}
			}
		}
		if (text != null)
		{
			pTooltip.name.text = Toolbox.coloredText(map_meta.mapStats.name, text);
		}
		if (map_meta.mapStats.description != "")
		{
			pTooltip.addDescription(map_meta.mapStats.description);
		}
		else
		{
			pTooltip.addDescription("WORLDBOX, HO!");
		}
		string pColor = "#95DD5D";
		pTooltip.addLineIntText("world_age", Date.getYear(map_meta.mapStats.world_time), pColor);
		pTooltip.addLineIntText("kingdoms", map_meta.kingdoms, pColor);
		pTooltip.addLineIntText("cultures", map_meta.cultures, pColor);
		pTooltip.addLineIntText("villages", map_meta.cities, pColor);
		pTooltip.addLineIntText("mobs", map_meta.mobs, pColor);
		pTooltip.addLineIntText("population", map_meta.population, pColor);
		if (pTooltip.stats_description.text.Length > 0)
		{
			pTooltip.addLineBreak();
		}
		pTooltip.addLineText("created", map_meta.temp_date_string);
	}

	private void showEquipment(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		Item item = pData.item;
		Transform obj = ((Component)pTooltip).transform.Find("Description/IconBG/LegendaryBG");
		Image component = ((Component)((Component)pTooltip).transform.Find("Description/IconBG/ItemIcon")).GetComponent<Image>();
		Text component2 = ((Component)((Component)pTooltip).transform.Find("Equipment Type/EquipmentText")).GetComponent<Text>();
		EquipmentAsset asset = item.getAsset();
		Sprite sprite = item.getSprite();
		component.sprite = sprite;
		Text component3 = ((Component)((Component)pTooltip).transform.Find("Item Description/item_description_text")).GetComponent<Text>();
		string qualityColor = item.getQualityColor();
		Transform obj2 = ((Component)pTooltip).transform.FindRecursive("Stats");
		bool flag = asset.isAvailable();
		string name = item.getName();
		pTooltip.name.text = Toolbox.coloredText(name, qualityColor);
		if (!flag)
		{
			if (asset.unlocked_with_achievement)
			{
				string text = LocalizedTextManager.getText("item_locked_tooltip_text_achievement");
				string newValue = "<color=#00ffffff>" + asset.getAchievementLocaleID().Localize() + "</color>";
				text = text.Replace("$achievement_id$", newValue);
				component3.text = text;
			}
			else
			{
				component3.text = LocalizedTextManager.getText("item_locked_tooltip_text_exploration");
			}
		}
		else
		{
			BaseStatsHelper.showItemMods(pTooltip.stats_description, pTooltip.stats_values, item);
			BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, item.getFullStats());
			pTooltip.addLineBreak();
			pTooltip.addLineText("durability", item.getDurabilityString());
			if (item.data.kills > 0)
			{
				pTooltip.addLineBreak();
				pTooltip.addItemText("creature_statistics_kills", item.data.kills, pPercent: false, pAddColor: true, pAddPlus: false, "#FF9B1C");
			}
			string pKey = asset.id + "_description";
			bool flag2 = LocalizedTextManager.stringExists(pKey);
			((Component)((Component)component3).transform.parent).gameObject.SetActive(flag2);
			if (flag2)
			{
				component3.text = LocalizedTextManager.getText(pKey);
			}
		}
		((Component)obj2).gameObject.SetActive(flag);
		Rarity quality = item.getQuality();
		((Component)obj).gameObject.SetActive(quality == Rarity.R3_Legendary);
		pTooltip.description.alignment = (TextAnchor)3;
		((Component)component2).GetComponent<LocalizedText>().setKeyAndUpdate(item.getItemKeyType());
		((Graphic)component2).color = Toolbox.makeColor(qualityColor);
		string pDescription = Toolbox.coloredText(item.getItemDescription(), "#FFFFFF");
		pTooltip.setDescription(pDescription);
		showTabBannerTip(pTooltip, pData);
	}

	private void showEquipmentInEditor(Tooltip pTooltip, string pType, TooltipData pData)
	{
		EquipmentAsset item_asset = pData.item_asset;
		string text = LocalizedTextManager.getText("achievement_tip_hidden");
		if (!item_asset.isAvailable())
		{
			pTooltip.name.text = text;
			if (item_asset.unlocked_with_achievement)
			{
				string text2 = LocalizedTextManager.getText("item_locked_tooltip_text_achievement");
				string newValue = "<color=#00ffffff>" + item_asset.getAchievementLocaleID().Localize() + "</color>";
				text2 = text2.Replace("$achievement_id$", newValue);
				pTooltip.setDescription(text2);
			}
			else
			{
				pTooltip.setDescription(LocalizedTextManager.getText("item_locked_tooltip_text_exploration"));
			}
			((Component)((Component)pTooltip).transform.FindRecursive("Stats")).gameObject.SetActive(false);
			return;
		}
		ItemTools.getTooltipTitle(item_asset, out var pName, out var pMaterial);
		pTooltip.name.text = pMaterial + pName;
		string text3 = item_asset.getDescriptionID()?.Localize();
		if (!string.IsNullOrEmpty(text3))
		{
			pTooltip.setDescription(text3);
		}
		else
		{
			pTooltip.resetDescription();
		}
		if (!string.IsNullOrEmpty(pData.tip_description_2))
		{
			string text4 = LocalizedTextManager.getText(pData.tip_description_2);
			pTooltip.setBottomDescription(text4);
		}
		BaseStatsHelper.showBaseStats(pTooltip.stats_description, pTooltip.stats_values, item_asset.base_stats);
	}

	private void showWorldLaw(Tooltip pTooltip, string pType, TooltipData pData)
	{
		WorldLawAsset world_law = pData.world_law;
		pTooltip.name.text = LocalizedTextManager.getText(world_law.getLocaleID());
		string text = LocalizedTextManager.getText(world_law.getDescriptionID());
		if (!InputHelpers.mouseSupported)
		{
			if (world_law.id != "world_law_cursed_world")
			{
				text += "\n\n";
				text += Toolbox.coloredText(LocalizedTextManager.getText("world_laws_tip_mobile_tap"), "#999999");
			}
			else if (!world_law.isEnabled())
			{
				text += "\n\n";
				text += Toolbox.coloredText(LocalizedTextManager.getText("world_laws_tip_mobile_tap_cursed"), "#999999");
			}
		}
		pTooltip.setDescription(text);
		string descriptionID = world_law.getDescriptionID2();
		if (LocalizedTextManager.stringExists(descriptionID))
		{
			string text2 = LocalizedTextManager.getText(descriptionID);
			pTooltip.setBottomDescription(text2);
		}
	}

	private void showWorldAge(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		string tip_name = pData.tip_name;
		WorldAgeAsset worldAgeAsset = AssetManager.era_library.get(tip_name);
		string localeID = worldAgeAsset.getLocaleID();
		string descriptionID = worldAgeAsset.getDescriptionID();
		pTooltip.name.text = Toolbox.coloredText(localeID, Toolbox.colorToHex(Color32.op_Implicit(worldAgeAsset.title_color)), pLocalize: true);
		string text = LocalizedTextManager.getText(descriptionID);
		Sprite sprite = worldAgeAsset.getSprite();
		((Component)((Component)pTooltip).transform.Find("Headline/IconLeft")).GetComponent<Image>().sprite = sprite;
		((Component)((Component)pTooltip).transform.Find("Headline/IconRight")).GetComponent<Image>().sprite = sprite;
		if (Config.isMobile)
		{
			text += "\n\n";
			text += Toolbox.coloredText(LocalizedTextManager.getText("world_age_tip_mobile_tap"), "#999999");
		}
		pTooltip.setDescription(text);
	}

	private void showStatsData(Tooltip pTooltip, string pType, TooltipData pData)
	{
		CustomDataContainer<string> custom_data_string = pData.custom_data_string;
		if (custom_data_string.TryGetValue("value", out var pValue))
		{
			pTooltip.addLineText(pData.tip_name, pValue);
		}
		if (custom_data_string.TryGetValue("max_value", out var pValue2))
		{
			pTooltip.addLineText("max", pValue2);
		}
	}

	private void opinionListToStatsLoyalty(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (pTooltip.opinion_list.Count == 0)
		{
			return;
		}
		pTooltip.opinion_list.Sort(sorter);
		string text = "";
		string text2 = "";
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < pTooltip.opinion_list.Count; i++)
		{
			TooltipOpinionInfo tooltipOpinionInfo = pTooltip.opinion_list[i];
			if (tooltipOpinionInfo.value > 0)
			{
				flag = true;
			}
			if ((tooltipOpinionInfo.value < 0 && !flag2 && i > 0) & flag)
			{
				flag2 = true;
				text += "\n---";
				text2 += "\n---";
			}
			if (i > 0)
			{
				text += "\n";
				text2 += "\n";
			}
			if (tooltipOpinionInfo.value > 0)
			{
				text2 += Toolbox.coloredText(tooltipOpinionInfo.value.ToString("+##,#;-##,#;0"), "#43FF43");
				text += Toolbox.coloredText(LocalizedTextManager.getText(tooltipOpinionInfo.translation_key), "#43FF43");
			}
			else
			{
				text2 += Toolbox.coloredText(tooltipOpinionInfo.value.ToString("+##,#;-##,#;0"), "#FB2C21");
				text += Toolbox.coloredText(LocalizedTextManager.getText(tooltipOpinionInfo.translation_key), "#FB2C21");
			}
		}
		pTooltip.addStatValues(text, text2);
	}

	private void opinionListToStatsDiplomacy(Tooltip pTooltip, string pType, TooltipData pData)
	{
		KingdomOpinion kingdomOpinion = showKingdomOpinion(pTooltip, pType, pData);
		if (pTooltip.opinion_list.Count == 0)
		{
			pTooltip.stats_container.SetActive(false);
			return;
		}
		pTooltip.opinion_list.Sort(sorter);
		string text = "";
		string text2 = "";
		int total = kingdomOpinion.total;
		if (total >= 0)
		{
			text2 += Toolbox.coloredText(total.ToText(), "#43FF43");
			text += Toolbox.coloredText(LocalizedTextManager.getText("opinion_total"), "#43FF43");
		}
		else
		{
			text2 += Toolbox.coloredText(total.ToText(), "#FB2C21");
			text += Toolbox.coloredText(LocalizedTextManager.getText("opinion_total"), "#FB2C21");
		}
		text += "\n------------";
		text2 += "\n------------";
		text += "\n";
		text2 += "\n";
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < pTooltip.opinion_list.Count; i++)
		{
			TooltipOpinionInfo tooltipOpinionInfo = pTooltip.opinion_list[i];
			if (tooltipOpinionInfo.value > 0)
			{
				flag = true;
			}
			if ((tooltipOpinionInfo.value < 0 && !flag2 && i > 0) & flag)
			{
				flag2 = true;
				text += "\n---";
				text2 += "\n---";
			}
			if (i > 0)
			{
				text += "\n";
				text2 += "\n";
			}
			if (tooltipOpinionInfo.value > 0)
			{
				text2 += Toolbox.coloredText(tooltipOpinionInfo.value.ToString("+##,#;-##,#;0"), "#43FF43");
				text += Toolbox.coloredText(LocalizedTextManager.getText(tooltipOpinionInfo.translation_key), "#43FF43");
			}
			else
			{
				text2 += Toolbox.coloredText(tooltipOpinionInfo.value.ToString("+##,#;-##,#;0"), "#FB2C21");
				text += Toolbox.coloredText(LocalizedTextManager.getText(tooltipOpinionInfo.translation_key), "#FB2C21");
			}
		}
		Transform obj = ((Component)pTooltip).transform.Find("StatsOpinion");
		Text component = ((Component)obj.Find("StatsDescription")).GetComponent<Text>();
		Text component2 = ((Component)obj.Find("StatsValues")).GetComponent<Text>();
		component.text = string.Empty;
		component2.text = string.Empty;
		pTooltip.showOpinion(text, text2, component, component2);
		((Component)component2).GetComponent<LocalizedText>().checkSpecialLanguages();
		((Component)component).GetComponent<LocalizedText>().checkSpecialLanguages();
		pTooltip.stats_container.SetActive(true);
	}

	private void showTaxonomy(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		ActorAsset actorAsset = AssetManager.actor_library.get(pData.subspecies.data.species_id);
		string tip_name = pData.tip_name;
		string taxonomyRank = actorAsset.getTaxonomyRank(tip_name);
		string colorForTaxonomy = ColorStyleLibrary.m.getColorForTaxonomy(tip_name);
		((Component)pTooltip.name).GetComponent<LocalizedText>().setKeyAndUpdate(tip_name);
		((Graphic)pTooltip.name).color = Toolbox.makeColor(colorForTaxonomy);
		Text name = pTooltip.name;
		name.text = name.text + "\n" + Toolbox.firstLetterToUpper(taxonomyRank);
		pTooltip.setDescription(LocalizedTextManager.getText("taxonomy_description_tooltip"));
		if (pTooltip.pool_icons == null)
		{
			Transform pParentTransform = ((Component)pTooltip).transform.FindRecursive("Assets");
			StatsIcon pPrefab = Resources.Load<StatsIcon>("ui/PrefabTextIconTooltipBig");
			pTooltip.pool_icons = new ObjectPoolGenericMono<StatsIcon>(pPrefab, pParentTransform);
		}
		foreach (ActorAsset item in AssetManager.actor_library.list)
		{
			if (!item.unit_zombie && item.show_in_taxonomy_tooltip && item.isTaxonomyRank(tip_name, taxonomyRank))
			{
				StatsIcon next = pTooltip.pool_icons.getNext();
				Image icon = next.getIcon();
				icon.sprite = item.getSpriteIcon();
				next.text.text = item.getTranslatedName();
				if (item.isAvailable())
				{
					((Graphic)icon).color = Toolbox.color_white;
				}
				else
				{
					((Graphic)icon).color = Toolbox.color_black;
				}
			}
		}
	}

	private void showColorCounter(Tooltip pTooltip, string pType, TooltipData pData)
	{
		int num = pData.custom_data_int["color_count"];
		pTooltip.setDescription(pData.custom_data_int["color_current"] + " / " + num);
	}

	private void showGameLanguage(Tooltip pTooltip, string pType, TooltipData pData)
	{
		GameLanguageAsset game_language_asset = pData.game_language_asset;
		pTooltip.name.text = game_language_asset.name;
		if (!game_language_asset.export || !game_language_asset.show_translators)
		{
			return;
		}
		GameLanguageData languageData = game_language_asset.getLanguageData();
		if (languageData == null)
		{
			return;
		}
		string[] active = languageData.active;
		if (active != null && active.Length != 0)
		{
			pTooltip.resetDescription();
			pTooltip.addDescription("translators_current_translators".Localize() + ":");
			pTooltip.description.text = "<b>" + pTooltip.description.text + "</b>";
			string[] active2 = languageData.active;
			foreach (string text in active2)
			{
				pTooltip.addDescription("\n" + text);
			}
		}
		string[] inactive = languageData.inactive;
		if (inactive != null && inactive.Length != 0)
		{
			pTooltip.resetBottomDescription();
			pTooltip.addBottomDescription("translators_past_translators".Localize() + ":");
			pTooltip.description_2.text = "<b>" + pTooltip.description_2.text + "</b>";
			string[] active2 = languageData.inactive;
			foreach (string text2 in active2)
			{
				pTooltip.addBottomDescription("\n" + text2);
			}
		}
	}

	private void showAchievement(Tooltip pTooltip, string pType, TooltipData pData)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Achievement achievement = pData.achievement;
		Image component = ((Component)((Component)pTooltip).transform.FindRecursive("IconLeft")).GetComponent<Image>();
		Image component2 = ((Component)((Component)pTooltip).transform.FindRecursive("IconRight")).GetComponent<Image>();
		if (achievement.isUnlocked())
		{
			((Graphic)component).color = Toolbox.color_white;
			((Graphic)component2).color = Toolbox.color_white;
		}
		else
		{
			((Graphic)component).color = Toolbox.color_black;
			((Graphic)component2).color = Toolbox.color_black;
		}
		Sprite icon = achievement.getIcon();
		if ((Object)(object)icon != (Object)null)
		{
			component.sprite = icon;
			component2.sprite = icon;
		}
		string localeID = achievement.getLocaleID();
		((Component)pTooltip.name).GetComponent<LocalizedText>().setKeyAndUpdate(localeID);
		string pString = ((!achievement.hidden || achievement.isUnlocked()) ? achievement.getDescriptionID() : "achievement_tip_hidden");
		string text = pString.Localize();
		text = text.Replace("$lifeissimhours$", 24f.ToText());
		pTooltip.setDescription(text);
		bool flag = achievement.isUnlocked();
		string pName = (flag ? "unlocked" : "locked");
		string pName2 = ((!flag) ? "unlocked" : "locked");
		Transform val = ((Component)pTooltip).transform.FindRecursive(pName);
		((Component)val.parent).gameObject.SetActive(achievement.unlocks_something);
		((Component)((Component)pTooltip).transform.FindRecursive(pName2)).gameObject.SetActive(false);
		if (!achievement.unlocks_something)
		{
			return;
		}
		((Component)val).gameObject.SetActive(true);
		string pString2 = ((pData.achievement.unlock_assets.Count > 1) ? "unlocks_goodies" : "unlocks_goodie");
		pTooltip.setBottomDescription(pString2.Localize());
		ObjectPoolGenericMono<StatsIcon> objectPoolGenericMono;
		if (!flag)
		{
			if (pTooltip.pool_icons == null)
			{
				StatsIcon pPrefab = Resources.Load<StatsIcon>("ui/AchievementGoodieTooltipLocked");
				pTooltip.pool_icons = new ObjectPoolGenericMono<StatsIcon>(pPrefab, val);
			}
			objectPoolGenericMono = pTooltip.pool_icons;
		}
		else
		{
			if (pTooltip.pool_icons_2 == null)
			{
				StatsIcon pPrefab2 = Resources.Load<StatsIcon>("ui/AchievementGoodieTooltipUnlocked");
				pTooltip.pool_icons_2 = new ObjectPoolGenericMono<StatsIcon>(pPrefab2, val);
			}
			objectPoolGenericMono = pTooltip.pool_icons_2;
		}
		foreach (BaseUnlockableAsset unlock_asset in achievement.unlock_assets)
		{
			((Component)objectPoolGenericMono.getNext()).GetComponent<AchievementGoodie>().load(unlock_asset, flag);
		}
	}

	public int sorter(TooltipOpinionInfo p1, TooltipOpinionInfo p2)
	{
		return p2.value.CompareTo(p1.value);
	}

	protected void setIconSprite(Tooltip pTooltip, string pName, string pIconName)
	{
		Transform val = ((Component)pTooltip).transform.FindRecursive(pName);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)("No icon with this name! " + pName));
		}
		else
		{
			((Component)val).GetComponent<StatsIcon>().getIcon().sprite = SpriteTextureLoader.getSprite("ui/Icons/" + pIconName);
		}
	}

	protected void setIconValue(Tooltip pTooltip, string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		Transform val = ((Component)pTooltip).transform.FindRecursive(pName);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)("No icon with this name! " + pName));
			return;
		}
		StatsIcon component = ((Component)val).GetComponent<StatsIcon>();
		component.enable_animation = false;
		component.setValue(pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
	}

	private void showTabBannerTip(Tooltip pTooltip, TooltipData pData)
	{
		if (Config.isComputer || Config.isEditor)
		{
			CustomDataContainer<bool> custom_data_bool = pData.custom_data_bool;
			if (custom_data_bool != null && custom_data_bool["tab_banner"])
			{
				string text = LocalizedTextManager.getText("tab_banner_show_window");
				text = AssetManager.hotkey_library.replaceSpecialTextKeys(text);
				pTooltip.setBottomDescription(text);
			}
		}
	}

	private void initDebug()
	{
		add(new TooltipAsset
		{
			id = "debug_asset",
			prefab_id = "tooltips/tooltip_asset_debug",
			callback = showAssetDebug
		});
		add(new TooltipAsset
		{
			id = "debug_collection",
			prefab_id = "tooltips/tooltip_collection_data",
			callback = showCollectionData
		});
	}

	private void showAssetDebug(Tooltip pTooltip, string pType, TooltipData pData)
	{
		if (pData.tip_name == "actor")
		{
			showActorAssetDebug(pTooltip, pType, pData);
		}
		if (pData.tip_name == "building")
		{
			showBuildingAssetDebug(pTooltip, pType, pData);
		}
	}

	private void showActorAssetDebug(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Sprite spriteIcon = BaseDebugAssetElement<ActorAsset>.selected_asset.getSpriteIcon();
		((Component)((Component)pTooltip).transform.FindRecursive("IconSpecial")).GetComponent<Image>().sprite = spriteIcon;
		((Component)((Component)pTooltip).transform.FindRecursive("IconRace")).GetComponent<Image>().sprite = spriteIcon;
		using ListPool<string> pFields = new ListPool<string> { "id", "icon", "has_skin", "banner_id", "skin_civ_default_male", "skin_civ_default_female" };
		showAssetDebug<ActorAsset>(pTooltip, pFields);
	}

	private void showBuildingAssetDebug(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/iconHouseTier0");
		((Component)((Component)pTooltip).transform.FindRecursive("IconSpecial")).GetComponent<Image>().sprite = sprite;
		((Component)((Component)pTooltip).transform.FindRecursive("IconRace")).GetComponent<Image>().sprite = sprite;
		using ListPool<string> pFields = new ListPool<string> { "id", "civ_kingdom", "can_be_upgraded", "upgrade_to", "housing_slots", "spawn_units_asset", "spawn_drop_id" };
		showAssetDebug<BuildingAsset>(pTooltip, pFields);
	}

	private void showAssetDebug<TAsset>(Tooltip pTooltip, ListPool<string> pFields) where TAsset : Asset
	{
		TAsset selected_asset = BaseDebugAssetElement<TAsset>.selected_asset;
		pTooltip.name.text = selected_asset.id;
		FieldInfoList componentInChildren = ((Component)pTooltip).GetComponentInChildren<FieldInfoList>();
		componentInChildren.init<TAsset>(pFields);
		componentInChildren.setData(selected_asset);
		pTooltip.setDescription("Need description to fix rounded tooltip");
	}

	private void showCollectionData(Tooltip pTooltip, string pType, TooltipData pData)
	{
		Dictionary<string, string> selected_field_data = FieldInfoList.selected_field_data;
		if (selected_field_data == null)
		{
			pTooltip.setDescription("Nothing to show");
			return;
		}
		FieldInfoList componentInChildren = ((Component)pTooltip).GetComponentInChildren<FieldInfoList>();
		componentInChildren.checkInitPool();
		foreach (KeyValuePair<string, string> item in selected_field_data)
		{
			componentInChildren.addRow(item.Key, item.Value);
		}
		pTooltip.setDescription("need description to fix rounded tooltip");
	}
}
