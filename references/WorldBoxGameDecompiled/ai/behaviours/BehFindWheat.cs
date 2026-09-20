namespace ai.behaviours;

public class BehFindWheat : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		int num = int.MaxValue;
		WorldTile worldTile = null;
		Building beh_building_target = null;
		foreach (WorldTile item in pActor.city.calculated_grown_wheat)
		{
			if (item.building != null && item.building.asset.wheat)
			{
				int num2 = Toolbox.SquaredDistTile(pActor.current_tile, item);
				if (num2 < num && item.building.isUsable() && item.building.component_wheat.isMaxLevel() && item.isSameIsland(pActor.current_tile) && !item.isTargeted())
				{
					num = num2;
					worldTile = item;
					beh_building_target = item.building;
				}
			}
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		pActor.beh_building_target = beh_building_target;
		return BehResult.Continue;
	}
}
