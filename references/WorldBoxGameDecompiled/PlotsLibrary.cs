using System;
using System.Collections.Generic;
using UnityEngine;

public class PlotsLibrary : BaseLibraryWithUnlockables<PlotAsset>
{
	public static PlotAsset rebellion;

	public static PlotAsset new_war;

	public static PlotAsset alliance_create;

	[NonSerialized]
	public readonly List<PlotAsset> basic_plots = new List<PlotAsset>();

	private static bool tryStartReligionRiteOnEnemyCity(Actor pActor, PlotAsset pPlotAsset, bool pForced)
	{
		using ListPool<Kingdom> listPool = pActor.kingdom.getEnemiesKingdoms();
		if (listPool.Count == 0)
		{
			return false;
		}
		Kingdom random = listPool.GetRandom();
		if (!random.hasCities())
		{
			return false;
		}
		World.world.plots.newPlot(pActor, pPlotAsset, pForced).target_city = random.getCities().GetRandom();
		return true;
	}

	private static bool tryStartCauseRebellion(Actor pActor, PlotAsset pPlotAsset, bool pForced)
	{
		using ListPool<Kingdom> listPool = pActor.kingdom.getEnemiesKingdoms();
		if (listPool.Count == 0)
		{
			return false;
		}
		Kingdom random = listPool.GetRandom();
		if (!random.hasCities())
		{
			return false;
		}
		using ListPool<City> listPool2 = new ListPool<City>();
		foreach (City city in random.getCities())
		{
			if (!city.isCapitalCity())
			{
				listPool2.Add(city);
			}
		}
		if (listPool2.Count == 0)
		{
			return false;
		}
		World.world.plots.newPlot(pActor, pPlotAsset, pForced).target_city = listPool2.GetRandom();
		return true;
	}

	private static bool tryStartReligionRiteOnSelfCity(Actor pActor, PlotAsset pPlotAsset, bool pForced)
	{
		World.world.plots.newPlot(pActor, pPlotAsset, pForced).target_city = pActor.city;
		return true;
	}

	private static bool tryStartReligionRiteOnSelfKingdom(Actor pActor, PlotAsset pPlotAsset, bool pForced)
	{
		World.world.plots.newPlot(pActor, pPlotAsset, pForced).target_kingdom = pActor.kingdom;
		return true;
	}

	private static bool checkShouldContinueReligionRiteOnEnemyCity(Actor pActor)
	{
		if (!pActor.hasKingdom())
		{
			return false;
		}
		if (!pActor.hasReligion())
		{
			return false;
		}
		if (!pActor.kingdom.hasEnemies())
		{
			return false;
		}
		City target_city = pActor.plot.target_city;
		if (target_city == null)
		{
			return false;
		}
		if (!target_city.isAlive())
		{
			return false;
		}
		if (!target_city.hasKingdom())
		{
			return false;
		}
		if (!target_city.kingdom.isEnemy(pActor.kingdom))
		{
			return false;
		}
		return true;
	}

	private static bool summonCloudAction(Actor pActor, string pCloud, int pMin, int pMax, float pLifespanMin, float pLifespanMax)
	{
		City target_city = pActor.plot.target_city;
		int num = MapBox.width;
		int num2 = MapBox.height;
		int num3 = 0;
		foreach (TileZone zone in target_city.zones)
		{
			if (zone.centerTile.x < num)
			{
				num = zone.centerTile.x;
			}
			if (zone.centerTile.y < num2)
			{
				num2 = zone.centerTile.y;
			}
			if (zone.centerTile.y > num3)
			{
				num3 = zone.centerTile.y;
			}
		}
		int num4 = Randy.randomInt(pMin, pMax + 1);
		int num5 = (num3 - num2) / num4;
		for (int i = 0; i < num4; i++)
		{
			int num6 = Randy.randomInt(-7, 8);
			int pX = Mathf.Clamp(num + num6, 0, MapBox.width - 1);
			int pY = num2 + num5 * i;
			WorldTile tileSimple = MapBox.instance.GetTileSimple(pX, pY);
			((Cloud)EffectsLibrary.spawn("fx_cloud", tileSimple, pCloud)).setLifespan(Randy.randomFloat(pLifespanMin, pLifespanMax));
		}
		return true;
	}

	private static bool summonUnitsAction(Actor pActor, string pActorAssetId, int pMin, int pMax, bool pJoinCasters)
	{
		City target_city = pActor.plot.target_city;
		if (target_city == null)
		{
			return false;
		}
		int num = Randy.randomInt(pMin, pMax + 1);
		for (int i = 0; i < num; i++)
		{
			WorldTile randomTile = target_city.zones.GetRandom().getRandomTile();
			Actor actor = World.world.units.createNewUnit(pActorAssetId, randomTile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: true, pAdultAge: true, pGiveOwnerlessItems: false, pJoinCasters);
			EffectsLibrary.spawn("fx_spawn", randomTile);
			if (pJoinCasters && actor != null && pActor.isKingdomCiv())
			{
				if (actor.subspecies.isJustCreated())
				{
					actor.subspecies.addTrait("prefrontal_cortex");
				}
				actor.joinKingdom(pActor.kingdom);
			}
		}
		return true;
	}

