using System.Collections.Generic;

namespace ai.behaviours;

public class BehGoToActorTarget : BehaviourActionActor
{
	private GoToActorTargetType _type;

	private bool _path_on_water;

	private bool _check_can_attack_target;

	private bool _check_same_island;

	private bool _check_inside_something;

	public BehGoToActorTarget(GoToActorTargetType pType = GoToActorTargetType.SameTile, bool pPathOnWater = false, bool pCheckCanAttackTarget = false, bool pCalibrateTargetPosition = false, float pCheckDistance = 2f, bool pCheckSameIsland = true, bool pCheckInsideSomething = true)
	{
		_path_on_water = pPathOnWater;
		_type = pType;
		_check_can_attack_target = pCheckCanAttackTarget;
		_check_same_island = pCheckSameIsland;
		_check_inside_something = pCheckInsideSomething;
		calibrate_target_position = pCalibrateTargetPosition;
		check_actor_target_position_distance = pCheckDistance;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		BaseSimObject beh_actor_target = pActor.beh_actor_target;
		WorldTile worldTile = beh_actor_target.current_tile;
		if (beh_actor_target.isActor())
		{
			Actor a = beh_actor_target.a;
			if (_check_can_attack_target && !pActor.isTargetOkToAttack(a))
			{
				return BehResult.Stop;
			}
			if (_check_same_island && !pActor.isSameIslandAs(a))
			{
				return BehResult.Stop;
			}
			if (_check_inside_something && a.isInsideSomething())
			{
				return BehResult.Stop;
			}
		}
		switch (_type)
		{
		case GoToActorTargetType.RaycastWithAttackRange:
			worldTile = raycastToTarget(pActor, beh_actor_target);
			if (worldTile == pActor.current_tile)
			{
				return BehResult.Continue;
			}
			break;
		case GoToActorTargetType.SameTile:
			worldTile = beh_actor_target.current_tile;
			break;
		case GoToActorTargetType.SameRegion:
			worldTile = beh_actor_target.current_tile.region.tiles.GetRandom();
			break;
		case GoToActorTargetType.NearbyTile:
			worldTile = beh_actor_target.current_tile.getTileAroundThisOnSameIsland(pActor.current_tile);
			break;
		case GoToActorTargetType.NearbyTileClosest:
			worldTile = beh_actor_target.current_tile.getTileAroundThisOnSameIsland(pActor.current_tile, pClosest: true);
			break;
		}
		if (worldTile == null)
		{
			pActor.ignoreTarget(beh_actor_target);
			return BehResult.Stop;
		}
		if (pActor.goTo(worldTile, _path_on_water) == ExecuteEvent.True)
		{
			return BehResult.Continue;
		}
		pActor.ignoreTarget(beh_actor_target);
		return BehResult.Stop;
	}

	private WorldTile raycastToTarget(Actor pSelf, BaseSimObject pTarget)
	{
		WorldTile current_tile = pSelf.current_tile;
		WorldTile current_tile2 = pTarget.current_tile;
		List<WorldTile> list = PathfinderTools.raycast(current_tile, current_tile2);
		WorldTile worldTile = null;
		float attackRangeSquared = pSelf.getAttackRangeSquared();
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile2 = list[i];
			if (worldTile2.isSameIsland(current_tile) && (float)Toolbox.SquaredDistTile(worldTile2, current_tile2) < attackRangeSquared)
			{
				worldTile = worldTile2;
				break;
			}
		}
		if (worldTile == null)
		{
			worldTile = current_tile2;
		}
		return worldTile;
	}
}
