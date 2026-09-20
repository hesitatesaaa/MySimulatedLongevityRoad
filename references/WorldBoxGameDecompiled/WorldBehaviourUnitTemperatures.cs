using System.Collections.Generic;

public class WorldBehaviourUnitTemperatures
{
	public static Dictionary<Actor, int> temperatures = new Dictionary<Actor, int>();

	private static List<TemperatureMod> _temperature_updaters = new List<TemperatureMod>();

	public static void updateUnitTemperatures()
	{
		if (temperatures.Count == 0)
		{
			return;
		}
		_temperature_updaters.Clear();
		bool flag = true;
		bool flag2 = false;
		foreach (Actor key in temperatures.Keys)
		{
			if (key.isRekt())
			{
				flag2 = true;
				continue;
			}
			flag = false;
			int num = temperatures[key];
			int num2 = num;
			if (num2 > 0)
			{
				num2--;
				if (num2 < 0)
				{
					num2 = 0;
				}
			}
			else
			{
				num2++;
				if (num2 > 0)
				{
					num2 = 0;
				}
			}
			if (num2 != num)
			{
				_temperature_updaters.Add(new TemperatureMod(key, num2));
			}
		}
		if (flag)
		{
			temperatures.Clear();
		}
		else if (_temperature_updaters.Count > 0)
		{
			for (int i = 0; i < _temperature_updaters.Count; i++)
			{
				TemperatureMod temperatureMod = _temperature_updaters[i];
				if (temperatureMod.new_temperature == 0)
				{
					temperatures.Remove(temperatureMod.actor);
				}
				else
				{
					temperatures[temperatureMod.actor] = temperatureMod.new_temperature;
				}
			}
		}
		if (flag2)
		{
			temperatures.RemoveByKey((Actor tActor) => tActor.isRekt());
		}
		_temperature_updaters.Clear();
	}

	private static void changeUnitTemperature(Actor pActor, int pTemperatureChangeSpeed)
	{
		if (!temperatures.ContainsKey(pActor))
		{
			temperatures.Add(pActor, 0);
		}
		temperatures[pActor] += pTemperatureChangeSpeed;
		float num = temperatures[pActor];
		if (pTemperatureChangeSpeed < 0)
		{
			pActor.finishStatusEffect("burning");
			if (num < -200f)
			{
				pActor.addStatusEffect("frozen");
				temperatures[pActor] = 0;
			}
		}
		else
		{
			pActor.finishStatusEffect("frozen");
			if (num > 300f)
			{
				pActor.addStatusEffect("burning");
				temperatures[pActor] = 0;
			}
		}
	}

	public static void checkTile(WorldTile pTile, int pTemperatureChangeSpeed)
	{
		pTile.doUnits(delegate(Actor tActor)
		{
			changeUnitTemperature(tActor, pTemperatureChangeSpeed);
		});
	}

	public static void clear()
	{
		temperatures.Clear();
		_temperature_updaters.Clear();
	}

	public static void removeUnit(Actor pActor)
	{
		temperatures.Remove(pActor);
	}

	public static void debug(DebugTool pTool)
	{
		pTool.setText("units", temperatures.Count, 0f, pShowBar: false, 0L);
		int num = 5;
		foreach (Actor key in temperatures.Keys)
		{
			if (key.isAlive())
			{
				pTool.setText(": " + key.getName(), temperatures[key].ToText(), 0f, pShowBar: false, 0L);
				num--;
				if (num == 0)
				{
					break;
				}
			}
		}
	}
}
