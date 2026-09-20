using System.Collections.Generic;

public static class RegionLinkHashes
{
	private static readonly Dictionary<int, RegionLink> _dict = new Dictionary<int, RegionLink>();

	private static readonly StackPool<RegionLink> _pool = new StackPool<RegionLink>();

	public static void addHash(int pHash, MapRegion pRegion)
	{
		if (!_dict.TryGetValue(pHash, out var value))
		{
			value = _pool.get();
			value.reset();
			value.id = pHash;
			_dict[value.id] = value;
		}
		if (value.regions.Add(pRegion))
		{
			pRegion.addLink(value);
		}
	}

	public static int getCount()
	{
		return _dict.Count;
	}

	public static void clear()
	{
		foreach (RegionLink value in _dict.Values)
		{
			value.reset();
			_pool.release(value);
		}
		_dict.Clear();
	}

	public static RegionLink getHash(int pHash)
	{
		_dict.TryGetValue(pHash, out var value);
		return value;
	}

	public static void remove(RegionLink pLink, MapRegion pRegion)
	{
		pLink.regions.Remove(pRegion);
		if (pLink.regions.Count == 0 && _dict.Remove(pLink.id))
		{
			pLink.reset();
			_pool.release(pLink);
		}
	}

	public static void debug(DebugTool pTool)
	{
		pTool.setText("hashes", _dict.Count, 0f, pShowBar: false, 0L);
	}
}
