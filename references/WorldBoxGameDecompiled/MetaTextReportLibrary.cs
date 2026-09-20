public class MetaTextReportLibrary : AssetLibrary<MetaTextReportAsset>
{
	public override void init()
	{
		base.init();
		addGeneralMeta();
		addCity();
	}

	private void addCity()
	{
		add(new MetaTextReportAsset
		{
			id = "happy",
			color = "#ADADAD",
			report_action = (IMetaObject pObject) => pObject.getRatioHappy() > 0.8f
		});
		add(new MetaTextReportAsset
		{
			id = "unhappy",
			color = "#919191",
			report_action = (IMetaObject pObject) => pObject.getRatioUnhappy() > 0.8f
		});
		add(new MetaTextReportAsset
		{
			id = "many_children",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				if (pObject.countUnits() < 20)
				{
					return false;
				}
				return pObject.getRatioChildren() > 0.7f;
			}
		});
		add(new MetaTextReportAsset
		{
			id = "many_homeless",
			color = "#919191",
			report_action = delegate(IMetaObject pObject)
			{
				if (pObject.countUnits() < 20)
				{
					return false;
				}
				return pObject.getRatioHomeless() > 0.8f;
			}
		});
		add(new MetaTextReportAsset
		{
			id = "food_plenty",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				City obj = pObject as City;
				int num = obj.countFoodTotal();
				int populationPeople = obj.getPopulationPeople();
				return (num > populationPeople * 4) ? true : false;
			}
		});
		add(new MetaTextReportAsset
		{
			id = "food_running_out",
			color = "#919191",
			report_action = delegate(IMetaObject pObject)
			{
				City city = pObject as City;
				int num = city.countFoodTotal();
				if (num == 0)
				{
					return false;
				}
				int populationPeople = city.getPopulationPeople();
				return (num < populationPeople * 2) ? true : false;
			}
		});
		add(new MetaTextReportAsset
		{
			id = "food_none",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as City).countFoodTotal() == 0
		});
		add(new MetaTextReportAsset
		{
			id = "stone_none",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as City).amount_stone == 0
		});
		add(new MetaTextReportAsset
		{
			id = "wood_none",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as City).amount_wood == 0
		});
		add(new MetaTextReportAsset
		{
			id = "metal_none",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as City).amount_common_metals == 0
		});
		add(new MetaTextReportAsset
		{
			id = "gold_none",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as City).amount_gold == 0
		});
		add(new MetaTextReportAsset
		{
			id = "war_high_casualties",
			color = "#919191",
			report_action = (IMetaObject pObject) => (pObject as War).getTotalDeaths() > 100
		});
		add(new MetaTextReportAsset
		{
			id = "war_long",
			color = "#919191",
			report_action = (IMetaObject pObject) => pObject.getAge() > 100
		});
		add(new MetaTextReportAsset
		{
			id = "war_fresh",
			color = "#ADADAD",
			report_action = (IMetaObject pObject) => pObject.getAge() < 5
		});
		add(new MetaTextReportAsset
		{
			id = "war_defenders_getting_captured",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				War pWar = pObject as War;
				if (!pWar.areDefendersGettingCaptured())
				{
					return false;
				}
				return !pWar.areAttackersGettingCaptured();
			}
		});
		add(new MetaTextReportAsset
		{
			id = "war_attackers_getting_captured",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				War pWar = pObject as War;
				if (!pWar.areAttackersGettingCaptured())
				{
					return false;
				}
				return !pWar.areDefendersGettingCaptured();
			}
		});
		add(new MetaTextReportAsset
		{
			id = "war_quiet",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				War pWar = pObject as War;
				if (pWar.areAttackersGettingCaptured())
				{
					return false;
				}
				return !pWar.areDefendersGettingCaptured();
			}
		});
		add(new MetaTextReportAsset
		{
			id = "war_full_on_battle",
			color = "#ADADAD",
			report_action = delegate(IMetaObject pObject)
			{
				War pWar = pObject as War;
				if (!pWar.areAttackersGettingCaptured())
				{
					return false;
				}
				return pWar.areDefendersGettingCaptured() ? true : false;
			}
		});
	}

	private void addGeneralMeta()
	{
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MetaTextReportAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
	}
}
