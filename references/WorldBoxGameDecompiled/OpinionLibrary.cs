public class OpinionLibrary : AssetLibrary<OpinionAsset>
{
	public override void init()
	{
		base.init();
		add(new OpinionAsset
		{
			id = "opinion_king",
			translation_key = "opinion_king",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				if (pTarget.hasKing())
				{
					num += (int)pTarget.king.stats["diplomacy"];
				}
				return num;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_kings_mood",
			translation_key = "opinion_kings_mood",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pMain.hasKing())
				{
					result = (int)pMain.king.stats["opinion"];
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_is_supreme",
			translation_key = "opinion_is_supreme",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pTarget.isSupreme() && World.world.kingdoms.Count >= 3)
				{
					result = -100;
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_borders",
			translation_key = "opinion_far_borders",
			translation_key_negative = "opinion_close_borders",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				return DiplomacyHelpers.areKingdomsClose(pMain, pTarget) ? (-25) : 25;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_far_lands",
			translation_key = "opinion_far_lands",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (!DiplomacyHelpers.areKingdomsClose(pMain, pTarget) && pMain.hasCapital() && pTarget.hasCapital())
				{
					WorldTile tile = pMain.capital.getTile();
					WorldTile tile2 = pTarget.capital.getTile();
					if (tile != null && tile2 != null && !tile.isSameIsland(tile2))
					{
						result = 60;
					}
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_in_war",
			translation_key = "opinion_in_war",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pMain.isEnemy(pTarget))
				{
					result = -500;
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_same_wars",
			translation_key = "opinion_same_wars",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (pMain.isEnemy(pTarget))
				{
					return 0;
				}
				return pMain.isInWarOnSameSide(pTarget) ? 50 : 0;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_species",
			translation_key = "opinion_same_species",
			translation_key_negative = "opinion_different_species",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				Actor king = pMain.king;
				Actor king2 = pTarget.king;
				if (king == null || king2 == null)
				{
					return 0;
				}
				if (!king.canHavePrejudice())
				{
					return 0;
				}
				return (pMain.getSpecies() == pTarget.getSpecies()) ? 15 : (-10);
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_zones",
			translation_key = "opinion_zones",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				foreach (City city in pMain.getCities())
				{
					num2 += city.zones.Count;
				}
				foreach (City city2 in pTarget.getCities())
				{
					num3 += city2.zones.Count;
				}
				num = (num2 - num3) / 5;
				if (num > 0)
				{
					num = 0;
				}
				if (num < -20)
				{
					num = -20;
				}
				return num;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_peace_time",
			translation_key = "opinion_peace_time",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				if (pMain.isEnemy(pTarget))
				{
					return 0;
				}
				DiplomacyRelation relation = World.world.diplomacy.getRelation(pMain, pTarget);
				if (relation == null)
				{
					return 0;
				}
				float num2 = Date.getYearsSince(relation.data.timestamp_last_war_ended);
				if (num2 <= (float)SimGlobals.m.minimum_years_between_wars)
				{
					return 0;
				}
				num = (int)num2;
				if (num2 > 20f)
				{
					num = 20;
				}
				return num;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_power",
			translation_key = "opinion_power",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				int num = pMain.countCities() * 5 + pMain.getPopulationPeople();
				int num2 = pTarget.countCities() * 5 + pTarget.getPopulationPeople() - num;
				if (num2 > 0)
				{
					result = num2 / 10;
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_loyalty_traits",
			translation_key = "opinion_traits",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				if (pTarget.hasCulture() && pMain.hasKing() && pTarget.hasKing())
				{
					int num2 = AssetManager.traits.checkTraitsMod(pTarget.king, pMain.king);
					num += num2;
				}
				return num;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_culture",
			translation_key = "opinion_culture_same",
			translation_key_negative = "opinion_culture_different",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pTarget.hasCulture())
				{
					result = ((pTarget.getCulture() != pMain.getCulture()) ? (-15) : 15);
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_religion",
			translation_key = "opinion_religion_same",
			translation_key_negative = "opinion_religion_different",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pTarget.hasReligion())
				{
					result = ((pTarget.getReligion() != pMain.getReligion()) ? (-15) : 15);
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_language",
			translation_key = "opinion_language_same",
			translation_key_negative = "opinion_language_different",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pTarget.hasLanguage())
				{
					result = ((pTarget.getLanguage() != pMain.getLanguage()) ? (-15) : 15);
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_subspecies",
			translation_key = "opinion_subspecies_same",
			translation_key_negative = "opinion_subspecies_different",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				if (!pMain.hasKing())
				{
					return 0;
				}
				if (pMain.getSpecies() == pTarget.getSpecies())
				{
					return 0;
				}
				if (!pMain.king.canHavePrejudice())
				{
					return 0;
				}
				return (pTarget.getMainSubspecies() == pMain.getMainSubspecies()) ? 10 : (-10);
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_clan",
			translation_key = "opinion_same_clan",
			translation_key_negative = "opinion_different_clan",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int num = 0;
				Actor king = pMain.king;
				Actor king2 = pTarget.king;
				if (king == null || king2 == null)
				{
					return 0;
				}
				if (king.subspecies != king2.subspecies)
				{
					return 0;
				}
				if (!king.hasClan() || !king2.hasClan())
				{
					return 0;
				}
				if (!king.canHavePrejudice())
				{
					return 0;
				}
				return (king.clan == king2.clan) ? 40 : (-40);
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_in_alliance",
			translation_key = "opinion_in_alliance",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				int result = 0;
				if (pTarget.hasAlliance() && pMain.hasAlliance() && pTarget.getAlliance() == pMain.getAlliance())
				{
					result = 30;
				}
				return result;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_truce",
			translation_key = "opinion_truce",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (pMain.isEnemy(pTarget))
				{
					return 0;
				}
				return ((float)Date.getYearsSince(World.world.diplomacy.getRelation(pMain, pTarget).data.timestamp_last_war_ended) <= (float)SimGlobals.m.minimum_years_between_wars) ? 100 : 0;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_world_era",
			translation_key = "opinion_world_era",
			calc = (Kingdom pMain, Kingdom pTarget) => World.world_era.bonus_opinion
		});
		add(new OpinionAsset
		{
			id = "opinion_baby_king",
			translation_key = "opinion_baby_king",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (!pMain.hasKing())
				{
					return 0;
				}
				if (!pTarget.hasKing())
				{
					return 0;
				}
				if (pMain.king.isBaby())
				{
					return 0;
				}
				return pTarget.king.isBaby() ? (-50) : 0;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_ethnocentric_guard",
			translation_key = "opinion_ethnocentric_guard",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (!pMain.hasCulture())
				{
					return 0;
				}
				if (!pMain.culture.hasTrait("ethnocentric_guard"))
				{
					return 0;
				}
				if (pMain.getMainSubspecies() != pTarget.getMainSubspecies())
				{
					return 0;
				}
				return (pMain.culture != pTarget.culture) ? (-50) : 0;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_xenophobic",
			translation_key = "opinion_xenophobic",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (!pMain.hasCulture())
				{
					return 0;
				}
				if (!pMain.culture.hasTrait("xenophobic"))
				{
					return 0;
				}
				return (pMain.getMainSubspecies() != pTarget.getMainSubspecies()) ? (-50) : 0;
			}
		});
		add(new OpinionAsset
		{
			id = "opinion_xenophiles",
			translation_key = "opinion_xenophiles",
			calc = delegate(Kingdom pMain, Kingdom pTarget)
			{
				if (!pMain.hasCulture())
				{
					return 0;
				}
				if (!pMain.culture.hasTrait("xenophiles"))
				{
					return 0;
				}
				return (pMain.getMainSubspecies() == pTarget.getMainSubspecies()) ? 20 : 0;
			}
		});
	}

	public override void editorDiagnosticLocales()
	{
		foreach (OpinionAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
		base.editorDiagnosticLocales();
	}
}
