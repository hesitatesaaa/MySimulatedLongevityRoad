using UnityEngine;

public class WorldLog
{
	public static WorldLog instance;

	public const int WORLDLOG_LIMIT = 2000;

	public WorldLog()
	{
		instance = this;
	}

	public static void logNewKing(Kingdom pKingdom)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.king_new, pKingdom.name, pKingdom.king.getName())
		{
			kingdom = pKingdom,
			unit = pKingdom.king,
			location = pKingdom.king.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logRoyalClanNew(Kingdom pKingdom, Clan pClan)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_royal_clan_new, pKingdom.name, pClan.name, pKingdom.king.name)
		{
			kingdom = pKingdom,
			unit = pKingdom.king,
			location = pKingdom.king.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pClan.getColor().getColorText(),
			color_special3 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logRoyalClanChanged(Kingdom pKingdom, Clan pOldClan, Clan pNewClan)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_royal_clan_changed, pKingdom.name, pOldClan.name, pNewClan.name)
		{
			kingdom = pKingdom,
			unit = pKingdom.king,
			location = pKingdom.king.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pOldClan.getColor().getColorText(),
			color_special3 = pNewClan.getColor().getColorText()
		}.add();
	}

	public static void logRoyalClanNoMore(Kingdom pKingdom, Clan pClan)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		WorldLogMessage worldLogMessage = new WorldLogMessage(WorldLogLibrary.kingdom_royal_clan_dead, pKingdom.name, pClan.name);
		worldLogMessage.kingdom = pKingdom;
		if (pKingdom.hasCapital())
		{
			worldLogMessage.location = pKingdom.capital.last_city_center;
		}
		worldLogMessage.color_special1 = pKingdom.getColor().getColorText();
		worldLogMessage.color_special2 = pClan.getColor().getColorText();
		worldLogMessage.add();
	}

	public static void logKingFledCity(Kingdom pKingdom, Actor pActor)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.king_fled_city, pKingdom.name, pActor.getName(), pActor.city.name)
		{
			kingdom = pKingdom,
			unit = pActor,
			location = pActor.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pKingdom.getColor().getColorText(),
			color_special3 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logKingFledCapital(Kingdom pKingdom, Actor pActor)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.king_fled_capital, pKingdom.name, pActor.getName(), pKingdom.capital.name)
		{
			kingdom = pKingdom,
			unit = pActor,
			location = pActor.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pKingdom.getColor().getColorText(),
			color_special3 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logKingLeft(Kingdom pKingdom, Actor pActor)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.king_left, pKingdom.name, pActor.getName())
		{
			kingdom = pKingdom,
			unit = pActor,
			location = pActor.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logKingDead(Kingdom pKingdom, Actor pActor)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.king_dead, pKingdom.name, pActor.getName())
		{
			kingdom = pKingdom,
			location = pActor.current_position,
			color_special1 = pKingdom.getColor().getColorText(),
			color_special2 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logKingMurder(Kingdom pKingdom, Actor pActor, Actor pAttacker)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		WorldLogMessage worldLogMessage = new WorldLogMessage(WorldLogLibrary.king_killed, pKingdom.name, pActor.getName(), pAttacker?.getName());
		worldLogMessage.kingdom = pKingdom;
		worldLogMessage.color_special1 = pKingdom.getColor().getColorText();
		worldLogMessage.color_special2 = pKingdom.getColor().getColorText();
		if (pAttacker?.kingdom?.getColor() != null)
		{
			worldLogMessage.color_special3 = pAttacker.kingdom.getColor().getColorText();
		}
		if (pAttacker != null && pAttacker.asset.can_be_inspected)
		{
			worldLogMessage.unit = pAttacker;
		}
		worldLogMessage.location = pActor.current_position;
		worldLogMessage.add();
	}

	public static void logFavDead(Actor pActor)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		WorldLogMessage worldLogMessage = new WorldLogMessage(WorldLogLibrary.favorite_dead, pActor.getName());
		worldLogMessage.location = pActor.current_position;
		if (pActor?.kingdom?.getColor() != null)
		{
			worldLogMessage.kingdom = pActor.kingdom;
			worldLogMessage.color_special1 = pActor.kingdom.getColor().getColorText();
		}
		worldLogMessage.add();
	}

	public static void logFavMurder(Actor pActor, Actor pAttacker)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		WorldLogMessage worldLogMessage = new WorldLogMessage(WorldLogLibrary.favorite_killed, pActor.getName(), pAttacker?.getName());
		if (pActor?.kingdom?.getColor() != null)
		{
			worldLogMessage.kingdom = pActor.kingdom;
			worldLogMessage.color_special1 = pActor.kingdom.getColor().getColorText();
		}
		if (pAttacker?.kingdom?.getColor() != null)
		{
			worldLogMessage.color_special2 = pAttacker.kingdom.getColor().getColorText();
		}
		if (pAttacker != null && pAttacker.asset.can_be_inspected)
		{
			worldLogMessage.unit = pAttacker;
		}
		worldLogMessage.location = pActor.current_position;
		worldLogMessage.add();
	}

	public static void logNewCity(City pCity)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.city_new, pCity.name)
		{
			kingdom = pCity.kingdom,
			location = pCity.last_city_center,
			color_special1 = pCity.kingdom.getColor().getColorText()
		}.add();
	}

	public static void logCityRevolt(City pCity)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.log_city_revolted, pCity.name, pCity.kingdom.name)
		{
			kingdom = pCity.kingdom,
			location = pCity.last_city_center
		}.add();
	}

	public static void logWarEnded(War pWar)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.diplomacy_war_ended, pWar.data.name)
		{
			color_special1 = pWar.getColor().getColorText()
		}.add();
	}

	public static void logNewWar(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.diplomacy_war_started, pKingdom1.name, pKingdom2.name)
		{
			location = Vector2.op_Implicit(pKingdom1.location),
			color_special1 = pKingdom1.getColor().getColorText(),
			color_special2 = pKingdom2.getColor().getColorText()
		}.add();
	}

	public static void logNewTotalWar(Kingdom pKingdom)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.total_war_started, pKingdom.name)
		{
			location = Vector2.op_Implicit(pKingdom.location),
			color_special1 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logAllianceCreated(Alliance pAlliance)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.alliance_new, pAlliance.name)
		{
			color_special1 = pAlliance.getColor().getColorText()
		}.add();
	}

	public static void logAllianceDisolved(Alliance pAlliance)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.alliance_dissolved, pAlliance.name)
		{
			color_special1 = pAlliance.getColor().getColorText()
		}.add();
	}

	public static void logNewKingdom(Kingdom pKingdom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_new, pKingdom.name)
		{
			kingdom = pKingdom,
			location = Vector2.op_Implicit(pKingdom.location),
			color_special1 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logKingdomDestroyed(Kingdom pKingdom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_destroyed, pKingdom.name)
		{
			kingdom = pKingdom,
			location = Vector2.op_Implicit(pKingdom.location),
			color_special1 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logCityDestroyed(City pCity)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.city_destroyed, pCity.name)
		{
			kingdom = pCity.kingdom,
			color_special1 = pCity.kingdom.getColor().getColorText(),
			location = pCity.last_city_center
		}.add();
	}

	public static void logDisaster(DisasterAsset pAsset, WorldTile pTile, string pName = null, City pCity = null, Actor pUnit = null)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(pAsset.world_log))
		{
			WorldLogMessage worldLogMessage = new WorldLogMessage(AssetManager.world_log_library.get(pAsset.world_log));
			worldLogMessage.location = Vector2.op_Implicit(pTile.posV3);
			worldLogMessage.special1 = pName;
			if (pCity != null)
			{
				worldLogMessage.special2 = pCity.name;
			}
			if (pUnit != null && pUnit.asset.can_be_inspected)
			{
				worldLogMessage.unit = pUnit;
			}
			worldLogMessage.add();
		}
	}

	public static void locationJump(Vector3 pVector)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		HistoryHud.disableRaycasts();
		World.world.locatePosition(pVector, HistoryHud.enableRaycasts, HistoryHud.enableRaycasts);
	}

	public static void locationFollow(Actor pActor)
	{
		if (pActor != null && pActor.isAlive())
		{
			HistoryHud.disableRaycasts();
			World.world.locateAndFollow(pActor, HistoryHud.enableRaycasts, HistoryHud.enableRaycasts);
		}
	}

	public static void logShatteredCrown(Kingdom pKingdom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_shattered, pKingdom.name)
		{
			kingdom = pKingdom,
			location = Vector2.op_Implicit(pKingdom.location),
			color_special1 = pKingdom.getColor().getColorText()
		}.add();
	}

	public static void logFracturedKingdom(Kingdom pKingdom)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		new WorldLogMessage(WorldLogLibrary.kingdom_fractured, pKingdom.name)
		{
			kingdom = pKingdom,
			location = Vector2.op_Implicit(pKingdom.location),
			color_special1 = pKingdom.getColor().getColorText()
		}.add();
	}
}
