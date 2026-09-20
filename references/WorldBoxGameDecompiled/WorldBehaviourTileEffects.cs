using System.Collections.Generic;
using UnityEngine;

public class WorldBehaviourTileEffects
{
	public static void tryToStartTileEffects()
	{
		for (int i = 0; i < 5; i++)
		{
			spawnEffect();
		}
	}

	public static void spawnEffect()
	{
		if (TrailerMonolith.enable_trailer_stuff || !World.world.zone_camera.hasVisibleZones() || World.world.stack_effects.controller_tile_effects.isLimitReached())
		{
			return;
		}
		WorldTile randomTile = World.world.zone_camera.getVisibleZones().GetRandom().getRandomTile();
		TileEffectAsset randomEffect = TileEffectsLibrary.getRandomEffect(randomTile);
		if (randomEffect == null || !Randy.randomChance(randomEffect.chance))
		{
			return;
		}
		WorldTile[] neighboursAll = randomTile.neighboursAll;
		foreach (WorldTile worldTile in neighboursAll)
		{
			if (!randomEffect.tile_types.Contains(worldTile.Type.id))
			{
				return;
			}
		}
		TileEffect tileEffect = EffectsLibrary.spawn("fx_tile_effect", randomTile) as TileEffect;
		if (!((Object)(object)tileEffect == (Object)null))
		{
			tileEffect.load(randomEffect);
		}
	}

	public static void checkTileForEffectKill(WorldTile pTile, int pRadius)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		BaseEffectController controller_tile_effects = World.world.stack_effects.controller_tile_effects;
		List<BaseEffect> list = controller_tile_effects.getList();
		for (int i = 0; i < list.Count; i++)
		{
			BaseEffect baseEffect = list[i];
			if (baseEffect.active)
			{
				float x = ((Component)baseEffect).transform.position.x;
				float y = ((Component)baseEffect).transform.position.y;
				Vector2Int pos = pTile.pos;
				float x2 = ((Vector2Int)(ref pos)).x;
				pos = pTile.pos;
				if (!(Toolbox.Dist(x, y, x2, ((Vector2Int)(ref pos)).y) > (float)pRadius))
				{
					controller_tile_effects.killObject(baseEffect);
					break;
				}
			}
		}
	}
}
