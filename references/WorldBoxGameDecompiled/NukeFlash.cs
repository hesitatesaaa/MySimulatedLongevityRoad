using UnityEngine;

public class NukeFlash : BaseEffect
{
	private bool _killing;

	private bool _bomb_spawned;

	private TerraformOptions _terraform_asset;

	internal void spawnFlash(WorldTile pTile, string pBomb)
	{
		tile = pTile;
		_bomb_spawned = false;
		_terraform_asset = AssetManager.terraform.get(pBomb);
		_killing = false;
		prepare(pTile, 0.1f);
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = m_transform.localScale;
		if (!_killing && localScale.x < 1f)
		{
			Config.grey_goo_damaged = false;
			localScale.x += World.world.elapsed * 2.5f;
			if (localScale.x >= 0.8f && !_bomb_spawned)
			{
				_bomb_spawned = true;
				_terraform_asset.bomb_action?.Invoke(null, tile);
			}
			if (Config.grey_goo_damaged && !AchievementLibrary.isUnlocked(AchievementLibrary.final_resolution))
			{
				AchievementLibrary.final_resolution.check();
			}
			if (localScale.x >= 1f)
			{
				localScale.x = 1f;
				_killing = true;
			}
			localScale.y = localScale.x;
			m_transform.localScale = localScale;
		}
		else if (_killing)
		{
			localScale.x -= World.world.elapsed * 2.5f;
			localScale.y = localScale.x;
			if (localScale.x <= 0f)
			{
				localScale.x = 0f;
				kill();
			}
			m_transform.localScale = localScale;
		}
	}

	internal static bool atomic_bomb_action(BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_nuke_atomic", pTile, 0.8f, 0.9f);
		if (World.world.explosion_checker.checkNearby(pTile, 30))
		{
			return false;
		}
		MapAction.damageWorld(pTile, 30, TerraformLibrary.atomic_bomb);
		return true;
	}

	internal static bool crabzilla_bomb_action(BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 0.8f, 0.9f);
		if (World.world.explosion_checker.checkNearby(pTile, 30))
		{
			return false;
		}
		MapAction.damageWorld(pTile, 30, TerraformLibrary.crabzilla_bomb);
		int num = Randy.randomInt(1, 5);
		for (int i = 0; i < num; i++)
		{
			Actor actor = World.world.units.createNewUnit("crab", pTile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: true, pAdultAge: true);
			actor.addTrait("fire_blood");
			actor.addTrait("fire_proof");
			actor.addTrait("evil");
			actor.addTrait("tough");
		}
		return true;
	}

	internal static bool czar_bomba_action(BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 1.4f, 1.6f);
		if (World.world.explosion_checker.checkNearby(pTile, 70))
		{
			return false;
		}
		MapAction.damageWorld(pTile, 70, TerraformLibrary.czar_bomba);
		return true;
	}
}
