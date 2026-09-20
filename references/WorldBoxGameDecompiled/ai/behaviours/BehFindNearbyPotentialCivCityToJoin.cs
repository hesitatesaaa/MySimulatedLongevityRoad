using UnityEngine;

namespace ai.behaviours;

public class BehFindNearbyPotentialCivCityToJoin : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		City cityToJoin = getCityToJoin(pActor);
		if (cityToJoin == null)
		{
			return BehResult.Stop;
		}
		if (cityToJoin.kingdom != pActor.kingdom)
		{
			pActor.removeFromPreviousFaction();
		}
		pActor.stopBeingWarrior();
		pActor.joinCity(cityToJoin);
		return BehResult.Continue;
	}

	private static City getCityToJoin(Actor pActor)
	{
		City zone_city = pActor.current_tile.zone_city;
		if (zone_city != null && !zone_city.isNeutral() && zone_city.isWelcomedToJoin(pActor))
		{
			return zone_city;
		}
		return getPotentialCityNearby(pActor.current_tile, pActor);
	}

	public static City getPotentialCityNearby(WorldTile pFromTile, Actor pActor)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		City city = null;
		foreach (City item in BehaviourActionBase<Actor>.world.cities.list.LoopRandom())
		{
			WorldTile tile = item.getTile();
			if (tile == null || item.isNeutral())
			{
				continue;
			}
			float num = SimGlobals.m.nomad_check_far_city_range;
			if (!pActor.isNomad() && item.kingdom != pActor.kingdom)
			{
				num *= 3f;
			}
			if (Toolbox.DistVec3(pFromTile.posV, Vector2.op_Implicit(item.city_center)) > num || !item.isWelcomedToJoin(pActor) || !tile.isSameIsland(pFromTile))
			{
				continue;
			}
			if (city != null)
			{
				if (item.kingdom == pActor.kingdom && city.kingdom != pActor.kingdom)
				{
					city = item;
				}
			}
			else
			{
				city = item;
			}
		}
		return city;
	}
}
