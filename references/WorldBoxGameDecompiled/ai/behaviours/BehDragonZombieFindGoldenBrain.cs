namespace ai.behaviours;

public class BehDragonZombieFindGoldenBrain : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasTrait("zombie"))
		{
			return BehResult.Continue;
		}
		if (dragon.aggroTargets.Count > 0)
		{
			return BehResult.Continue;
		}
		pActor.data.get("cityToAttack", out var pResult, -1L);
		if (pResult.hasValue())
		{
			return BehResult.Continue;
		}
		if (BehaviourActionBase<Actor>.world.kingdoms_wild.get("golden_brain").hasBuildings())
		{
			float num = 0f;
			float num2 = 0f;
			WorldTile worldTile = null;
			foreach (Building building in BehaviourActionBase<Actor>.world.kingdoms_wild.get("golden_brain").buildings)
			{
				num = Toolbox.DistTile(building.current_tile, pActor.current_tile);
				if (worldTile == null || num < num2)
				{
					worldTile = building.current_tile;
					num2 = num;
				}
			}
			if (worldTile != null)
			{
				if (dragon.landAttackRange(worldTile))
				{
					return forceTask(pActor, "dragon_land");
				}
				pActor.beh_tile_target = dragon.randomTileWithinLandAttackRange(worldTile);
			}
		}
		return BehResult.Continue;
	}
}
