using UnityEngine;

public static class DebugZonesTool
{
	public static void actionGrowBorder()
	{
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos != null)
		{
			TileZone zone = mouseTilePos.zone;
			if (zone.hasCity())
			{
				World.world.city_zone_helper.city_growth.getZoneToClaim(null, zone.city);
			}
		}
	}

	public static void actionAbandonZones()
	{
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos != null)
		{
			TileZone zone = mouseTilePos.zone;
			if (zone.hasCity())
			{
				Bench.bench("abandon_stuff", "meh");
				World.world.city_zone_helper.city_abandon.check(zone.city, pDebug: true);
				Debug.Log((object)("bench abandon: " + Bench.benchEnd("abandon_stuff", "meh", pSaveCounter: false, 0L)));
			}
		}
	}
}
