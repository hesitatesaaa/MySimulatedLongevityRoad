using System.Collections.Generic;
using UnityEngine;

public static class DebugHighlight
{
	public static HashSet<DebugHighlightContainer> hashset = new HashSet<DebugHighlightContainer>();

	private static List<DebugHighlightContainer> to_remove = new List<DebugHighlightContainer>();

	public static void updateDebugHighlights()
	{
		if (hashset.Count == 0)
		{
			return;
		}
		to_remove.Clear();
		foreach (DebugHighlightContainer item in hashset)
		{
			item.timer -= World.world.delta_time;
			if (item.timer < 0f)
			{
				to_remove.Add(item);
			}
		}
		foreach (DebugHighlightContainer item2 in to_remove)
		{
			hashset.Remove(item2);
		}
	}

	public static void newHighlightList(Color pColor, List<TileZone> pZones, float pTime = 3f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		foreach (TileZone pZone in pZones)
		{
			newHighlight(pColor, pZone, pTime);
		}
	}

	public static void newHighlightList(Color pColor, List<MapChunk> pChunks, float pTime = 3f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		foreach (MapChunk pChunk in pChunks)
		{
			newHighlight(pColor, pChunk, pTime);
		}
	}

	public static void clear()
	{
		hashset.Clear();
	}

	public static void newHighlight(Color pColor, MapChunk pChunk, float pTime = 3f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		DebugHighlightContainer debugHighlightContainer = new DebugHighlightContainer();
		debugHighlightContainer.chunk = pChunk;
		debugHighlightContainer.color = pColor;
		debugHighlightContainer.setTimer(pTime);
		hashset.Add(debugHighlightContainer);
	}

	public static void newHighlight(Color pColor, TileZone pZone, float pTime = 3f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		DebugHighlightContainer debugHighlightContainer = new DebugHighlightContainer();
		debugHighlightContainer.zone = pZone;
		debugHighlightContainer.color = pColor;
		debugHighlightContainer.setTimer(pTime);
		hashset.Add(debugHighlightContainer);
	}
}
