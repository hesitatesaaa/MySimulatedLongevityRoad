namespace ai.behaviours;

public class CityBehCheckAttackZone : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		City city = pCity.target_attack_city;
		bool flag = true;
		if (city == null)
		{
			flag = false;
		}
		else if (!city.isAlive() || !pCity.hasAnyWarriors() || !city.kingdom.isEnemy(pCity.kingdom) || !city.reachableFrom(pCity))
		{
			flag = false;
		}
		if (!flag)
		{
			pCity.target_attack_city = null;
			pCity.target_attack_zone = null;
			city = null;
		}
		if (pCity.target_attack_city != null && pCity.target_attack_zone.city != pCity.target_attack_city)
		{
			pCity.target_attack_zone = null;
		}
		if (pCity.hasAttackZoneOrder())
		{
			return BehResult.Continue;
		}
		if (city == null)
		{
			if (!pCity.isOkToSendArmy())
			{
				return BehResult.Continue;
			}
			city = findTargetCity(pCity);
		}
		if (city == null)
		{
			return BehResult.Continue;
		}
		pCity.target_attack_city = city;
		if (city.buildings.Count > 0)
		{
			Building random = city.buildings.GetRandom();
			pCity.target_attack_zone = random.current_tile.zone;
		}
		else if (pCity.hasZones())
		{
			pCity.target_attack_zone = pCity.zones.GetRandom();
		}
		return BehResult.Continue;
	}

	private City findTargetCity(City pOurCity)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Kingdom kingdom = pOurCity.kingdom;
		City result = null;
		float num = float.MaxValue;
		using ListPool<City> listPool = new ListPool<City>(BehaviourActionBase<City>.world.cities.Count);
		BehaviourActionBase<City>.world.wars.getWarCities(kingdom, listPool);
		foreach (ref City item in listPool)
		{
			City current = item;
			float num2 = Toolbox.SquaredDistVec2Float(current.city_center, pOurCity.city_center);
			if (!(num2 >= num) && current.reachableFrom(pOurCity))
			{
				result = current;
				num = num2;
			}
		}
		return result;
	}
}
