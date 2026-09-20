namespace ai.behaviours;

public class BehMakeItem : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		ItemCrafting.tryToCraftRandomWeapon(pActor, pActor.city);
		for (int i = 0; i < 5; i++)
		{
			if (!ItemCrafting.tryToCraftRandomEquipment(pActor, pActor.city))
			{
				break;
			}
		}
		return BehResult.Continue;
	}
}
