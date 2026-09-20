using System.Collections.Generic;
using UnityEngine;

public class ExplosionChecker
{
	private const float TIMER = 1f;

	private Dictionary<int, ExplosionMemoryData> data = new Dictionary<int, ExplosionMemoryData>();

	private List<int> _to_remove = new List<int>(16);

	public bool checkNearby(WorldTile pTile, int pRange)
	{
		int num = pRange * 10000000 + pTile.x * 1000 + pTile.y;
		if (data.ContainsKey(num) || isNearbyOthers(pTile, pRange / 3))
		{
			return true;
		}
		add(num, pTile, pRange);
		updateNearbyTimers(pTile, pRange);
		return false;
	}

	private void updateNearbyTimers(WorldTile pTile, float pRange)
	{
		float num = 1f;
		float num2 = pRange;
		num += (float)(data.Count / 10);
		num2 += (float)(data.Count / 5);
		num = Mathf.Clamp(num, 1f, 5f);
		num2 = Mathf.Clamp(num2, pRange, pRange * 5f);
		foreach (int key in data.Keys)
		{
			ExplosionMemoryData explosionMemoryData = data[key];
			if (Toolbox.Dist(pTile.x, pTile.y, explosionMemoryData.x, explosionMemoryData.y) < num2)
			{
				explosionMemoryData.timer = num;
			}
		}
	}

	private bool isNearbyOthers(WorldTile pTile, float pRange)
	{
		foreach (ExplosionMemoryData value in data.Values)
		{
			if (Toolbox.Dist(pTile.x, pTile.y, value.x, value.y) < pRange)
			{
				return true;
			}
		}
		return false;
	}

	private void add(int pID, WorldTile pTile, int pRange)
	{
		ExplosionMemoryData explosionMemoryData = new ExplosionMemoryData();
		explosionMemoryData.range = pRange;
		explosionMemoryData.x = pTile.x;
		explosionMemoryData.y = pTile.y;
		float num = 1f;
		num += (float)(data.Count / 10);
		num = Mathf.Clamp(num, 1f, 5f);
		explosionMemoryData.timer = num;
		data.Add(pID, explosionMemoryData);
	}

	public void update(float pElapsed)
	{
		Bench.bench("explosion_checker", "game_total");
		foreach (int key in data.Keys)
		{
			ExplosionMemoryData explosionMemoryData = data[key];
			explosionMemoryData.timer -= pElapsed;
			if (explosionMemoryData.timer <= 0f)
			{
				_to_remove.Add(key);
			}
		}
		if (_to_remove.Count > 0)
		{
			for (int i = 0; i < _to_remove.Count; i++)
			{
				data.Remove(_to_remove[i]);
			}
			_to_remove.Clear();
		}
		Bench.benchEnd("explosion_checker", "game_total", pSaveCounter: false, 0L);
	}

	public void clear()
	{
		data.Clear();
	}

	public static void debug(DebugTool pTool)
	{
		pTool.setText("explosion_checker", MapBox.instance.explosion_checker.data.Count, 0f, pShowBar: false, 0L);
	}
}
