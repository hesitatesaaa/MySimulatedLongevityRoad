using ai.behaviours.conditions;

public class ActorJobLibrary : AssetLibrary<ActorJob>
{
	public override void init()
	{
		base.init();
		initJobsCivs();
		initJobsMobs();
	}

	private void initJobsCivs()
	{
		add(new ActorJob
		{
			id = "unit_citizen"
		});
		t.addTask("make_decision");
		t.addTask("check_city_destroyed");
		add(new ActorJob
		{
			id = "hunter"
		});
		t.addTask("random_move");
		t.addTask("do_hunting");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "builder"
		});
		t.addTask("try_build_building");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "cleaner"
		});
		t.addTask("cleaning");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "manure_cleaner"
		});
		t.addTask("manure_cleaning");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "road_builder"
		});
		t.addTask("build_road");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "woodcutter"
		});
		t.addTask("chop_trees");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "miner"
		});
		t.addTask("mine");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "miner_deposit"
		});
		t.addTask("mine_deposit");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "gatherer_bushes"
		});
		t.addTask("collect_fruits");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "gatherer_herbs"
		});
		t.addTask("collect_herbs");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "gatherer_honey"
		});
		t.addTask("collect_honey");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "farmer"
		});
		t.addTask("farmer_make_field");
		t.addTask("farmer_plant_crops");
		t.addTask("farmer_harvest");
		t.addTask("farmer_fertilize_crops");
		t.addTask("farmer_random_move");
		t.addTask("check_end_job");
		t.addTask("check_city_destroyed");
		add(new ActorJob
		{
			id = "attacker"
		});
		t.addTask("make_decision");
	}

	private void initJobsMobs()
	{
		add(new ActorJob
		{
			id = "crab"
		});
		t.addTask("swim_to_island");
		t.addTask("crab_danger_check");
		t.addTask("random_move");
		t.addTask("crab_danger_check");
		t.addTask("crab_eat");
		t.addTask("crab_danger_check");
		add(new ActorJob
		{
			id = "crab_burrow"
		});
		t.addTask("crab_burrow");
		add(new ActorJob
		{
			id = "decision"
		});
		t.addTask("make_decision");
		add(new ActorJob
		{
			id = "egg"
		});
		t.addTask("wait10");
		t.addTask("end_job");
		add(new ActorJob
		{
			id = "random_move"
		});
		t.addTask("random_move");
		add(new ActorJob
		{
			id = "random_swim"
		});
		t.addTask("random_swim");
		add(new ActorJob
		{
			id = "printer_job"
		});
		t.addTask("print_start");
		t.addTask("print_step");
		add(new ActorJob
		{
			id = "godfinger_job",
			random = true
		});
		t.addTask("godfinger_move");
		t.addTask("godfinger_move");
		t.addTask("godfinger_find_target");
		t.addTask("godfinger_random_fun_move");
		t.addTask("godfinger_random_fun_move");
		t.addTask("godfinger_circle_move");
		t.addTask("godfinger_circle_move");
		t.addTask("godfinger_circle_move_big");
		t.addTask("godfinger_circle_move_big");
		t.addTask("godfinger_circle_move_small");
		t.addTask("godfinger_circle_move_small");
		add(new ActorJob
		{
			id = "dragon_job",
			random = true
		});
		t.addTask("dragon_slide");
		t.addCondition(new CondActorFlying());
		t.addCondition(new CondNoPeace());
		t.addCondition(new CondDragonHasTargets());
		t.addCondition(new CondDragonCanSlide());
		t.addTask("dragon_fly");
		t.addCondition(new CondActorFlying());
		t.addTask("dragon_land");
		t.addCondition(new CondActorFlying());
		t.addCondition(new CondDragonCanLand());
		t.addTask("dragon_sleep");
		t.addCondition(new CondActorFlying(), pExpectedResult: false);
		t.addCondition(new CondDragonSleepy());
		t.addCondition(new CondDragonHasTargets(), pExpectedResult: false);
		t.addCondition(new CondDragonHasCityTarget(), pExpectedResult: false);
		t.addCondition(new CondCurrentTileNoOtherUnits());
		t.addTask("dragon_wakeup");
		t.addCondition(new CondActorFlying(), pExpectedResult: false);
		t.addCondition(new CondDragonSleeping());
		t.addTask("dragon_land_attack");
		t.addCondition(new CondActorFlying(), pExpectedResult: false);
		t.addCondition(new CondNoPeace());
		t.addCondition(new CondDragonCanLandAttack());
		t.addTask("dragon_idle");
		t.addCondition(new CondActorFlying(), pExpectedResult: false);
		t.addCondition(new CondDragonHasTargets(), pExpectedResult: false);
		t.addCondition(new CondDragonHasCityTarget(), pExpectedResult: false);
		t.addTask("dragon_up");
		t.addCondition(new CondActorFlying(), pExpectedResult: false);
		t.addCondition(new CondActorNotJustLanded());
		t.addCondition(new CondDragonCanLandAttack(), pExpectedResult: false);
		add(new ActorJob
		{
			id = "ufo_job"
		});
		t.addTask("ufo_idle");
		t.addTask("ufo_fly");
		t.addTask("ufo_explore");
		add(new ActorJob
		{
			id = "worm_job"
		});
		t.addTask("worm_move");
		add(new ActorJob
		{
			id = "sandspider_job"
		});
		t.addTask("sandspider_move");
		add(new ActorJob
		{
			id = "ant_black"
		});
		t.addTask("ant_black_island");
		t.addTask("ant_black_sand");
		add(new ActorJob
		{
			id = "ant_red"
		});
		t.addTask("ant_red_move");
		add(new ActorJob
		{
			id = "ant_blue"
		});
		t.addTask("ant_blue_move");
		add(new ActorJob
		{
			id = "ant_green"
		});
		t.addTask("ant_green_move");
		add(new ActorJob
		{
			id = "skeleton_job"
		});
		t.addTask("skeleton_move");
		t.addTask("make_decision");
	}
}
