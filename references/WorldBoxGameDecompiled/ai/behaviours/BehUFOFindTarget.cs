using System;
using UnityEngine;

namespace ai.behaviours;

public class BehUFOFindTarget : BehaviourActionActor
{
	public unsafe override BehResult execute(Actor pActor)
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		pActor.beh_tile_target = null;
		pActor.data.get("attacksForCity", out var pResult, 0);
		pActor.data.get("cityToAttack", out var pResult2, -1L);
		City city = (pResult2.hasValue() ? BehaviourActionBase<Actor>.world.cities.get(pResult2) : null);
		if (pResult > 0 && city != null)
		{
			if (!city.isAlive() || city.buildings.Count == 0)
			{
				pActor.beh_tile_target = null;
				pResult = 0;
				pActor.data.removeLong("cityToAttack");
			}
			else
			{
				Building random = city.buildings.GetRandom();
				pActor.beh_tile_target = random.current_tile.zone.tiles.GetRandom();
				pResult--;
			}
		}
		else if (pResult <= 0)
		{
			pActor.beh_tile_target = null;
			pActor.data.removeLong("cityToAttack");
		}
		if (pResult > 0)
		{
			pActor.data.set("attacksForCity", pResult);
		}
		else
		{
			pActor.data.removeInt("attacksForCity");
		}
		if (pActor.beh_tile_target == null)
		{
			WorldTile worldTile = Toolbox.getRandomTileWithinDistance(pActor.current_tile, 100);
			if (!BehaviourActionBase<Actor>.world.islands_calculator.hasGround())
			{
				pActor.beh_tile_target = worldTile;
				return BehResult.Continue;
			}
			int num = 5;
			while (!worldTile.Type.ground && num > 0)
			{
				worldTile = Toolbox.getRandomTileWithinDistance(pActor.current_tile, 100);
				num--;
			}
			if (!worldTile.Type.ground && BehaviourActionBase<Actor>.world.islands_calculator.getRandomIslandGround() != null)
			{
				Span<Vector2Int> pArray = new Span<Vector2Int>(stackalloc Vector2Int[8], 8);
				for (int i = 0; i < 8; i++)
				{
					pArray[i] = BehaviourActionBase<Actor>.world.islands_calculator.tryGetRandomGround().pos;
				}
				Vector2Int closestTile = Toolbox.getClosestTile(pArray, pActor.current_tile);
				worldTile = BehaviourActionBase<Actor>.world.GetTileSimple(((Vector2Int)(ref closestTile)).x, ((Vector2Int)(ref closestTile)).y);
			}
			pActor.beh_tile_target = worldTile;
		}
		return BehResult.Continue;
	}
}
