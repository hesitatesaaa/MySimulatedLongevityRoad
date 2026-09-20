using System.Collections.Generic;

namespace ai.behaviours;

public class BehSpawnTreeFertilizer : BehaviourActionActor
{
	private static List<WorldTile> _tiles = new List<WorldTile>();

	public override BehResult execute(Actor pActor)
	{
		if (!Randy.randomChance(0.3f))
		{
			return BehResult.Stop;
		}
		if (!pActor.current_tile.Type.ground)
		{
			return BehResult.Stop;
		}
		BiomeAsset biome_asset = pActor.current_tile.Type.biome_asset;
		if (biome_asset == null)
		{
			return BehResult.Stop;
		}
		if (biome_asset.grow_vegetation_auto)
		{
			return BehResult.Stop;
		}
		SpellAsset spellAsset = AssetManager.spells.get("spawn_vegetation");
		_tiles.Clear();
		foreach (WorldTile tile in pActor.current_tile.region.tiles)
		{
			if (!(tile.Type.biome_id == "biome_grass"))
			{
				BiomeAsset biome_asset2 = tile.Type.biome_asset;
				if (biome_asset2 != null && biome_asset2.grow_vegetation_auto)
				{
					_tiles.Add(tile);
				}
			}
		}
		if (_tiles.Count == 0)
		{
			return BehResult.Stop;
		}
		spellAsset.action?.Invoke(pActor, pActor, _tiles.GetRandom());
		pActor.doCastAnimation();
		return BehResult.Continue;
	}
}
