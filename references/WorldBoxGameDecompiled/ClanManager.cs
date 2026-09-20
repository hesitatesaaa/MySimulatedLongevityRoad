using System.Collections.Generic;

public class ClanManager : MetaSystemManager<Clan, ClanData>
{
	public ClanManager()
	{
		type_id = "clan";
	}

	public Clan newClan(Actor pFounder, bool pAddDefaultTraits)
	{
		World.world.game_stats.data.clansCreated++;
		World.world.map_stats.clansCreated++;
		Clan clan = newObject();
		clan.newClan(pFounder, pAddDefaultTraits);
		MetaHelper.addRandomTrait(clan, AssetManager.clan_traits);
		pFounder.setClan(clan);
		if (pFounder.isKing())
		{
			pFounder.kingdom.trySetRoyalClan();
		}
		convertFamilyToClan(pFounder, clan);
		addRandomTraitFromBiomeToClan(clan, pFounder.current_tile);
		return clan;
	}

	private void convertFamilyToClan(Actor pFounder, Clan pNewClan)
	{
		if (!pFounder.hasFamily())
		{
			return;
		}
		foreach (Actor child in pFounder.getChildren())
		{
			if (!child.hasClan())
			{
				child.setClan(pNewClan);
			}
		}
	}

	public override void removeObject(Clan pClan)
	{
		foreach (Kingdom item in World.world.kingdoms.list)
		{
			if (item.data.royal_clan_id == pClan.getID() && pClan.getRenown() >= 10)
			{
				item.logRoyalClanLost(pClan);
			}
		}
		World.world.game_stats.data.clansDestroyed++;
		World.world.map_stats.clansDestroyed++;
		base.removeObject(pClan);
	}

	public void addRandomTraitFromBiomeToClan(Clan pClan, WorldTile pTile)
	{
		pClan.addRandomTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_clan, AssetManager.clan_traits);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		foreach (Clan item in list)
		{
			if (!item.hasChief())
			{
				item.checkMembersForNewChief();
			}
		}
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Clan clan = actor.clan;
			if (clan != null && clan.isDirtyUnits())
			{
				clan.listUnit(actor);
			}
		}
	}
}
