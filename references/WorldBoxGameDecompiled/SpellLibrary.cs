using Beebyte.Obfuscator;

[ObfuscateLiterals]
public class SpellLibrary : AssetLibrary<SpellAsset>
{
	public override void init()
	{
		base.init();
		add(new SpellAsset
		{
			id = "teleport",
			chance = 0.3f,
			cast_target = CastTarget.Himself,
			health_ratio = 0.6f,
			can_be_used_in_combat = true,
			cost_mana = 15
		});
		t.addDecision("random_teleport");
		t.addDecision("teleport_back_home");
		t.action = ActionLibrary.teleportRandom;
		add(new SpellAsset
		{
			id = "summon_lightning",
			chance = 0.1f,
			min_distance = 6f,
			cost_mana = 5,
			can_be_used_in_combat = true
		});
		t.action = ActionLibrary.castLightning;
		add(new SpellAsset
		{
			id = "summon_tornado",
			chance = 0.1f,
			min_distance = 6f,
			cost_mana = 10,
			can_be_used_in_combat = true
		});
		t.action = ActionLibrary.castTornado;
		add(new SpellAsset
		{
			id = "cast_curse",
			chance = 0.2f,
			min_distance = 4f,
			cast_entity = CastEntity.UnitsOnly,
			cost_mana = 3,
			can_be_used_in_combat = true
		});
		t.action = ActionLibrary.castCurses;
		add(new SpellAsset
		{
			id = "cast_fire",
			chance = 0.2f,
			min_distance = 3f,
			cast_entity = CastEntity.Both,
			can_be_used_in_combat = true,
			cost_mana = 3
		});
		t.addDecision("burn_tumors");
		t.action = ActionLibrary.castFire;
		add(new SpellAsset
		{
			id = "cast_silence",
			chance = 0.2f,
			min_distance = 6f,
			cast_entity = CastEntity.UnitsOnly,
			can_be_used_in_combat = true,
			cost_mana = 5
		});
		t.action = ActionLibrary.castSpellSilence;
		add(new SpellAsset
		{
			id = "cast_blood_rain",
			chance = 0.3f,
			min_distance = 0f,
			health_ratio = 0.9f,
			cast_target = CastTarget.Himself,
			cast_entity = CastEntity.UnitsOnly,
			can_be_used_in_combat = true,
			cost_mana = 2
		});
		t.addDecision("check_heal");
		t.action = ActionLibrary.castBloodRain;
		add(new SpellAsset
		{
			id = "cast_grass_seeds",
			chance = 0.1f,
			min_distance = 0f,
			cast_target = CastTarget.Region,
			cast_entity = CastEntity.Tile,
			cost_mana = 4
		});
		t.action = ActionLibrary.castSpawnGrassSeeds;
		add(new SpellAsset
		{
			id = "spawn_vegetation",
			chance = 0.1f,
			min_distance = 0f,
			cast_target = CastTarget.Region,
			cast_entity = CastEntity.Tile,
			cost_mana = 5
		});
		t.addDecision("spawn_fertilizer");
		t.action = ActionLibrary.castSpawnFertilizer;
		add(new SpellAsset
		{
			id = "spawn_skeleton",
			chance = 0.2f,
			min_distance = 0f,
			cast_target = CastTarget.Himself,
			can_be_used_in_combat = true,
			cost_mana = 10
		});
		t.addDecision("make_skeleton");
		t.action = ActionLibrary.castSpawnSkeleton;
		add(new SpellAsset
		{
			id = "cast_shield",
			chance = 0.2f,
			cast_target = CastTarget.Himself,
			can_be_used_in_combat = true,
			cost_mana = 5
		});
		t.action = ActionLibrary.castShieldOnHimself;
		add(new SpellAsset
		{
			id = "cast_cure",
			chance = 0.3f,
			min_distance = 0f,
			cast_target = CastTarget.Friendly,
			cast_entity = CastEntity.UnitsOnly,
			cost_mana = 3
		});
		t.addDecision("check_cure");
		t.action = ActionLibrary.castCure;
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (SpellAsset item in list)
		{
			if (item.decision_ids != null)
			{
				item.decisions_assets = new DecisionAsset[item.decision_ids.Count];
				for (int i = 0; i < item.decision_ids.Count; i++)
				{
					string pID = item.decision_ids[i];
					DecisionAsset decisionAsset = AssetManager.decisions_library.get(pID);
					item.decisions_assets[i] = decisionAsset;
				}
			}
		}
	}
}