	private static bool afterRiteWorldTransform(Actor pActor)
	{
		if (WorldLawLibrary.world_law_gaias_covenant.isEnabled())
		{
			return false;
		}
		Religion religion = pActor.religion;
		if (religion == null)
		{
			return false;
		}
		using ListPool<ReligionTrait> listPool = new ListPool<ReligionTrait>();
		foreach (ReligionTrait trait in religion.getTraits())
		{
			if (!(trait.group_id != "transformation"))
			{
				listPool.Add(trait);
			}
		}
		if (listPool.Count == 0)
		{
			return false;
		}
		ReligionTrait random = listPool.GetRandom();
		TileTypeBase tileHigh = AssetManager.biome_library.get(random.transformation_biome_id).getTileHigh();
		WorldTile randomTile = World.world.islands_calculator.getRandomIslandGround().getRandomTile();
		WorldBehaviourActionBiomes.trySpreadBiomeAround(randomTile, tileHigh);
		BrushData brushData = Brush.get(2, "special_");
		int num = brushData.pos.Length;
		int num2 = Randy.randomInt((int)((float)num * 0.25f), (int)((float)num * 0.5f));
		using ListPool<BrushPixelData> listPool2 = new ListPool<BrushPixelData>(brushData.pos);
		for (int i = 0; i < num2; i++)
		{
			BrushPixelData random2 = listPool2.GetRandom();
			listPool2.Remove(random2);
			WorldTile tile = MapBox.instance.GetTile(randomTile.x + random2.x, randomTile.y + random2.y);
			if (tile != null)
			{
				WorldBehaviourActionBiomes.trySpreadBiomeAround(tile, tileHigh, pCheckRoad: false, pCheckBonuses: false, pForce: true);
			}
		}
		return true;
	}

	public override void init()
	{
		Debug.Log((object)"Init PlotLibrary");
		base.init();
		addBasic();
		addBasicMetas();
		addMagicRites();
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (PlotAsset item in list)
		{
			if (item.is_basic_plot)
			{
				basic_plots.Add(item);
			}
		}
	}

	public override PlotAsset add(PlotAsset pAsset)
	{
		base.add(pAsset);
		pAsset.get_formatted_description = getDescriptionGeneric;
		return pAsset;
	}

	public override void editorDiagnosticLocales()
	{
		foreach (PlotAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
			checkLocale(item, item.getDescriptionID2());
		}
		base.editorDiagnosticLocales();
	}

	private static string getDescriptionGeneric(Plot pPlot)
	{
		string pString = pPlot.getAsset().getDescriptionID().Localize();
		Actor author = pPlot.getAuthor();
		return tryToReplace(tryToReplace(tryToReplace(tryToReplace(tryToReplace(tryToReplace(pString, "$initiator_actor$", author?.name), "$initiator_kingdom$", author?.kingdom?.name), "$initiator_city$", author?.city?.name), "$target_kingdom$", pPlot.target_kingdom?.name), "$target_alliance$", pPlot.target_alliance?.name), "$target_war$", pPlot.target_war?.name);
	}

	private static string tryToReplace(string pString, string pReplaceID, string pName)
	{
		if (pString.Contains(pReplaceID))
		{
			pString = pString.Replace(pReplaceID, pName);
		}
		return pString;
	}

