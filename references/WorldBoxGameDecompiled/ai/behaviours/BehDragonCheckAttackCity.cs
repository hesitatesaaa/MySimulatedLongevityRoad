namespace ai.behaviours;

public class BehDragonCheckAttackCity : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_tile_target != null)
		{
			return BehResult.Continue;
		}
		pActor.data.get("cityToAttack", out var pResult, -1L);
		if (!pResult.hasValue())
		{
			return BehResult.Continue;
		}
		pActor.data.get("attacksForCity", out var pResult2, 0);
		if (pResult2 < 1)
		{
			pActor.data.removeLong("cityToAttack");
			return BehResult.Continue;
		}
		City city = BehaviourActionBase<Actor>.world.cities.get(pResult);
		bool flag = true;
		if (city != null && city.isAlive() && city.buildings.Count > 0)
		{
			WorldTile random = city.buildings.GetRandom().current_tile.zone.tiles.GetRandom();
			pActor.beh_tile_target = dragon.randomTileWithinLandAttackRange(random);
			pActor.data.set("attacksForCity", --pResult2);
			flag = false;
		}
		if (flag)
		{
			pActor.data.removeLong("cityToAttack");
			pActor.data.removeInt("attacksForCity");
		}
		return BehResult.Continue;
	}
}
