using System.Collections.Generic;

public class EnemyFinderData
{
	public readonly List<BaseSimObject> list = new List<BaseSimObject>();

	public bool isEmpty()
	{
		return list.Count == 0;
	}

	public void addEnemyList(List<Actor> pList)
	{
		list.AddRange(pList);
	}

	public void addEnemyList(List<Building> pList)
	{
		list.AddRange(pList);
	}

	public void reset()
	{
		list.Clear();
	}
}
