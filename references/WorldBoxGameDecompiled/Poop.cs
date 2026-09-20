using UnityEngine;

public class Poop : BaseBuildingComponent
{
	public override void update(float pElapsed)
	{
		if (Time.frameCount % 30 == 0 && Date.getYearsSince(building.data.created_time) >= 1)
		{
			BuildingActions.tryGrowVegetationRandom(building.current_tile, VegetationType.Plants, pOnStart: false, pCheckLimit: false, pCheckRandom: false);
			building.startDestroyBuilding();
		}
	}
}
