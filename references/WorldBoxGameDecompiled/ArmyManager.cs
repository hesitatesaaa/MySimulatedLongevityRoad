using System.Collections.Generic;

public class ArmyManager : MetaSystemManager<Army, ArmyData>
{
	public ArmyManager()
	{
		type_id = "army";
	}

	public Army newArmy(Actor pActor, City pCity)
	{
		World.world.game_stats.data.armiesCreated++;
		World.world.map_stats.armiesCreated++;
		Army army = newObject();
		army.createArmy(pActor, pCity);
		pActor.setArmy(army);
		pCity.setArmy(army);
		return army;
	}

	public override void removeObject(Army pObject)
	{
		World.world.game_stats.data.armiesDestroyed++;
		World.world.map_stats.armiesDestroyed++;
		base.removeObject(pObject);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		using IEnumerator<Army> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.checkCaptainExistence();
		}
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Army army = actor.army;
			if (army != null && army.isDirtyUnits())
			{
				army.listUnit(actor);
			}
		}
	}
}
