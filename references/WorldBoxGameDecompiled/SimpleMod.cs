using ai.behaviours;

public class SimpleMod
{
	public SimpleMod()
	{
		ProjectileAsset projectileAsset = AssetManager.projectiles.clone("skeleton_arrow", "arrow");
		projectileAsset.trail_effect_enabled = true;
		projectileAsset.texture = "fireball";
		projectileAsset.scale_target = 0.5f;
		EquipmentAsset equipmentAsset = AssetManager.items.clone("skeleton_bow", "_range");
		equipmentAsset.base_stats["range"] = 22f;
		equipmentAsset.base_stats["critical_chance"] = 0.1f;
		equipmentAsset.base_stats["critical_damage_multiplier"] = 0.5f;
		equipmentAsset.projectile = "skeleton_arrow";
		ActorAsset actorAsset = AssetManager.actor_library.clone("super_skeleton", "skeleton");
		actorAsset.default_attack = "skeleton_bow";
		actorAsset.default_weapons = null;
		actorAsset.base_stats["health"] = 100000f;
		actorAsset.base_stats["damage"] = 500f;
		actorAsset.base_stats["speed"] = 500f;
		actorAsset.job = Toolbox.a<string>("super_skeleton_job");
		AssetManager.actor_library.addTrait("regeneration");
		AssetManager.actor_library.addTrait("immortal");
		ActorJob actorJob = AssetManager.job_actor.add(new ActorJob
		{
			id = "super_skeleton_job"
		});
		actorJob.addTask("mod_destroy_trees");
		actorJob.addTask("random_move");
		actorJob.addTask("wait");
		actorJob.addTask("attack_golden_brain");
		BehaviourTaskActor behaviourTaskActor = AssetManager.tasks_actor.add(new BehaviourTaskActor
		{
			id = "mod_destroy_trees"
		});
		behaviourTaskActor.addBeh(new BehFindBuilding("type_tree", pOnlyNonTargeted: true, pOnlyWithResources: true));
		behaviourTaskActor.addBeh(new BehGoToBuildingTarget());
		behaviourTaskActor.addBeh(new BehResourceGatheringAnimation(1f));
		behaviourTaskActor.addBeh(new BehResourceGatheringAnimation(1f));
		behaviourTaskActor.addBeh(new BehResourceGatheringAnimation(1f));
		behaviourTaskActor.addBeh(new BehResourceGatheringAnimation(1f));
		behaviourTaskActor.addBeh(new BehExtractResourcesFromBuilding());
		behaviourTaskActor.addBeh(new BehRandomWait(1f, 2f));
	}
}
