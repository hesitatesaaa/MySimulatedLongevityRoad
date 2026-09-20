using UnityEngine;

public static class QuantumSpriteManager
{
	public static float arrow_middle_current;

	public static float highlight_animation;

	private static float _enemy_anim_timer = 0f;

	private static bool _enemy_anim_timer_positive = true;

	private static bool _initiated = false;

	public static void update()
	{
		if (!_initiated)
		{
			_initiated = true;
			createSystems();
		}
		Bench.bench("quantum_sprites_scale", "game_total");
		updateScaleEffect();
		Bench.benchEnd("quantum_sprites_scale", "game_total", pSaveCounter: false, 0L);
		Bench.bench("quantum_sprites", "game_total");
		updateArrowEffect();
		updateEnemyEffect();
		updateSystems();
		Bench.benchEnd("quantum_sprites", "game_total", pSaveCounter: false, 0L);
	}

	public static void hideAll()
	{
		foreach (QuantumSpriteAsset item in AssetManager.quantum_sprites.list)
		{
			item.group_system?.clearFull();
		}
	}

	private static void createSystems()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		foreach (QuantumSpriteAsset item in AssetManager.quantum_sprites.list)
		{
			QuantumSpriteGroupSystem quantumSpriteGroupSystem = new GameObject().AddComponent<QuantumSpriteGroupSystem>();
			quantumSpriteGroupSystem.create(item);
			item.group_system = quantumSpriteGroupSystem;
			item.group_system.turn_off_renderer = item.turn_off_renderer;
			if (Config.preload_quantum_sprites && item.default_amount != 0)
			{
				for (int i = 0; i < item.default_amount; i++)
				{
					quantumSpriteGroupSystem.getNext();
				}
				quantumSpriteGroupSystem.clearFull();
			}
		}
	}

	private static void updateSystems()
	{
		bool flag = World.world.quality_changer.isLowRes();
		QuantumSpriteAsset[] array = AssetManager.quantum_sprites.getArray();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			QuantumSpriteAsset quantumSpriteAsset = array[i];
			Bench.bench(quantumSpriteAsset.id, "quantum_sprites");
			quantumSpriteAsset.group_system.prepare();
			if ((flag && !quantumSpriteAsset.render_map) || (!flag && !quantumSpriteAsset.render_gameplay))
			{
				quantumSpriteAsset.group_system.update(0f);
			}
			else
			{
				if (quantumSpriteAsset.debug_option != DebugOption.Nothing)
				{
					if (DebugConfig.isOn(quantumSpriteAsset.debug_option))
					{
						quantumSpriteAsset.draw_call(quantumSpriteAsset);
					}
				}
				else
				{
					quantumSpriteAsset.draw_call(quantumSpriteAsset);
				}
				quantumSpriteAsset.group_system.update(0f);
			}
			Bench.benchEnd(quantumSpriteAsset.id, "quantum_sprites", pSaveCounter: true, quantumSpriteAsset.group_system.count_active_debug);
		}
	}

	private static void updateEnemyEffect()
	{
		if (_enemy_anim_timer > 0f)
		{
			_enemy_anim_timer -= Time.deltaTime;
			return;
		}
		_enemy_anim_timer = 0.02f;
		if (_enemy_anim_timer_positive)
		{
			highlight_animation++;
			if (highlight_animation > 10f)
			{
				_enemy_anim_timer_positive = !_enemy_anim_timer_positive;
			}
		}
		else
		{
			highlight_animation--;
			if (highlight_animation == 0f)
			{
				_enemy_anim_timer_positive = !_enemy_anim_timer_positive;
			}
		}
	}

	private static void updateArrowEffect()
	{
		arrow_middle_current += 10f * Time.deltaTime;
		if (arrow_middle_current >= 5f)
		{
			arrow_middle_current -= 5f;
		}
	}

	private static void updateScaleEffect()
	{
		WorldTile worldTile = null;
		if (InputHelpers.mouseSupported)
		{
			worldTile = World.world.getMouseTilePos();
		}
		City city = null;
		Kingdom kingdom = null;
		Alliance alliance = null;
		if (worldTile != null && !World.world.isBusyWithUI())
		{
			city = worldTile.zone.city;
			kingdom = worldTile.zone.city?.kingdom;
			alliance = kingdom?.getAlliance();
		}
		float num = 0.1f;
		Kingdom kingdom2 = null;
		if (World.world.isSelectedPower("relations"))
		{
			kingdom2 = SelectedMetas.selected_kingdom;
		}
		foreach (City city2 in World.world.cities)
		{
			Kingdom kingdom3 = city2.kingdom;
			if (kingdom3.wild)
			{
				continue;
			}
			bool flag = false;
			if (Zones.isBordersEnabled())
			{
				if (Zones.showCityZones())
				{
					if (city2 == city || kingdom3 == kingdom2)
					{
						flag = true;
						city2.setCursorOver();
					}
				}
				else if (Zones.showKingdomZones())
				{
					if (city2.kingdom == kingdom || kingdom3 == kingdom2)
					{
						flag = true;
						kingdom3.setCursorOver();
					}
				}
				else if (Zones.showAllianceZones() && kingdom != null)
				{
					Kingdom kingdom4 = city2.kingdom;
					if (kingdom4 == null)
					{
						continue;
					}
					Alliance alliance2 = kingdom4.getAlliance();
					if (kingdom4.hasAlliance())
					{
						if (alliance2 == alliance || kingdom3 == kingdom2)
						{
							flag = true;
							city2.setCursorOver();
							kingdom3.setCursorOver();
						}
					}
					else if (city2.kingdom == kingdom || kingdom3 == kingdom2)
					{
						flag = true;
						city2.setCursorOver();
						kingdom3.setCursorOver();
					}
				}
			}
			if (city2.isCursorOver())
			{
				flag = true;
			}
			if (city2.kingdom.isCursorOver())
			{
				flag = true;
			}
			if (city2.kingdom.hasAlliance() && city2.kingdom.getAlliance().isCursorOver())
			{
				flag = true;
			}
			if (flag)
			{
				city2.mark_scale_effect += num;
			}
			else
			{
				city2.mark_scale_effect -= num;
			}
			city2.mark_scale_effect = Mathf.Clamp(city2.mark_scale_effect, 0.5f, 0.75f);
		}
	}
}
