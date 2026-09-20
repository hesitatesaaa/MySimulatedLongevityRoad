public class TerraformLibrary : AssetLibrary<TerraformOptions>
{
	public static TerraformOptions nothing;

	public static TerraformOptions remove_lava;

	public static TerraformOptions lava_damage;

	public static TerraformOptions destroy_no_flash;

	public static TerraformOptions remove;

	public static TerraformOptions draw;

	public static TerraformOptions water_fill;

	public static TerraformOptions grey_goo;

	public static TerraformOptions flash;

	public static TerraformOptions road;

	public static TerraformOptions tumor_attack;

	public static TerraformOptions destroy;

	public static TerraformOptions destroy_life;

	public static TerraformOptions earthquake;

	public static TerraformOptions earthquake_disaster;

	public static TerraformOptions atomic_bomb;

	public static TerraformOptions crabzilla_bomb;

	public static TerraformOptions bomb;

	public static TerraformOptions czar_bomba;

	public override void init()
	{
		base.init();
		tumor_attack = add(new TerraformOptions
		{
			id = "tumor_attack",
			flash = true,
			destroy_buildings = true,
			remove_frozen = true,
			ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("tumor")
		});
		destroy = add(new TerraformOptions
		{
			id = "destroy",
			flash = true,
			destroy_buildings = true
		});
		destroy_life = add(new TerraformOptions
		{
			id = "destroy_life",
			destroy_only = AssetLibrary<TerraformOptions>.l<string>("tumor", "super_pumpkin", "biomass"),
			destroy_buildings = true,
			flash = true,
			remove_borders = true,
			remove_tornado = true
		});
		earthquake = add(new TerraformOptions
		{
			id = "earthquake",
			remove_lava = true,
			make_ruins = true,
			remove_top_tile = true,
			remove_roads = true,
			remove_frozen = true,
			remove_water = true
		});
		earthquake_disaster = add(new TerraformOptions
		{
			id = "earthquake_disaster",
			remove_lava = true,
			make_ruins = true,
			remove_top_tile = true,
			remove_roads = true,
			remove_frozen = true,
			remove_water = true
		});
		road = add(new TerraformOptions
		{
			id = "road",
			flash = true,
			destroy_buildings = true,
			remove_frozen = true,
			destroy_only = AssetLibrary<TerraformOptions>.l<string>("nature")
		});
		flash = add(new TerraformOptions
		{
			id = "flash",
			flash = true
		});
		grey_goo = add(new TerraformOptions
		{
			id = "grey_goo",
			destroy_buildings = true,
			flash = true,
			remove_borders = true,
			remove_frozen = true
		});
		remove = add(new TerraformOptions
		{
			id = "remove",
			flash = true,
			destroy_buildings = true,
			remove_frozen = true
		});
		draw = add(new TerraformOptions
		{
			id = "draw",
			flash = true,
			remove_frozen = true
		});
		water_fill = add(new TerraformOptions
		{
			id = "water_fill",
			flash = true,
			destroy_buildings = true,
			remove_frozen = true,
			remove_fire = true,
			ignore_buildings = AssetLibrary<TerraformOptions>.l<string>("geyser", "volcano", "geyser_acid")
		});
		destroy_no_flash = add(new TerraformOptions
		{
			id = "destroy_no_flash",
			remove_burned = true,
			destroy_buildings = true
		});
		nothing = add(new TerraformOptions
		{
			id = "nothing"
		});
		remove_lava = add(new TerraformOptions
		{
			id = "remove_lava",
			remove_lava = true
		});
		lava_damage = add(new TerraformOptions
		{
			id = "lava_damage",
			flash = true,
			damage_buildings = true,
			damage = 100,
			remove_frozen = true
		});
		add(new TerraformOptions
		{
			id = "grenade",
			flash = true,
			damage_buildings = true,
			damage = 50,
			apply_force = true,
			explode_and_set_random_fire = true,
			explode_tile = true,
			explosion_pixel_effect = true,
			explode_strength = 1,
			shake = true,
			remove_tornado = true,
			remove_frozen = true,
			attack_type = AttackType.Explosion
		});
		add(new TerraformOptions
		{
			id = "lightning_power",
			flash = true,
			lightning_effect = true,
			add_burned = true,
			add_heat = 9,
			damage = 77,
			set_fire = true,
			apply_force = true,
			force_power = 2.5f,
			shake = true,
			shake_intensity = 2f,
			remove_frozen = true
		});
		add(new TerraformOptions
		{
			id = "lightning_normal",
			flash = true,
			lightning_effect = true,
			add_burned = true,
			damage = 47,
			set_fire = true,
			apply_force = true,
			force_power = 2.5f,
			remove_frozen = true
		});
		add(new TerraformOptions
		{
			id = "madness_ball",
			apply_force = true,
			force_power = 1.6f,
			add_trait = "madness",
			flash = true,
			remove_frozen = true
		});
		add(new TerraformOptions
		{
			id = "ufo_attack",
			flash = true,
			add_burned = true,
			damage = 50,
			set_fire = true,
			apply_force = true,
			shake = true,
			damage_buildings = true,
			remove_frozen = true
		});
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("aliens");
		t.shake_intensity = 0.1f;
		add(new TerraformOptions
		{
			id = "dragon_attack",
			flash = true,
			add_burned = true,
			force_power = 0.5f,
			damage = 25,
			set_fire = true,
			apply_force = true,
			shake = true,
			damage_buildings = true,
			remove_frozen = true,
			attack_type = AttackType.Fire
		});
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("dragons");
		t.shake_intensity = 0.1f;
		add(new TerraformOptions
		{
			id = "zombie_dragon_attack",
			flash = true,
			force_power = 0.5f,
			damage = 25,
			set_fire = false,
			apply_force = true,
			shake = true,
			damage_buildings = true,
			remove_frozen = true,
			attack_type = AttackType.Acid
		});
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("undead");
		t.shake_intensity = 0.1f;
		clone("ufo_explosion", "grenade");
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("aliens");
		bomb = clone("bomb", "grenade");
		t.damage = 100;
		t.explode_strength = 3;
		clone("meteorite", "grenade");
		t.damage = 1000;
		t.explode_strength = 1;
		clone("meteorite_disaster", "grenade");
		t.damage = 500;
		t.explode_strength = 1;
		clone("crab_bomb", "grenade");
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("crabzilla", "crab");
		t.damage_buildings = true;
		t.damage = 30;
		t.set_fire = true;
		t.shake = false;
		atomic_bomb = clone("atomic_bomb", "bomb");
		t.damage = 10000;
		t.explode_strength = 1;
		t.transform_to_wasteland = true;
		t.applies_to_high_flyers = true;
		t.shake = false;
		t.bomb_action = NukeFlash.atomic_bomb_action;
		crabzilla_bomb = clone("crabzilla_bomb", "bomb");
		t.damage = 10000;
		t.explode_strength = 1;
		t.applies_to_high_flyers = true;
		t.bomb_action = NukeFlash.crabzilla_bomb_action;
		czar_bomba = clone("czar_bomba", "bomb");
		t.damage = 10000;
		t.transform_to_wasteland = true;
		t.explode_strength = 4;
		t.applies_to_high_flyers = true;
		t.shake = false;
		t.bomb_action = NukeFlash.czar_bomba_action;
		add(new TerraformOptions
		{
			id = "crab_step",
			flash = true,
			damage_buildings = true,
			damage = 10,
			apply_force = true,
			explode_tile = true,
			explosion_pixel_effect = false,
			explode_strength = 1,
			shake = true,
			shake_intensity = 0.1f,
			remove_frozen = true
		});
		clone("crab_laser", "bomb");
		t.damage = 200;
		t.explode_strength = 1;
		t.shake_intensity = 1f;
		t.attack_type = AttackType.Fire;
		clone("santa_bomb", "bomb");
		clone("demon_fireball", "grenade");
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("demon");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 30;
		t.set_fire = true;
		clone("plasma_ball", "grenade");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 30;
		clone("torch", "grenade");
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("demon");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 25;
		t.set_fire = true;
		t.attack_type = AttackType.Fire;
		clone("assimilator_bullet", "flash");
		t.remove_frozen = true;
		t.ignore_kingdoms = AssetLibrary<TerraformOptions>.a<string>("assimilators");
		t.remove_frozen = true;
		clone("cannonball", "grenade");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 0;
		t.apply_force = true;
		t.force_power = 2f;
		clone("acid", "grenade");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 0;
		t.explode_strength = 2;
		t.apply_force = true;
		t.force_power = 0.5f;
		t.set_fire = false;
		t.flash = true;
		t.remove_frozen = true;
		t.explode_and_set_random_fire = false;
		t.attack_type = AttackType.Acid;
		clone("acid_ball", "grenade");
		t.shake = false;
		t.damage_buildings = true;
		t.damage = 0;
		t.apply_force = true;
		t.force_power = 0.5f;
		t.set_fire = false;
		t.flash = true;
		t.remove_frozen = true;
		t.explode_and_set_random_fire = false;
		t.attack_type = AttackType.Acid;
	}
}
