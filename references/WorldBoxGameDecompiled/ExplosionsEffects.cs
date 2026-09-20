using System.Collections.Generic;
using UnityEngine;

public class ExplosionsEffects : MapLayer
{
	private Dictionary<WorldTile, TileTypeBase> explosionDict;

	private Dictionary<WorldTile, TileTypeBase> explosionDictCurrent;

	public List<WorldTile> explosionQueue;

	private List<WorldTile> explosionQueueCurrent;

	private float timerExplosionQueue;

	public float interval = 0.01f;

	internal Queue<WorldTile> nextWave = new Queue<WorldTile>();

	internal HashSetWorldTile hashset_bombs = new HashSetWorldTile();

	internal List<WorldTile> delayedBombs = new List<WorldTile>();

	internal List<WorldTile> timedBombs = new List<WorldTile>();

	internal override void create()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		colorValues = new Color(1f, 1f, 1f, 1f);
		colors_amount = 60;
		explosionQueue = new List<WorldTile>();
		explosionQueueCurrent = new List<WorldTile>();
		explosionDict = new Dictionary<WorldTile, TileTypeBase>();
		explosionDictCurrent = new Dictionary<WorldTile, TileTypeBase>();
		hashsetTiles = new HashSetWorldTile();
		base.create();
	}

	internal override void clear()
	{
		explosionQueue.Clear();
		explosionQueueCurrent.Clear();
		explosionDict.Clear();
		explosionDictCurrent.Clear();
		hashsetTiles.Clear();
		timedBombs.Clear();
		delayedBombs.Clear();
		nextWave.Clear();
		hashset_bombs.Clear();
		base.clear();
	}

	internal void activateDelayedBomb(WorldTile pBomb)
	{
		if (!delayedBombs.Contains(pBomb))
		{
			delayedBombs.Add(pBomb);
			pBomb.delayed_bomb_type = pBomb.Type.id;
			pBomb.delayed_timer_bomb = 0.09f;
		}
	}

	internal void addTimedTnt(WorldTile pTile)
	{
		if (!timedBombs.Contains(pTile))
		{
			pTile.delayed_timer_bomb = 5f;
			timedBombs.Add(pTile);
		}
	}

	internal void explodeBomb(WorldTile pBombTile, bool pForce = false)
	{
		if (hashset_bombs.Contains(pBombTile))
		{
			return;
		}
		if (pBombTile.Type.explodable_delayed && !pForce)
		{
			activateDelayedBomb(pBombTile);
			return;
		}
		World.world.startShake();
		nextWave.Enqueue(pBombTile);
		while (nextWave.Count > 0)
		{
			WorldTile worldTile = nextWave.Dequeue();
			hashset_bombs.Add(worldTile);
			if (worldTile.Type.explodable && !worldTile.Type.explodable_delayed)
			{
				worldTile.explosion_wave = worldTile.Type.explode_range;
				worldTile.explosion_power = worldTile.Type.explode_range;
			}
			if (worldTile.explosion_wave <= 0)
			{
				continue;
			}
			for (int i = 0; i < worldTile.neighbours.Length; i++)
			{
				WorldTile worldTile2 = worldTile.neighbours[i];
				if (worldTile2.explosion_wave > 0 && hashset_bombs.Contains(worldTile2))
				{
					if (worldTile2.explosion_wave < worldTile.explosion_wave && worldTile2.Type.explodable)
					{
					}
				}
				else
				{
					hashset_bombs.Add(worldTile2);
					worldTile2.explosion_wave = worldTile.explosion_wave - 1;
					worldTile2.explosion_power = worldTile.explosion_power;
					nextWave.Enqueue(worldTile2);
				}
			}
		}
		if (hashset_bombs.Count < 20)
		{
			MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionSmall", pBombTile);
		}
		else
		{
			MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionMiddle", pBombTile);
		}
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>(hashset_bombs);
		foreach (ref WorldTile item in listPool)
		{
			WorldTile current = item;
			MapAction.explodeTile(current, (current.explosion_power - current.explosion_wave) * 10, current.explosion_power * 10, pBombTile, AssetManager.terraform.get("bomb"));
		}
	}

	public void prepareNewExplosion(WorldTile pTile)
	{
		if (!explosionDict.ContainsKey(pTile))
		{
			explosionQueue.Add(pTile);
			explosionDict.Add(pTile, pTile.Type);
		}
	}

	private void updateExplosionQueue()
	{
		if (timerExplosionQueue > 0f)
		{
			timerExplosionQueue -= World.world.elapsed;
			return;
		}
		timerExplosionQueue = 0.1f;
		if (explosionQueue.Count != 0)
		{
			for (int i = 0; i < explosionQueue.Count; i++)
			{
				WorldTile worldTile = explosionQueue[i];
				explosionQueueCurrent.Add(worldTile);
				explosionDictCurrent.Add(worldTile, explosionDict[worldTile]);
			}
			explosionQueue.Clear();
			explosionDict.Clear();
			for (int j = 0; j < explosionQueueCurrent.Count; j++)
			{
				WorldTile worldTile2 = explosionQueueCurrent[j];
				MapAction.damageWorld(worldTile2, explosionDictCurrent[worldTile2].explode_range, AssetManager.terraform.get("bomb"));
				MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionMiddle", worldTile2);
			}
			explosionQueueCurrent.Clear();
			explosionDictCurrent.Clear();
		}
	}

	public override void update(float pElapsed)
	{
		checkAutoDisable();
		if (timedBombs.Count <= 0 || World.world.isPaused())
		{
			return;
		}
		int num = 0;
		while (num < timedBombs.Count)
		{
			WorldTile worldTile = timedBombs[num];
			if (worldTile.delayed_timer_bomb > 0f)
			{
				worldTile.delayed_timer_bomb -= pElapsed;
				num++;
				continue;
			}
			timedBombs.RemoveAt(num);
			if (worldTile.Type.explodable_timed)
			{
				MapAction.damageWorld(worldTile, worldTile.Type.explode_range, AssetManager.terraform.get("bomb"));
				MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionMiddle", worldTile);
			}
		}
	}

	public override void draw(float pElapsed)
	{
		if (Object.op_Implicit((Object)(object)sprRnd) || delayedBombs.Count > 0)
		{
			UpdateDirty(pElapsed);
		}
	}

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (delayedBombs.Count > 0)
		{
			int num = 0;
			while (num < delayedBombs.Count)
			{
				WorldTile worldTile = delayedBombs[num];
				worldTile.delayed_timer_bomb -= World.world.elapsed;
				if (worldTile.delayed_timer_bomb <= 0f)
				{
					worldTile.delayed_timer_bomb = -100f;
					delayedBombs.Remove(worldTile);
					TileTypeBase tileTypeBase = (string.IsNullOrEmpty(worldTile.delayed_bomb_type) ? TopTileLibrary.tnt_timed : AssetManager.top_tiles.get(worldTile.delayed_bomb_type));
					MapAction.damageWorld(worldTile, tileTypeBase.explode_range, AssetManager.terraform.get("bomb"));
					MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionMiddle", worldTile);
				}
				else
				{
					num++;
				}
			}
		}
		if (hashset_bombs.Count > 0)
		{
			foreach (WorldTile hashset_bomb in hashset_bombs)
			{
				hashset_bomb.explosion_wave = 0;
				hashset_bomb.explosion_power = 0;
			}
			hashset_bombs.Clear();
		}
		if (timer > 0f)
		{
			timer -= World.world.elapsed;
			return;
		}
		timer = interval;
		if (hashsetTiles.Count == 0)
		{
			return;
		}
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>();
		foreach (WorldTile hashsetTile in hashsetTiles)
		{
			if (hashsetTile.explosion_fx_stage > 0)
			{
				if (Randy.randomBool())
				{
					pixels[hashsetTile.data.tile_id] = Toolbox.clear;
				}
				else
				{
					pixels[hashsetTile.data.tile_id] = colors[hashsetTile.explosion_fx_stage - 1];
				}
				hashsetTile.explosion_fx_stage--;
				if (hashsetTile.explosion_fx_stage <= 0)
				{
					hashsetTile.explosion_fx_stage = 0;
					listPool.Add(hashsetTile);
				}
			}
		}
		if (listPool.Count > 0)
		{
			for (int i = 0; i < listPool.Count; i++)
			{
				WorldTile item = listPool[i];
				hashsetTiles.Remove(item);
			}
		}
		updatePixels();
	}

	internal void setDirty(WorldTile pTile, float pDist, float pRadius)
	{
		int num = (int)(60f * (1f - pDist / pRadius));
		if (num != 0 && num >= pTile.explosion_fx_stage)
		{
			hashsetTiles.Add(pTile);
			pTile.explosion_fx_stage = num;
		}
	}
}
