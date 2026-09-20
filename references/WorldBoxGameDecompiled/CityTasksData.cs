using System;

[Serializable]
public class CityTasksData
{
	public int trees;

	public int minerals;

	public int bushes;

	public int plants;

	public int hives;

	public int farm_fields;

	public int farms_total;

	public int wheats;

	public int ruins;

	public int poops;

	public int roads;

	public int fire;

	public void clear()
	{
		trees = 0;
		minerals = 0;
		bushes = 0;
		plants = 0;
		hives = 0;
		ruins = 0;
		poops = 0;
		farm_fields = 0;
		roads = 0;
		wheats = 0;
		fire = 0;
		farms_total = 0;
	}
}
