public class PlotCategoryLibrary : BaseCategoryLibrary<PlotCategoryAsset>
{
	public override void init()
	{
		base.init();
		add(new PlotCategoryAsset
		{
			id = "diplomacy",
			name = "plot_group_diplomacy",
			color = "#5EFFFF",
			show_counter = false,
			plot_retry_action = diplomacyRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "rites_wrathful",
			name = "plot_group_rites_wrathful",
			color = "#FF6145",
			show_counter = false,
			plot_retry_action = ritesRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "rites_summoning",
			name = "plot_group_rites_summoning",
			color = "#BC42FF",
			show_counter = false,
			plot_retry_action = ritesRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "rites_merciful",
			name = "plot_group_rites_merciful",
			color = "#89FF56",
			show_counter = false,
			plot_retry_action = ritesRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "culture",
			name = "plot_group_culture",
			color = "#FFDA23",
			show_counter = false,
			plot_retry_action = culturePlotsRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "language",
			name = "plot_group_language",
			color = "#A3AFFF",
			show_counter = false,
			plot_retry_action = culturePlotsRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "religion",
			name = "plot_group_religion",
			color = "#BAF0F4",
			show_counter = false,
			plot_retry_action = culturePlotsRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "rites_various",
			name = "plot_group_rites_various",
			color = "#d86569",
			show_counter = false,
			plot_retry_action = ritesRetryAction
		});
		add(new PlotCategoryAsset
		{
			id = "plots_others",
			name = "plot_group_others",
			color = "#D8D8D8",
			show_counter = false,
			plot_retry_action = ritesRetryAction
		});
	}

	public static bool diplomacyRetryAction()
	{
		if (World.world.cities.isLocked())
		{
			return true;
		}
		if (World.world.kingdoms.isLocked())
		{
			return true;
		}
		if (World.world.alliances.isLocked())
		{
			return true;
		}
		if (World.world.wars.isLocked())
		{
			return true;
		}
		if (World.world.clans.isLocked())
		{
			return true;
		}
		return false;
	}

	public static bool ritesRetryAction()
	{
		if (World.world.cities.isLocked())
		{
			return true;
		}
		if (World.world.kingdoms.isLocked())
		{
			return true;
		}
		if (World.world.clans.isLocked())
		{
			return true;
		}
		if (World.world.religions.isLocked())
		{
			return true;
		}
		if (World.world.subspecies.isLocked())
		{
			return true;
		}
		return false;
	}

	public static bool culturePlotsRetryAction()
	{
		if (World.world.cultures.isLocked())
		{
			return true;
		}
		if (World.world.religions.isLocked())
		{
			return true;
		}
		if (World.world.languages.isLocked())
		{
			return true;
		}
		return false;
	}
}
