namespace ai.behaviours;

public class BehFindFarmField : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.city.calculated_grown_wheat.Count > 0)
		{
			return BehResult.Stop;
		}
		int num = int.MaxValue;
		WorldTile worldTile = null;
		WorldTile current_tile = pActor.current_tile;
		WorldTileContainer calculated_farm_fields = pActor.city.calculated_farm_fields;
		calculated_farm_fields.checkAddRemove();
		foreach (WorldTile item in calculated_farm_fields)
		{
			int num2 = Toolbox.SquaredDistTile(current_tile, item);
			if (num2 < num && item.Type.farm_field && !item.isTargeted() && current_tile.isSameIsland(item) && (!item.hasBuilding() || (item.building.canRemoveForFarms() && !item.building.asset.wheat)))
			{
				num = num2;
				worldTile = item;
			}
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
