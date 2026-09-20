using System.Collections.Generic;

public static class WorldBehaviourTilesTemperatureFreezeWaves
{
	private static List<WorldTile> _nextFreezeWave = new List<WorldTile>();

	private static List<WorldTile> _currentWave = new List<WorldTile>();

	private static int _waveNumber = 0;

	private const int MAX_WAVES = 60;

	private const int MAX_TILES_PER_WAVE = 20;

	public static void clear()
	{
		_nextFreezeWave.Clear();
		_currentWave.Clear();
		_waveNumber = 0;
	}

	public static void update()
	{
		if (World.world_era.global_freeze_world)
		{
			updateTileFreezeWaves();
		}
	}

	public static void updateTileFreezeWaves()
	{
		if (_waveNumber == 60)
		{
			_nextFreezeWave.Clear();
			for (int i = 0; i < _currentWave.Count; i++)
			{
				WorldTile worldTile = _currentWave[i];
				if (worldTile.canBeFrozen() && worldTile.heat <= 0)
				{
					_nextFreezeWave.Add(worldTile);
					if (_nextFreezeWave.Count > 20)
					{
						break;
					}
				}
			}
			_nextFreezeWave.Shuffle();
			_waveNumber = 0;
		}
		_currentWave.Clear();
		if (_nextFreezeWave.Count == 0)
		{
			int num = 3 + Randy.randomInt(0, 3);
			while (num-- > 0)
			{
				MapChunk random = World.world.map_chunk_manager.chunks.GetRandom();
				int num2 = 0;
				foreach (WorldTile item in random.tiles.LoopRandom())
				{
					if (item.canBeFrozen() && item.heat <= 0)
					{
						_currentWave.Add(item);
						num2++;
						if (num2 > 5)
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			_currentWave = _nextFreezeWave;
			_nextFreezeWave = new List<WorldTile>();
		}
		for (int j = 0; j < _currentWave.Count; j++)
		{
			WorldTile worldTile2 = _currentWave[j];
			if (worldTile2.canBeFrozen() && (_waveNumber <= 3 || !Randy.randomChance(0.7f)) && worldTile2.freeze(5))
			{
				_nextFreezeWave.AddRange(worldTile2.neighboursAll);
			}
		}
		_waveNumber++;
	}
}
