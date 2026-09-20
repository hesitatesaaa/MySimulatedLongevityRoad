using UnityEngine;

public class LoyaltyLibrary : AssetLibrary<LoyaltyAsset>
{
	public override void init()
	{
		base.init();
		add(new LoyaltyAsset
		{
			id = "king_diplomacy",
			translation_key = "loyalty_king",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (!pCity.kingdom.hasKing())
				{
					return result;
				}
				Actor king = pCity.kingdom.king;
				if (king.isAlive())
				{
					result = (int)(king.stats["diplomacy"] + king.stats["stewardship"] * 2f);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "leader_diplomacy",
			translation_key = "loyalty_leader",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (!pCity.hasLeader())
				{
					return result;
				}
				Actor leader = pCity.leader;
				if (leader.isAlive())
				{
					result = -(int)(leader.stats["diplomacy"] + leader.stats["stewardship"] * 2f);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "leader_loyalty",
			translation_key = "loyalty_traits",
			calc = delegate(City pCity)
			{
				int num = 0;
				if (pCity.hasLeader())
				{
					num = (int)pCity.leader.stats["loyalty_traits"];
				}
				if (pCity.hasLeader() && pCity.kingdom.hasKing())
				{
					int num2 = AssetManager.traits.checkTraitsMod(pCity.leader, pCity.kingdom.king);
					num += num2;
				}
				return num;
			}
		});
		add(new LoyaltyAsset
		{
			id = "population",
			translation_key = "loyalty_population",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return result;
				}
				if (pCity.kingdom.hasCapital())
				{
					int num = Mathf.Abs(pCity.status.population - pCity.kingdom.capital.status.population) / 3;
					if (num > 30)
					{
						num = 30;
					}
					result = ((pCity.status.population <= pCity.kingdom.capital.status.population) ? num : (-num));
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "zones",
			translation_key = "loyalty_zones",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return result;
				}
				if (pCity.kingdom.hasCapital())
				{
					int num = Mathf.Abs(pCity.zones.Count - pCity.kingdom.capital.zones.Count) / 20;
					if (num > 5)
					{
						num = 5;
					}
					result = ((pCity.zones.Count <= pCity.kingdom.capital.zones.Count) ? num : (-num));
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "distance",
			translation_key = "loyalty_distance",
			calc = delegate(City pCity)
			{
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return result;
				}
				if (pCity.kingdom.hasCapital() && pCity.city_center.x != Globals.POINT_IN_VOID_2.x && pCity.kingdom.capital.city_center.x != Globals.POINT_IN_VOID_2.x)
				{
					result = -(int)(Toolbox.DistVec2Float(pCity.city_center, pCity.kingdom.capital.city_center) / 10f);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "capital",
			translation_key = "loyalty_capital",
			calc = (City pCity) => pCity.isCapitalCity() ? 1000 : 0
		});
		add(new LoyaltyAsset
		{
			id = "mood",
			translation_key = "loyalty_leader_mood",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.hasLeader())
				{
					result = (int)pCity.leader.stats["loyalty_mood"];
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "new_city",
			translation_key = "loyalty_new_city",
			calc = delegate(City pCity)
			{
				int result = 0;
				int age = pCity.getAge();
				int num = 15;
				if (age <= num)
				{
					result = (num - age) * 5;
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "new_kingdom",
			translation_key = "loyalty_new_kingdom",
			calc = delegate(City pCity)
			{
				int result = 0;
				int age = pCity.kingdom.getAge();
				if (age <= 5)
				{
					result = (5 - age) * 5;
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "cities",
			translation_key = "loyalty_number_of_cities",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				int maxCities = pCity.kingdom.getMaxCities();
				int num = pCity.kingdom.countCities();
				if (num > maxCities)
				{
					result = (maxCities - num) * 25;
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "superior_enemies",
			translation_key = "loyalty_superior_enemies",
			calc = delegate(City pCity)
			{
				int num = 0;
				if (pCity.kingdom.hasEnemies())
				{
					int num2 = 0;
					using ListPool<Kingdom> listPool = World.world.wars.getEnemiesOf(pCity.kingdom);
					foreach (ref Kingdom item in listPool)
					{
						Kingdom current = item;
						num2 += current.power;
					}
					num = (num2 - pCity.kingdom.power) / 2;
					if (num < 0)
					{
						num = 0;
					}
					else if (num > 50)
					{
						num = 50;
					}
				}
				return num;
			}
		});
		add(new LoyaltyAsset
		{
			id = "close_to_capital",
			translation_key = "loyalty_close_to_capital",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (pCity.kingdom.hasCapital() && City.nearbyBorders(pCity.kingdom.capital, pCity))
				{
					result = 20;
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "connected_to_capital",
			translation_key = "loyalty_connected_to_capital",
			translation_key_negative = "loyalty_not_connected_to_capital",
			calc = delegate(City pCity)
			{
				if (!pCity.kingdom.hasCapital())
				{
					return 0;
				}
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (pCity.kingdom.capital.getSpecies() != pCity.getSpecies())
				{
					return 0;
				}
				return pCity.isConnectedToCapital() ? 20 : (-35);
			}
		});
		add(new LoyaltyAsset
		{
			id = "culture",
			translation_key = "loyalty_culture",
			translation_key_negative = "opinion_culture_different",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (!pCity.hasCulture())
				{
					return 0;
				}
				if (pCity.kingdom.hasCapital())
				{
					result = ((pCity.kingdom.capital.culture != pCity.culture) ? (-25) : 15);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "language",
			translation_key = "loyalty_language",
			translation_key_negative = "opinion_language_different",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (!pCity.hasLanguage())
				{
					return 0;
				}
				if (pCity.kingdom.hasCapital())
				{
					result = ((pCity.kingdom.capital.language != pCity.language) ? (-20) : 15);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "religion",
			translation_key = "loyalty_religion",
			translation_key_negative = "opinion_religion_different",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (!pCity.hasReligion())
				{
					return 0;
				}
				if (pCity.kingdom.hasCapital())
				{
					result = ((pCity.kingdom.capital.religion != pCity.religion) ? (-30) : 15);
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "species",
			translation_key = "loyalty_species",
			translation_key_negative = "loyalty_species_different",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				if (pCity.kingdom.hasCapital())
				{
					if (pCity.kingdom.capital.getSpecies() == pCity.getSpecies())
					{
						result = 0;
					}
					else
					{
						result = ((!pCity.hasLeader()) ? (-25) : ((!pCity.leader.hasXenophiles()) ? (-25) : (-5)));
						if (pCity.hasLeader() && (pCity.leader.hasXenophobic() || (pCity.kingdom.hasKing() && pCity.kingdom.king.hasXenophobic())))
						{
							result = -50;
						}
					}
				}
				return result;
			}
		});
		add(new LoyaltyAsset
		{
			id = "subspecies",
			translation_key = "loyalty_subspecies",
			translation_key_negative = "opinion_subspecies_different",
			calc = delegate(City pCity)
			{
				int num = 0;
				if (!pCity.kingdom.hasCapital())
				{
					return 0;
				}
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				City capital = pCity.kingdom.capital;
				if (capital.getSpecies() != pCity.getSpecies())
				{
					return 0;
				}
				return (capital.getMainSubspecies() == pCity.getMainSubspecies()) ? 15 : (-15);
			}
		});
		add(new LoyaltyAsset
		{
			id = "clan",
			translation_key = "loyalty_same_clan",
			translation_key_negative = "loyalty_different_clans",
			calc = delegate(City pCity)
			{
				int result = 0;
				if (pCity.isCapitalCity())
				{
					return 0;
				}
				Actor leader = pCity.leader;
				Actor king = pCity.kingdom.king;
				if (!pCity.hasLeader() || !pCity.kingdom.hasKing())
				{
					return 0;
				}
				if (king.subspecies != leader.subspecies)
				{
					return result;
				}
				return (pCity.leader.clan == pCity.kingdom.king.clan) ? 30 : (-20);
			}
		});
		add(new LoyaltyAsset
		{
			id = "new_conquest",
			translation_key = "loyalty_new_conquest",
			calc = delegate(City pCity)
			{
				Kingdom kingdom = pCity.kingdom;
				if (kingdom.data.timestamp_new_conquest == -1.0)
				{
					return 0;
				}
				int yearsSince = Date.getYearsSince(kingdom.data.timestamp_new_conquest);
				int num = 10;
				return (yearsSince <= num) ? ((num - yearsSince) * 30) : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "part_of_kingdom",
			translation_key = "loyalty_part_of_kingdom",
			calc = delegate(City pCity)
			{
				int yearsSince = Date.getYearsSince(pCity.data.timestamp_kingdom);
				int num = 10;
				return (yearsSince <= num) ? ((num - yearsSince) * 10) : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "supreme_kingdom",
			translation_key = "loyalty_supreme_kingdom",
			calc = delegate(City pCity)
			{
				if (World.world.kingdoms.Count <= 1)
				{
					return 0;
				}
				return pCity.kingdom.isSupreme() ? 100 : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "second_best_kingdom",
			translation_key = "loyalty_second_best",
			calc = delegate(City pCity)
			{
				if (World.world.kingdoms.Count <= 2)
				{
					return 0;
				}
				return pCity.kingdom.isSecondBest() ? 50 : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "king_rule",
			translation_key = "loyalty_king_ruled",
			calc = delegate(City pCity)
			{
				if (!pCity.kingdom.hasKing())
				{
					return 0;
				}
				int yearsSince = Date.getYearsSince(pCity.kingdom.data.timestamp_king_rule);
				int num = 5;
				int num2 = 40;
				if (yearsSince < num)
				{
					return 0;
				}
				return (yearsSince > num2) ? num2 : yearsSince;
			}
		});
		add(new LoyaltyAsset
		{
			id = "loyalty_world_era",
			translation_key = "loyalty_world_era",
			calc = (City pCity) => World.world_era.bonus_loyalty
		});
		add(new LoyaltyAsset
		{
			id = "loyalty_baby_king",
			translation_key = "loyalty_baby_king",
			calc = delegate(City pCity)
			{
				if (!pCity.kingdom.hasKing())
				{
					return 0;
				}
				return (pCity.kingdom.king.getAge() < 18) ? (-50) : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "opinion_patriarchy",
			translation_key = "opinion_patriarchy",
			calc = delegate(City pCity)
			{
				Culture culture = pCity.culture;
				Kingdom kingdom = pCity.kingdom;
				if (!pCity.hasCulture())
				{
					return 0;
				}
				if (culture != kingdom.culture)
				{
					return 0;
				}
				if (!culture.hasTrait("patriarchy"))
				{
					return 0;
				}
				if (!kingdom.hasKing())
				{
					return 0;
				}
				return (!kingdom.king.isSexMale()) ? (-50) : 0;
			}
		});
		add(new LoyaltyAsset
		{
			id = "opinion_matriarchy",
			translation_key = "opinion_matriarchy",
			calc = delegate(City pCity)
			{
				Culture culture = pCity.culture;
				Kingdom kingdom = pCity.kingdom;
				if (!pCity.hasCulture())
				{
					return 0;
				}
				if (culture != kingdom.culture)
				{
					return 0;
				}
				if (!culture.hasTrait("matriarchy"))
				{
					return 0;
				}
				if (!kingdom.hasKing())
				{
					return 0;
				}
				return (!kingdom.king.isSexFemale()) ? (-50) : 0;
			}
		});
	}

	public override void editorDiagnosticLocales()
	{
		foreach (LoyaltyAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
		base.editorDiagnosticLocales();
	}
}
