using System.Collections.Generic;

namespace ai.behaviours;

public class BehActorTryToTakeItemFromCity : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		foreach (List<long> allEquipmentList in city.data.equipment.getAllEquipmentLists())
		{
			City.giveItem(pActor, allEquipmentList, city);
		}
		return BehResult.Continue;
	}
}
