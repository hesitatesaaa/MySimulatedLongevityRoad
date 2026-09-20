using System.Collections.Generic;

public class Heat
{
	public const int MAX_HEAT = 404;

	private float tickTimer;

	private List<WorldTile> tilesToRemove = new List<WorldTile>();

	private HashSetWorldTile tiles;

	internal void clear()
	{
		if (tiles == null)
		{
			tiles = new HashSetWorldTile();
		}
		foreach (WorldTile tile in tiles)
		{
			tile.heat = 0;
		}
		tiles.Clear();
	}

	internal void addTile(WorldTile pTile, int pHeat = 1)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (pTile.heat == 0)
		{
			tiles.Add(pTile);
		}
		pTile.heat += pHeat;
		if (pTile.heat > 404)
		{
			pTile.heat = 404;
		}
		if (pTile.heat > 5)
		{
			if (pTile.hasBuilding() && pTile.building.isAlive())
			{
				pTile.building.getHit(0f);
			}
			if (pTile.Type.layer_type == TileLayerType.Ocean)
			{
				MapAction.removeLiquid(pTile);
				World.world.particles_smoke.spawn(pTile.posV3);
			}
			if (pTile.isTemporaryFrozen())
			{
				pTile.unfreeze();
			}
			if (pTile.Type.grey_goo)
			{
				pTile.startFire();
			}
			pTile.setBurned();
		}
		if (pTile.heat > 10)
		{
			if (pTile.Type.burnable)
			{
				if (pTile.Type.IsType("tnt") || pTile.Type.IsType("tnt_timed"))
				{
					AchievementLibrary.tnt_and_heat.check();
				}
				pTile.startFire();
			}
			if (pTile.hasBuilding() && !pTile.building.isRuin())
			{
				pTile.building.getHit((float)pTile.building.getMaxHealth() * 0.1f, pFlash: true, AttackType.Divine, null, pSkipIfShake: false);
			}
			if (pTile.hasBuilding())
			{
				pTile.startFire();
			}
			pTile.doUnits(delegate(Actor tActor)
			{
				if (!tActor.asset.very_high_flyer && !tActor.isImmuneToFire())
				{
					ActionLibrary.addBurningEffectOnTarget(null, tActor);
					tActor.getHit(50f, pFlash: true, AttackType.Fire, null, pSkipIfShake: false);
				}
			});
		}
		if (pTile.heat > 20)
		{
			if (pTile.Type.explodable && pTile.explosion_wave == 0)
			{
				World.world.explosion_layer.explodeBomb(pTile);
			}
			if (pTile.hasBuilding())
			{
				pTile.startFire();
			}
			if (pTile.top_type != null)
			{
				MapAction.decreaseTile(pTile, pDamage: true);
			}
		}
		if (WorldLawLibrary.world_law_gaias_covenant.isEnabled())
		{
			return;
		}
		if (pTile.heat > 30)
		{
			if (pTile.Type.lava)
			{
				LavaHelper.addLava(pTile);
			}
			if (pTile.Type.IsType("soil_low") || pTile.Type.IsType("soil_high"))
			{
				pTile.setTileType("sand");
			}
			if (pTile.Type.road)
			{
				pTile.setTileType("sand");
			}
		}
		if (pTile.heat > 100 && pTile.Type.IsType("sand"))
		{
			LavaHelper.addLava(pTile);
		}
		if (pTile.heat > 160)
		{
			if (pTile.Type.IsType("mountains"))
			{
				LavaHelper.addLava(pTile);
			}
			if (pTile.Type.IsType("hills"))
			{
				LavaHelper.addLava(pTile);
			}
		}
	}

	internal void update(float pElapsed)
	{
		if (World.world.isPaused() || tiles.Count == 0)
		{
			return;
		}
		if (tickTimer > 0f)
		{
			tickTimer -= pElapsed;
			return;
		}
		tickTimer = 1f;
		tilesToRemove.Clear();
		foreach (WorldTile tile in tiles)
		{
			tile.heat--;
			if (tile.heat <= 0)
			{
				tilesToRemove.Add(tile);
			}
		}
		for (int i = 0; i < tilesToRemove.Count; i++)
		{
			WorldTile item = tilesToRemove[i];
			tiles.Remove(item);
		}
		tilesToRemove.Clear();
	}
}
