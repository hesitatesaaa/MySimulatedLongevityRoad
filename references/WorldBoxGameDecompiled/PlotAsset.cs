using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class PlotAsset : BaseAugmentationAsset, IDescription2Asset, IDescriptionAsset, ILocalizedAsset
{
	public int limit_members;

	[DefaultValue(1)]
	public int pot_rate = 1;

	public int min_level;

	public int min_renown_kingdom;

	public int min_renown_actor;

	[DefaultValue(2)]
	public int min_intelligence = 2;

	[DefaultValue(2)]
	public int min_diplomacy = 2;

	[DefaultValue(2)]
	public int min_warfare = 2;

	[DefaultValue(2)]
	public int min_stewardship = 2;

	public bool can_be_done_by_king;

	public bool can_be_done_by_leader;

	public bool can_be_done_by_clan_member;

	public bool requires_diplomacy;

	public bool requires_rebellion;

	[DefaultValue(60f)]
	public float progress_needed = 60f;

	[DefaultValue(5)]
	public int money_cost = 5;

	public bool is_basic_plot;

	public Rarity rarity;

	public PlotDescription get_formatted_description;

	public PlotCheckerDelegate check_is_possible;

	public PlotCheckerDelegate check_can_be_forced;

	public PlotCheckerDelegate check_should_continue;

	public PlotActorPlotDelegate check_other_plots;

	public PlotTryToStart try_to_start_advanced;

	public PlotStart start;

	public PlotAction action;

	public PlotAction post_action;

	public PlotActorPlotDelegate check_supporters;

	public bool check_target_actor;

	public bool check_target_city;

	public bool check_target_kingdom;

	public bool check_target_alliance;

	public bool check_target_war;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_plots;

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.plot_category_library.get(group_id);
	}

	public PlotCategoryAsset getPlotGroup()
	{
		return AssetManager.plot_category_library.get(group_id);
	}

	public bool checkIsPossible(Actor pActor, bool pWorldLawsCheck = true)
	{
		if (!pActor.hasEnoughMoney(money_cost))
		{
			return false;
		}
		if (pActor.level < min_level)
		{
			return false;
		}
		if (pActor.renown < min_renown_actor)
		{
			return false;
		}
		if (pActor.kingdom.getRenown() < min_renown_kingdom)
		{
			return false;
		}
		if (pActor.intelligence < min_intelligence)
		{
			return false;
		}
		if (pActor.diplomacy < min_diplomacy)
		{
			return false;
		}
		if (pActor.warfare < min_warfare)
		{
			return false;
		}
		if (pActor.stewardship < min_stewardship)
		{
			return false;
		}
		if (!canBeDoneByRole(pActor))
		{
			return false;
		}
		if (pWorldLawsCheck && !isAllowedByWorldLaws())
		{
			return false;
		}
		if (!check_is_possible(pActor))
		{
			return false;
		}
		return true;
	}

	public bool canBeDoneByRole(Actor pActor)
	{
		if (can_be_done_by_king && pActor.isKing())
		{
			return true;
		}
		if (can_be_done_by_leader && pActor.isCityLeader())
		{
			return true;
		}
		if (can_be_done_by_clan_member && pActor.hasClan())
		{
			return true;
		}
		return false;
	}

	public bool isAllowedByWorldLaws()
	{
		if (requires_diplomacy && !isDiplomacyON())
		{
			return false;
		}
		if (requires_rebellion && !isRebellionON())
		{
			return false;
		}
		return true;
	}

	private static bool isDiplomacyON()
	{
		return WorldLawLibrary.world_law_diplomacy.isEnabled();
	}

	private static bool isRebellionON()
	{
		return WorldLawLibrary.world_law_rebellions.isEnabled();
	}

	protected override bool isDebugUnlockedAll()
	{
		return DebugConfig.isOn(DebugOption.UnlockAllPlots);
	}

	public override string getLocaleID()
	{
		return "plot_" + id;
	}

	public string getDescriptionID()
	{
		return "plot_" + id + "_info";
	}

	public string getDescriptionID2()
	{
		return "plot_" + id + "_info_base";
	}
}
