using System.Collections.Generic;
using UnityEngine;

public class ResourceThrowManager
{
	private List<ResourceThrowData> _list = new List<ResourceThrowData>();

	public void update(float pElapsed)
	{
		updateRemoval();
	}

	private void updateRemoval()
	{
		List<ResourceThrowData> list = _list;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ResourceThrowData resourceThrowData = list[num];
			if (resourceThrowData.isFinished())
			{
				list.RemoveAt(num);
				World.world.buildings.get(resourceThrowData.building_target_id)?.startShake(0.3f);
			}
		}
	}

	public void addNew(Vector2 pStart, Vector2 pEnd, float pDuration, string pResourceAssetId, int pResourceAmount, float pHeight, Building pBuildingTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		ResourceThrowData item = new ResourceThrowData(pStart, pEnd, pDuration, pResourceAssetId, pResourceAmount, pBuildingTarget.getID(), pHeight);
		_list.Add(item);
	}

	public List<ResourceThrowData> getList()
	{
		return _list;
	}

	public void clear()
	{
		_list.Clear();
	}
}
