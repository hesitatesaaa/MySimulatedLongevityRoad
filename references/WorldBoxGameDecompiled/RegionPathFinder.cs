using System.Collections.Generic;

public class RegionPathFinder
{
	private Dictionary<string, List<MapRegion>> cachedPaths = new Dictionary<string, List<MapRegion>>();

	private List<MapRegion> _temp_regions_cur_wave = new List<MapRegion>();

	private List<MapRegion> _temp_regions_next_wave = new List<MapRegion>();

	internal List<MapRegion> _temp_regions_checked = new List<MapRegion>();

	internal List<MapRegion> last_globalPath;

	private int currentWave;

	public WorldTile tileStart;

	public WorldTile tileTarget;

	public bool simplePath;

	public PathFinderResult getGlobalPath(WorldTile pFrom, WorldTile pTarget, bool pBoat = false)
	{
		last_globalPath = null;
		if (pFrom == pTarget)
		{
			return PathFinderResult.SamePlace;
		}
		if (pFrom.region == pTarget.region && pFrom.region.tiles.Count == 256)
		{
			return PathFinderResult.SamePlace;
		}
		if (pFrom.region == pTarget.region)
		{
			return PathFinderResult.PathFound;
		}
		if (pFrom.region.island != pTarget.region.island)
		{
			return PathFinderResult.DifferentIslands;
		}
		tileStart = pFrom;
		tileTarget = pTarget;
		string text = pFrom.region.id + "_" + pTarget.region.id;
		if (DebugConfig.isOn(DebugOption.UseCacheForRegionPath) && cachedPaths.TryGetValue(text, out last_globalPath))
		{
			return PathFinderResult.PathFound;
		}
		last_globalPath = new List<MapRegion>();
		startWave(tileTarget.region);
		if (DebugConfig.isOn(DebugOption.UseCacheForRegionPath))
		{
			addToCache(text, last_globalPath);
		}
		return PathFinderResult.PathFound;
	}

	public void addToCache(string pID, List<MapRegion> pPath)
	{
		if (cachedPaths.Count > 1000)
		{
			cachedPaths.Clear();
		}
		cachedPaths.Add(pID, pPath);
	}

	public void clearCache()
	{
		cachedPaths.Clear();
	}

	public void clear()
	{
		last_globalPath = null;
		_temp_regions_cur_wave.Clear();
		_temp_regions_next_wave.Clear();
		clearRegions();
		clearCache();
		currentWave = 0;
		tileStart = null;
		tileTarget = null;
	}

	private void startWave(MapRegion pRegion)
	{
		simplePath = true;
		clearRegions();
		currentWave = 0;
		_temp_regions_cur_wave.Clear();
		_temp_regions_next_wave.Clear();
		addToNext(pRegion);
		newWave();
	}

	private void addToNext(MapRegion pRegion)
	{
		if (pRegion.tiles.Count != 256)
		{
			simplePath = false;
		}
		pRegion.is_checked_path = true;
		_temp_regions_next_wave.Add(pRegion);
		_temp_regions_checked.Add(pRegion);
		pRegion.path_wave_id = currentWave;
	}

	private void newWave()
	{
		currentWave++;
		_temp_regions_cur_wave.Clear();
		_temp_regions_cur_wave.AddRange(_temp_regions_next_wave);
		_temp_regions_next_wave.Clear();
		for (int i = 0; i < _temp_regions_cur_wave.Count; i++)
		{
			MapRegion mapRegion = _temp_regions_cur_wave[i];
			for (int j = 0; j < mapRegion.neighbours.Count; j++)
			{
				MapRegion mapRegion2 = mapRegion.neighbours[j];
				if (!mapRegion2.is_checked_path)
				{
					addToNext(mapRegion2);
					if (mapRegion2 == tileStart.region)
					{
						finalPath(tileStart.region);
						return;
					}
				}
			}
		}
		if (_temp_regions_next_wave.Count > 0)
		{
			newWave();
		}
	}

	private void finalPath(MapRegion pMainRegion)
	{
		last_globalPath.Add(pMainRegion);
		pMainRegion.region_path = true;
		if (pMainRegion == tileTarget.region)
		{
			return;
		}
		MapRegion mapRegion = null;
		for (int i = 0; i < pMainRegion.neighbours.Count; i++)
		{
			MapRegion mapRegion2 = pMainRegion.neighbours[i];
			if (mapRegion2.path_wave_id == -1)
			{
				continue;
			}
			if (mapRegion == null)
			{
				mapRegion = mapRegion2;
			}
			else if (mapRegion2.path_wave_id < mapRegion.path_wave_id)
			{
				mapRegion = mapRegion2;
			}
			else if (mapRegion2.path_wave_id == mapRegion.path_wave_id)
			{
				MapChunk chunk = tileTarget.chunk;
				MapChunk chunk2 = mapRegion.chunk;
				MapChunk chunk3 = mapRegion2.chunk;
				int num = Toolbox.SquaredDist(chunk2.x, chunk2.y, chunk.x, chunk.y);
				if (Toolbox.SquaredDist(chunk3.x, chunk3.y, chunk.x, chunk.y) < num)
				{
					mapRegion = mapRegion2;
				}
			}
		}
		finalPath(mapRegion);
	}

	private void clearRegions()
	{
		for (int i = 0; i < _temp_regions_checked.Count; i++)
		{
			MapRegion mapRegion = _temp_regions_checked[i];
			mapRegion.is_checked_path = false;
			mapRegion.path_wave_id = -1;
			mapRegion.region_path = false;
		}
		_temp_regions_checked.Clear();
	}

	public string debug()
	{
		return cachedPaths.Count.ToString() ?? "";
	}
}