	public void addBasic()
	{
		PlotAsset obj = new PlotAsset
		{
			id = "rebellion",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_rebellion",
			group_id = "diplomacy",
			min_level = 2,
			money_cost = 15,
			min_stewardship = 6,
			min_warfare = 5,
			can_be_done_by_leader = true,
			check_target_kingdom = true,
			requires_diplomacy = true,
			requires_rebellion = true,
			check_is_possible = delegate(Actor pActor)
			{
				Kingdom kingdom = pActor.kingdom;
				if (pActor.isKing())
				{
					return false;
				}
				if (kingdom.countCities() == 1)
				{
					return false;
				}
				if (!kingdom.hasCapital())
				{
					return false;
				}
				if (kingdom.getAge() <= SimGlobals.m.rebellions_min_age)
				{
					return false;
				}
				if (pActor.hasTrait("pacifist"))
				{
					return false;
				}
				City city = pActor.city;
				if (city.isCapitalCity())
				{
					return false;
				}
				if (city.isHappy())
				{
					return false;
				}
				float num = city.countWarriors();
				float num2 = kingdom.capital.countWarriors();
				if (num < (float)SimGlobals.m.rebellions_min_warriors)
				{
					return false;
				}
				float num3 = (float)kingdom.countUnhappyCities() * SimGlobals.m.rebellions_unhappy_multiplier;
				float num4 = num2 - num2 * num3;
				if (!(num >= num4))
				{
					return false;
				}
				foreach (War war2 in kingdom.getWars())
				{
					if (war2.getAsset() == WarTypeLibrary.rebellion && war2.isMainDefender(kingdom))
					{
						return false;
					}
				}
				int num5 = kingdom.countCities();
				if (num5 <= 6)
				{
					int maxCities = kingdom.getMaxCities();
					if (num5 <= maxCities)
					{
						return false;
					}
				}
				foreach (Plot plot2 in World.world.plots)
				{
					if (plot2.isActive() && plot2.isSameType(rebellion) && plot2.target_kingdom == kingdom)
					{
						return false;
					}
				}
				return true;
			},
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				Plot plot = World.world.plots.newPlot(pActor, pPlotAsset, pForced);
				plot.target_kingdom = pActor.kingdom;
				if (!plot.checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotRebellion is missing start requirements");
					return true;
				}
				return true;
			},
			check_other_plots = (Actor pActor, Plot pPlot) => (pActor.kingdom == pPlot.target_kingdom) ? true : false,
			check_can_be_forced = delegate(Actor pActor)
			{
				if (pActor.isKing())
				{
					return false;
				}
				Kingdom kingdom = pActor.kingdom;
				if (pActor.city.isCapitalCity())
				{
					return false;
				}
				return kingdom.countCities() != 1;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isCityLeader())
				{
					return false;
				}
				if (pActor.isKing())
				{
					return false;
				}
				return !pActor.city.isCapitalCity();
			},
			action = delegate(Actor pActor)
			{
				bool pCheckForHappiness = !pActor.plot.data.forced;
				DiplomacyHelpersRebellion.startRebellion(pActor, pActor.plot, pCheckForHappiness);
				return true;
			}
		};
		PlotAsset pAsset = obj;
		t = obj;
		rebellion = add(pAsset);
		add(t = new PlotAsset
		{
			id = "new_war",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_new_war",
			group_id = "diplomacy",
			min_level = 3,
			min_warfare = 6,
			money_cost = 20,
			min_renown_kingdom = 50,
			can_be_done_by_king = true,
			check_target_kingdom = true,
			requires_diplomacy = true,
			check_is_possible = delegate(Actor pActor)
			{
				Kingdom kingdom = pActor.kingdom;
				if (!DiplomacyHelpers.isWarNeeded(kingdom))
				{
					return false;
				}
				if (pActor.hasCulture() && pActor.culture.hasTrait("serenity_now"))
				{
					return false;
				}
				if (pActor.hasTrait("pacifist"))
				{
					return false;
				}
				if (kingdom.hasAlliance())
				{
					foreach (Kingdom item in kingdom.getAlliance().kingdoms_hashset)
					{
						if (item != kingdom && item.hasKing())
						{
							Actor king = item.king;
							if (king.hasPlot() && king.plot.isSameType(new_war))
							{
								return false;
							}
						}
					}
				}
				return true;
			},
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				Kingdom warTarget = DiplomacyHelpers.getWarTarget(pActor.kingdom);
				if (warTarget == null)
				{
					return false;
				}
				Plot plot = World.world.plots.newPlot(pActor, pPlotAsset, pForced);
				plot.target_kingdom = warTarget;
				if (!plot.checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotWar is missing start requirements");
					return true;
				}
				return true;
			},
			check_should_continue = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				if (!plot.target_kingdom.isAlive())
				{
					return false;
				}
				if (pActor.kingdom.hasAlliance() && pActor.kingdom.getAlliance() == plot.target_kingdom.getAlliance())
				{
					return false;
				}
				return DiplomacyHelpers.isWarNeeded(pActor.kingdom) ? true : false;
			},
			action = delegate(Actor pActor)
			{
				World.world.diplomacy.startWar(pActor.kingdom, pActor.plot.target_kingdom, WarTypeLibrary.normal);
				return true;
			}
		});
		alliance_create = add(t = new PlotAsset
		{
			id = "alliance_create",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_alliance_create",
			group_id = "diplomacy",
			min_level = 5,
			money_cost = 20,
			min_renown_kingdom = 60,
			can_be_done_by_king = true,
			requires_diplomacy = true,
			check_is_possible = delegate(Actor pActor)
			{
				Kingdom kingdom = pActor.kingdom;
				if (kingdom.hasAlliance())
				{
					return false;
				}
				if (kingdom.hasEnemies())
				{
					return false;
				}
				if (kingdom.isSupreme())
				{
					return false;
				}
				if (Date.getYearsSince(kingdom.data.timestamp_alliance) < SimGlobals.m.alliance_timeout)
				{
					return false;
				}
				return !World.world.plots.isPlotTypeAlreadyRunning(pActor, alliance_create);
			},
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				if (DiplomacyHelpers.getAllianceTarget(pActor.kingdom) == null)
				{
					return false;
				}
				if (!World.world.plots.newPlot(pActor, pPlotAsset, pForced).checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotNewAlliance is missing start requirements");
					return true;
				}
				return true;
			},
			check_other_plots = delegate(Actor pActor, Plot pPlot)
			{
				Kingdom kingdom = pActor.kingdom;
				foreach (Actor unit in pPlot.units)
				{
					if (DiplomacyHelpers.areKingdomsClose(unit.kingdom, kingdom))
					{
						return true;
					}
				}
				return false;
			},
			check_can_be_forced = (Actor pActor) => !pActor.kingdom.hasAlliance(),
			check_should_continue = delegate(Actor pActor)
			{
				foreach (Actor unit2 in pActor.plot.units)
				{
					Kingdom kingdom = unit2.kingdom;
					if (!kingdom.isAlive())
					{
						return false;
					}
					if (kingdom.hasEnemies())
					{
						return false;
					}
					if (kingdom != pActor.kingdom && !kingdom.isOpinionTowardsKingdomGood(pActor.kingdom))
					{
						return false;
					}
				}
				return true;
			},
			action = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				Kingdom allianceTarget = DiplomacyHelpers.getAllianceTarget(pActor.kingdom);
				if (allianceTarget == null)
				{
					return false;
				}
				using ListPool<Kingdom> listPool = new ListPool<Kingdom>(World.world.kingdoms.Count);
				listPool.Add(pActor.kingdom);
				listPool.Add(allianceTarget);
				Kingdom kingdom = listPool.Shift();
				Kingdom kingdom2 = listPool.Shift();
				if (!kingdom.isOpinionTowardsKingdomGood(kingdom2))
				{
					return false;
				}
				Alliance alliance = World.world.alliances.newAlliance(kingdom, kingdom2);
				if (kingdom.hasKing())
				{
					kingdom.king.leavePlot();
				}
				if (kingdom2.hasKing())
				{
					kingdom2.king.leavePlot();
				}
				foreach (ref Kingdom item2 in listPool)
				{
					Kingdom current = item2;
					alliance.join(current, pRecalc: false);
				}
				foreach (Actor unit3 in plot.units)
				{
					unit3.leavePlot();
				}
				alliance.recalculate();
				return true;
			}
		});
		add(t = new PlotAsset
		{
			id = "alliance_join",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_alliance_create",
			group_id = "diplomacy",
			min_level = 2,
			money_cost = 5,
			min_diplomacy = 5,
			min_renown_kingdom = 50,
			can_be_done_by_king = true,
			check_target_alliance = true,
			requires_diplomacy = true,
			check_is_possible = delegate(Actor pActor)
			{
				Kingdom kingdom = pActor.kingdom;
				if (kingdom.isSupreme())
				{
					return false;
				}
				if (kingdom.hasAlliance())
				{
					return false;
				}
				if (kingdom.hasEnemies())
				{
					return false;
				}
				if (Date.getYearsSince(kingdom.data.timestamp_alliance) < SimGlobals.m.alliance_timeout)
				{
					return false;
				}
				return World.world.alliances.anyAlliances() ? true : false;
			},
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				Kingdom kingdom = pActor.kingdom;
				_ = kingdom.power;
				Alliance alliance = null;
				foreach (Alliance item3 in World.world.alliances.list.LoopRandom())
				{
					if (!item3.hasWars())
					{
						if (pForced)
						{
							alliance = item3;
							break;
						}
						if (item3.canJoin(kingdom) && !item3.hasSupremeKingdom())
						{
							_ = item3.power;
							bool flag = false;
							if (kingdom.countCities() <= 2 && !kingdom.hasNearbyKingdoms())
							{
								flag = true;
							}
							if (!flag && item3.hasSharedBordersWithKingdom(kingdom))
							{
								flag = true;
							}
							if (flag)
							{
								alliance = item3;
							}
						}
					}
				}
				if (alliance == null)
				{
					return false;
				}
				Plot plot = World.world.plots.newPlot(pActor, pPlotAsset, pForced);
				plot.target_alliance = alliance;
				if (!plot.checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotJoinAlliance is missing start requirements");
					return true;
				}
				pActor.setPlot(plot);
				return true;
			},
			check_other_plots = alliance_create.check_other_plots,
			check_can_be_forced = (Actor pActor) => !pActor.kingdom.hasAlliance(),
			check_should_continue = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				if (pActor.kingdom.hasAlliance())
				{
					return false;
				}
				if (!plot.target_alliance.isAlive())
				{
					return false;
				}
				if (!plot.target_alliance.canJoin(pActor.kingdom))
				{
					return false;
				}
				if (pActor.kingdom.hasEnemies())
				{
					return false;
				}
				return !plot.target_alliance.hasWars();
			},
			action = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				if (pActor.kingdom.hasAlliance())
				{
					return false;
				}
				if (!plot.target_alliance.isAlive())
				{
					return false;
				}
				plot.target_alliance.join(pActor.kingdom);
				if (pActor.kingdom.hasKing())
				{
					pActor.kingdom.king.leavePlot();
				}
				return true;
			}
		});
		add(t = new PlotAsset
		{
			id = "alliance_destroy",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_alliance_destroy",
			group_id = "diplomacy",
			min_level = 5,
			money_cost = 15,
			can_be_done_by_king = true,
			check_target_alliance = true,
			requires_diplomacy = true,
			check_is_possible = delegate(Actor pActor)
			{
				Kingdom kingdom = pActor.kingdom;
				if (!kingdom.hasAlliance())
				{
					return false;
				}
				if (kingdom.hasEnemies())
				{
					return false;
				}
				Alliance alliance = kingdom.getAlliance();
				if (alliance.isForcedType())
				{
					return false;
				}
				if ((float)Date.getYearsSince(alliance.data.timestamp_member_joined) < 10f)
				{
					return false;
				}
				if (!kingdom.isSupreme() && !kingdom.isSecondBest())
				{
					return false;
				}
				if (alliance.getAge() < 10)
				{
					return false;
				}
				float num = alliance.power;
				float num2 = 0f;
				foreach (Kingdom kingdom3 in World.world.kingdoms)
				{
					if (kingdom3.getAlliance() != alliance)
					{
						num2 += (float)kingdom3.power;
					}
				}
				if (num < num2 * 2f)
				{
					return false;
				}
				foreach (Kingdom item4 in alliance.kingdoms_hashset)
				{
					if (item4.isSupreme())
					{
						return true;
					}
				}
				return false;
			},
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				Kingdom kingdom = pActor.kingdom;
				if (!kingdom.hasAlliance())
				{
					return false;
				}
				Plot plot = World.world.plots.newPlot(pActor, pPlotAsset, pForced);
				plot.target_alliance = kingdom.getAlliance();
				if (!plot.checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotDisbandAlliance is missing start requirements");
					return true;
				}
				return true;
			},
			check_should_continue = (Actor pActor) => pActor.kingdom.hasAlliance() ? true : false,
			action = delegate(Actor pActor)
			{
				if (!pActor.kingdom.hasAlliance())
				{
					return false;
				}
				Alliance alliance = pActor.kingdom.getAlliance();
				World.world.alliances.dissolveAlliance(alliance);
				return true;
			}
		});
		t.check_can_be_forced = t.check_should_continue;
		add(t = new PlotAsset
		{
			id = "attacker_stop_war",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_stop_war",
			group_id = "diplomacy",
			min_level = 4,
			money_cost = 20,
			min_diplomacy = 8,
			can_be_done_by_king = true,
			check_target_war = true,
			requires_diplomacy = true,
			check_is_possible = (Actor pActor) => pActor.kingdom.hasEnemies() ? true : false,
			try_to_start_advanced = delegate(Actor pActor, PlotAsset pPlotAsset, bool pForced)
			{
				Kingdom kingdom = pActor.kingdom;
				War war = null;
				foreach (War war3 in kingdom.getWars(pRandom: true))
				{
					if (war3.getAge() > SimGlobals.m.minimum_age_before_war_stop && war3.getAsset().can_end_with_plot && war3.isAttacker(kingdom))
					{
						int num = war3.countAttackersWarriors();
						int num2 = war3.countDefendersWarriors();
						if (!war3.areDefendersGettingCaptured() && !war3.areAttackersAttackingAnotherCity() && !war3.areAttackersGettingCaptured() && !war3.areDefendersAttackingAnotherCity() && (num <= 100 || num2 <= 100))
						{
							if (war3.isAttacker(kingdom))
							{
								if (num < num2 * 2)
								{
									continue;
								}
							}
							else if (num2 < num * 2)
							{
								continue;
							}
							war = war3;
							break;
						}
					}
				}
				if (war == null)
				{
					return false;
				}
				foreach (Plot plot3 in World.world.plots)
				{
					if (plot3.isActive() && plot3.isSameType(pPlotAsset) && plot3.target_war == war)
					{
						pActor.setPlot(plot3);
						return true;
					}
				}
				Plot plot = World.world.plots.newPlot(pActor, pPlotAsset, pForced);
				plot.target_war = war;
				if (!plot.checkInitiatorAndTargets())
				{
					Debug.Log((object)"tryPlotStopWar is missing start requirements");
					return true;
				}
				return true;
			},
			check_can_be_forced = (Actor pActor) => pActor.kingdom.hasEnemies() ? true : false,
			check_should_continue = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				War target_war = plot.target_war;
				if (target_war == null || target_war.isRekt())
				{
					return false;
				}
				return !plot.target_war.hasEnded();
			},
			action = delegate(Actor pActor)
			{
				Plot plot = pActor.plot;
				if (plot.target_war.hasEnded())
				{
					return false;
				}
				World.world.wars.endWar(plot.target_war, WarWinner.Peace);
				return true;
			}
		});
		add(t = new PlotAsset
		{
			id = "new_book",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_new_book",
			group_id = "plots_others",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			can_be_done_by_clan_member = true,
			progress_needed = 200f,
			min_level = 10,
			money_cost = 200,
			min_intelligence = 3,
			pot_rate = 5,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				return pActor.city.hasBookSlots() ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				return pActor.city.hasBookSlots() ? true : false;
			},
			action = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.city.hasBookSlots())
				{
					return false;
				}
				World.world.books.generateNewBook(pActor);
				return true;
			}
		});
		t.check_can_be_forced = t.check_is_possible;
	}

	public void addBasicMetas()
	{
		PlotAsset obj = new PlotAsset
		{
			id = "new_language",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_new_language",
			group_id = "language",
			priority = 99,
			min_level = 1,
			money_cost = 0,
			min_intelligence = 3,
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_is_possible = delegate(Actor pActor)
			{
				if (pActor.hasLanguage())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_communication ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (pActor.hasLanguage())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_communication ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Language language = World.world.languages.newLanguage(pActor, pAddDefaultTraits: true);
				pActor.joinLanguage(language);
				language.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		};
		PlotAsset pAsset = obj;
		t = obj;
		add(pAsset);
		t.check_can_be_forced = (Actor pActor) => pActor.subspecies.has_advanced_communication ? true : false;
		add(t = new PlotAsset
		{
			id = "new_religion",
			path_icon = "plots/icons/plot_new_religion",
			group_id = "religion",
			priority = 99,
			min_level = 5,
			money_cost = 25,
			min_intelligence = 8,
			is_basic_plot = true,
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_is_possible = delegate(Actor pActor)
			{
				if (pActor.hasReligion())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				if (!pActor.subspecies.has_advanced_memory)
				{
					return false;
				}
				return pActor.culture.countUnits() >= 50;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (pActor.hasReligion())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_memory ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Religion religion = World.world.religions.newReligion(pActor, pAddDefaultTraits: true);
				pActor.setReligion(religion);
				religion.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		});
		t.check_can_be_forced = (Actor pActor) => pActor.subspecies.has_advanced_memory ? true : false;
		add(t = new PlotAsset
		{
			id = "new_culture",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_new_culture",
			group_id = "culture",
			priority = 99,
			min_level = 1,
			money_cost = 0,
			min_intelligence = 2,
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_is_possible = delegate(Actor pActor)
			{
				if (pActor.hasCulture())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_memory ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (pActor.hasCulture())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_memory ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Culture culture = World.world.cultures.newCulture(pActor, pAddDefaultTraits: true);
				pActor.setCulture(culture);
				culture.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		});
		t.check_can_be_forced = (Actor pActor) => pActor.subspecies.has_advanced_memory ? true : false;
		add(t = new PlotAsset
		{
			id = "clan_ascension",
			path_icon = "plots/icons/plot_clan_ascension",
			can_be_done_by_king = true,
			group_id = "rites_various",
			min_level = 7,
			money_cost = 100,
			min_intelligence = 5,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasReligion())
				{
					return false;
				}
				if (!pActor.hasClan())
				{
					return false;
				}
				if (pActor.clan.hasTrait("mark_of_becoming"))
				{
					return false;
				}
				if (!pActor.clan.hasChief())
				{
					return false;
				}
				if (pActor.clan.getChief() != pActor)
				{
					return false;
				}
				if (pActor.kingdom.getPopulationPeople() < 500)
				{
					return false;
				}
				if (!pActor.hasTrait("evil"))
				{
					return false;
				}
				return !pActor.hasSubspeciesTrait("pure");
			},
			check_should_continue = (Actor pActor) => pActor.hasClan() ? true : false,
			action = delegate(Actor pActor)
			{
				Clan clan = pActor.clan;
				if (!ActionLibrary.tryToEvolveUnitViaAscension(pActor, out var pEvolvedActorForm))
				{
					return false;
				}
				clan.addTrait("mark_of_becoming");
				Subspecies subspecies = pEvolvedActorForm.subspecies;
				if (clan != null)
				{
					foreach (Actor unit in clan.units)
					{
						unit.setSubspecies(subspecies);
					}
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = (Actor pActor) => !pActor.hasSubspeciesTrait("pure");
		add(t = new PlotAsset
		{
			id = "culture_divide",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_culture_divide",
			group_id = "culture",
			can_be_done_by_leader = true,
			min_level = 5,
			money_cost = 20,
			min_intelligence = 6,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				if (pActor.isOneCityKingdom())
				{
					return false;
				}
				Culture culture = pActor.culture;
				if (culture.countUnits() < SimGlobals.m.people_before_meta_divide)
				{
					return false;
				}
				if (culture.getAge() < SimGlobals.m.years_before_meta_divide)
				{
					return false;
				}
				if (culture.hasTrait("legacy_keepers"))
				{
					return false;
				}
				if (!pActor.wantsToSplitMeta())
				{
					return false;
				}
				if (culture != pActor.kingdom.culture)
				{
					return false;
				}
				if (!pActor.subspecies.has_advanced_memory)
				{
					return false;
				}
				if (culture.data.creator_kingdom_id == pActor.kingdom.id)
				{
					return false;
				}
				return (culture.data.creator_clan_id != pActor.clan.id) ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (!pActor.hasCulture())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_memory ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Culture culture = pActor.culture;
				Culture culture2 = World.world.cultures.newCulture(pActor, pAddDefaultTraits: false);
				culture2.cloneAndEvolveOnomastics(culture);
				culture2.copyTraits(culture.getTraits());
				pActor.setCulture(culture2);
				culture2.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		});
		t.check_can_be_forced = delegate(Actor pActor)
		{
			if (!pActor.hasCity())
			{
				return false;
			}
			if (!pActor.hasCulture())
			{
				return false;
			}
			if (!pActor.hasLanguage())
			{
				return false;
			}
			return pActor.subspecies.has_advanced_memory ? true : false;
		};
		add(t = new PlotAsset
		{
			id = "religion_schism",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_religion_schism",
			can_be_done_by_leader = true,
			group_id = "religion",
			min_level = 5,
			money_cost = 15,
			min_intelligence = 6,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				if (!pActor.hasReligion())
				{
					return false;
				}
				if (pActor.isOneCityKingdom())
				{
					return false;
				}
				if (!pActor.wantsToSplitMeta())
				{
					return false;
				}
				if (pActor.culture.hasTrait("legacy_keepers"))
				{
					return false;
				}
				Religion religion = pActor.religion;
				if (religion.countUnits() < SimGlobals.m.people_before_meta_divide)
				{
					return false;
				}
				if (religion.getAge() < SimGlobals.m.years_before_meta_divide)
				{
					return false;
				}
				if (religion != pActor.kingdom.religion)
				{
					return false;
				}
				if (!pActor.subspecies.has_advanced_memory)
				{
					return false;
				}
				if (religion.data.creator_kingdom_id == pActor.kingdom.id)
				{
					return false;
				}
				return (religion.data.creator_clan_id != pActor.clan.id) ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (!pActor.hasReligion())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_memory ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Religion religion = pActor.religion;
				Religion religion2 = World.world.religions.newReligion(pActor, pAddDefaultTraits: false);
				religion2.copyTraits(religion.getTraits());
				pActor.setReligion(religion2);
				religion2.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		});
		t.check_can_be_forced = delegate(Actor pActor)
		{
			if (!pActor.hasCity())
			{
				return false;
			}
			if (!pActor.hasCulture())
			{
				return false;
			}
			if (!pActor.hasLanguage())
			{
				return false;
			}
			if (!pActor.hasReligion())
			{
				return false;
			}
			if (pActor.religion != pActor.kingdom.religion)
			{
				return false;
			}
			return pActor.subspecies.has_advanced_memory ? true : false;
		};
		add(t = new PlotAsset
		{
			id = "language_divergence",
			is_basic_plot = true,
			path_icon = "plots/icons/plot_language_divergence",
			can_be_done_by_leader = true,
			group_id = "language",
			min_level = 5,
			money_cost = 15,
			min_intelligence = 8,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				if (!pActor.hasReligion())
				{
					return false;
				}
				if (pActor.isOneCityKingdom())
				{
					return false;
				}
				if (!pActor.wantsToSplitMeta())
				{
					return false;
				}
				if (pActor.culture.hasTrait("legacy_keepers"))
				{
					return false;
				}
				Language language = pActor.language;
				if (language.countUnits() < SimGlobals.m.people_before_meta_divide)
				{
					return false;
				}
				if (language.getAge() < SimGlobals.m.years_before_meta_divide)
				{
					return false;
				}
				if (language != pActor.kingdom.language)
				{
					return false;
				}
				if (!pActor.subspecies.has_advanced_communication)
				{
					return false;
				}
				if (language.data.creator_kingdom_id == pActor.kingdom.id)
				{
					return false;
				}
				return (language.data.creator_clan_id != pActor.clan.id) ? true : false;
			},
			check_should_continue = delegate(Actor pActor)
			{
				if (!pActor.hasLanguage())
				{
					return false;
				}
				return pActor.subspecies.has_advanced_communication ? true : false;
			},
			action = delegate(Actor pActor)
			{
				Language language = pActor.language;
				Language language2 = World.world.languages.newLanguage(pActor, pAddDefaultTraits: false);
				language2.copyTraits(language.getTraits());
				pActor.joinLanguage(language2);
				language2.forceConvertSameSpeciesAroundUnit(pActor);
				return true;
			}
		});
		t.check_can_be_forced = delegate(Actor pActor)
		{
			if (!pActor.hasCity())
			{
				return false;
			}
			if (!pActor.hasCulture())
			{
				return false;
			}
			if (!pActor.hasLanguage())
			{
				return false;
			}
			if (!pActor.hasReligion())
			{
				return false;
			}
			return pActor.subspecies.has_advanced_communication ? true : false;
		};
	}

	public void addMagicRites()
	{
		PlotAsset obj = new PlotAsset
		{
			id = "summon_meteor_rain",
			path_icon = "plots/icons/plot_summon_meteor_rain",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 200f,
			min_level = 13,
			money_cost = 1000,
			check_is_possible = (Actor pActor) => pActor.kingdom.hasEnemies() ? true : false,
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				City target_city = pActor.plot.target_city;
				int num = 7;
				int num2 = Randy.randomInt(3, num + 1);
				for (int i = 0; i < num2; i++)
				{
					Meteorite.spawnMeteoriteDisaster(target_city.zones.GetRandom().getRandomTile(), pActor);
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		};
		PlotAsset pAsset = obj;
		t = obj;
		add(pAsset);
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_earthquake",
			path_icon = "plots/icons/plot_summon_earthquake",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 200f,
			min_level = 13,
			money_cost = 600,
			pot_rate = 2,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				Earthquake.startQuake(pActor.plot.target_city.zones.GetRandom().getRandomTile(), EarthquakeType.SmallDisaster);
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_thunderstorm",
			path_icon = "plots/icons/plot_summon_thunderstorm",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 200f,
			min_level = 13,
			money_cost = 500,
			pot_rate = 4,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				City target_city = pActor.plot.target_city;
				if (!target_city.hasZones())
				{
					return false;
				}
				WorldTile randomTile = target_city.zones.GetRandom().getRandomTile();
				MapBox.spawnLightningMedium(randomTile, 0.15f, pActor);
				int num = Randy.randomInt(5, 13);
				for (int i = 0; i < num; i++)
				{
					WorldTile tTargetTileL = Toolbox.getRandomTileWithinDistance(randomTile, 10);
					DelayedActionsManager.addAction(pDelay: Randy.randomFloat(0.1f, 0.75f) * (float)i, pAction: delegate
					{
						MapBox.spawnLightningMedium(tTargetTileL, 0.15f, pActor);
					});
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_stormfront",
			path_icon = "plots/icons/plot_summon_stormfront",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 13,
			money_cost = 200,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = (Actor pActor) => summonCloudAction(pActor, "cloud_lightning", 5, 15, 20f, 60f),
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_hellstorm",
			path_icon = "plots/icons/plot_summon_hellstorm",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 13,
			money_cost = 300,
			pot_rate = 4,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = (Actor pActor) => summonCloudAction(pActor, "cloud_fire", 3, 5, 20f, 60f),
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_demons",
			path_icon = "plots/icons/plot_summon_demons",
			group_id = "rites_summoning",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 13,
			money_cost = 300,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				if (Randy.randomChance(0.0666f))
				{
					pActor.plot.target_city = pActor.city;
				}
				return summonUnitsAction(pActor, "demon", 5, 10, pJoinCasters: false);
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_angles",
			path_icon = "plots/icons/plot_summon_angles",
			group_id = "rites_summoning",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 12,
			money_cost = 200,
			pot_rate = 2,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = (Actor pActor) => summonUnitsAction(pActor, "angle", 3, 7, pJoinCasters: true),
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_skeletons",
			path_icon = "plots/icons/plot_summon_skeletons",
			group_id = "rites_summoning",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 13,
			money_cost = 150,
			pot_rate = 2,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = (Actor pActor) => summonUnitsAction(pActor, "skeleton", 6, 13, pJoinCasters: true),
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "summon_living_plants",
			path_icon = "plots/icons/plot_summon_living_plants",
			group_id = "rites_summoning",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 100f,
			min_level = 13,
			money_cost = 400,
			pot_rate = 2,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				if (!pActor.kingdom.hasEnemies())
				{
					return false;
				}
				return AssetManager.actor_library.get("living_plants").units.Count <= 100;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				foreach (WorldTile calculated_farm_field in pActor.plot.target_city.calculated_farm_fields)
				{
					if (calculated_farm_field.hasBuilding())
					{
						ActionLibrary.tryToMakeFloraAlive(calculated_farm_field.building, pFullyGrownOnly: false);
					}
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "big_cast_coffee",
			path_icon = "plots/icons/plot_big_cast_coffee",
			group_id = "rites_merciful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 60f,
			min_level = 9,
			money_cost = 100,
			pot_rate = 2,
			check_is_possible = (Actor pActor) => pActor.hasCity(),
			try_to_start_advanced = tryStartReligionRiteOnSelfCity,
			check_should_continue = (Actor pActor) => pActor.hasCity(),
			action = delegate(Actor pActor)
			{
				foreach (Actor unit in pActor.plot.target_city.getUnits())
				{
					unit.addStatusEffect("caffeinated");
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "big_cast_bubble_shield",
			path_icon = "plots/icons/plot_big_cast_bubble_shield",
			group_id = "rites_merciful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_kingdom = true,
			progress_needed = 50f,
			min_level = 11,
			money_cost = 200,
			pot_rate = 3,
			check_is_possible = (Actor pActor) => pActor.hasKingdom(),
			try_to_start_advanced = tryStartReligionRiteOnSelfKingdom,
			check_should_continue = (Actor pActor) => pActor.hasKingdom(),
			action = delegate(Actor pActor)
			{
				foreach (Actor unit2 in pActor.plot.target_kingdom.getUnits())
				{
					if (unit2.isProfession(UnitProfession.Warrior))
					{
						unit2.addStatusEffect("shield");
					}
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "big_cast_madness",
			path_icon = "plots/icons/plot_big_cast_madness",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			progress_needed = 120f,
			min_level = 11,
			money_cost = 600,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				City target_city = pActor.plot.target_city;
				float pVal = 0.8f;
				foreach (Actor unit3 in target_city.getUnits())
				{
					if (Randy.randomChance(pVal))
					{
						unit3.addTrait("madness");
					}
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "big_cast_slowness",
			path_icon = "plots/icons/plot_big_cast_slowness",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			pot_rate = 3,
			progress_needed = 70f,
			min_level = 11,
			money_cost = 150,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartReligionRiteOnEnemyCity,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				foreach (Actor unit4 in pActor.plot.target_city.getUnits())
				{
					if (unit4.isProfession(UnitProfession.Warrior))
					{
						unit4.addStatusEffect("slowness");
					}
				}
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
		add(t = new PlotAsset
		{
			id = "cause_rebellion",
			path_icon = "plots/icons/plot_cause_rebellion",
			group_id = "rites_wrathful",
			can_be_done_by_king = true,
			can_be_done_by_leader = true,
			check_target_city = true,
			pot_rate = 2,
			progress_needed = 70f,
			min_level = 12,
			money_cost = 400,
			check_is_possible = delegate(Actor pActor)
			{
				if (!pActor.hasKingdom())
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			try_to_start_advanced = tryStartCauseRebellion,
			check_should_continue = checkShouldContinueReligionRiteOnEnemyCity,
			action = delegate(Actor pActor)
			{
				City target_city = pActor.plot.target_city;
				if (target_city.isCapitalCity())
				{
					return false;
				}
				if (target_city.units.Count == 0)
				{
					return false;
				}
				Actor random = target_city.getUnits().GetRandom();
				if (!random.isAlive())
				{
					return false;
				}
				DiplomacyHelpersRebellion.startRebellion(random, pActor.plot, pCheckForHappiness: false);
				return true;
			},
			post_action = afterRiteWorldTransform
		});
		t.check_can_be_forced = t.check_is_possible;
	}
}
