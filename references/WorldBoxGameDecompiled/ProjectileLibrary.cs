using System;

public class ProjectileLibrary : AssetLibrary<ProjectileAsset>
{
	public override void init()
	{
		base.init();
		add(new ProjectileAsset
		{
			id = "arrow",
			speed = 20f,
			texture = "arrow",
			texture_shadow = "shadows/projectiles/shadow_arrow",
			sound_launch = "event:/SFX/WEAPONS/WeaponStartArrow",
			sound_impact = "event:/SFX/HIT/HitGeneric",
			can_be_left_on_ground = true,
			can_be_blocked = true
		});
		add(new ProjectileAsset
		{
			id = "snowball",
			texture = "snowball",
			texture_shadow = "shadows/projectiles/shadow_snowball",
			sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
			sound_impact = "event:/SFX/WEAPONS/WeaponSnowballLand",
			hit_freeze = true,
			can_be_blocked = true
		});
		add(new ProjectileAsset
		{
			id = "firebomb",
			texture = "firebomb",
			texture_shadow = "shadows/projectiles/shadow_ball",
			speed = 12f,
			trigger_on_collision = true,
			terraform_option = "demon_fireball",
			end_effect = "fx_firebomb_explosion",
			hit_shake = true,
			shake_duration = 0.01f,
			shake_interval = 0.01f,
			shake_intensity = 0.25f,
			scale_start = 0.075f,
			scale_target = 0.075f,
			draw_light_area = true,
			sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
			terraform_range = 2
		});
		add(new ProjectileAsset
		{
			id = "torch",
			texture = "pr_torch",
			texture_shadow = "shadows/projectiles/shadow_ball",
			terraform_option = "torch",
			scale_start = 0.075f,
			scale_target = 0.075f,
			speed = 15f,
			terraform_range = 1,
			trigger_on_collision = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
			sound_impact = "event:/SFX/WEAPONS/WeaponTorchLand"
		});
		add(new ProjectileAsset
		{
			id = "red_orb",
			speed_random = 5f,
			texture = "pr_red_orb",
			trigger_on_collision = true,
			scale_start = 0.035f,
			scale_target = 0.2f,
			sound_launch = "event:/SFX/WEAPONS/WeaponRedOrbStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponRedOrbLand",
			draw_light_area = true
		});
		ProjectileAsset projectileAsset = t;
		projectileAsset.world_actions = (AttackAction)Delegate.Combine(projectileAsset.world_actions, new AttackAction(ActionLibrary.burnTile));
		add(new ProjectileAsset
		{
			id = "freeze_orb",
			texture = "pr_freeze_orb",
			trigger_on_collision = true,
			scale_start = 0.035f,
			scale_target = 0.2f,
			sound_launch = "event:/SFX/WEAPONS/WeaponFreezeOrbStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponFreezeOrbLand",
			draw_light_area = true
		});
		add(new ProjectileAsset
		{
			id = "green_orb",
			texture = "pr_green_orb",
			trigger_on_collision = true,
			scale_start = 0.035f,
			scale_target = 0.2f,
			sound_launch = "event:/SFX/WEAPONS/WeaponGreenOrbStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponGreenOrbLand",
			draw_light_area = true
		});
		add(new ProjectileAsset
		{
			id = "bone",
			speed = 15f,
			texture = "pr_bone",
			texture_shadow = "shadows/projectiles/shadow_ball",
			scale_start = 0.035f,
			scale_target = 0.05f,
			sound_launch = "event:/SFX/WEAPONS/WeaponBoneProjectileStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponBoneProjectileLand",
			can_be_blocked = true
		});
		add(new ProjectileAsset
		{
			id = "plasma_ball",
			speed = 40f,
			texture = "pr_plasma_ball",
			trigger_on_collision = true,
			trail_effect_enabled = true,
			trail_effect_id = "fx_plasma_trail",
			trail_effect_scale = 0.1f,
			trail_effect_timer = 0.1f,
			look_at_target = true,
			terraform_option = "plasma_ball",
			end_effect = "fx_plasma_ball_explosion",
			terraform_range = 2,
			draw_light_area = true,
			draw_light_size = 0.1f,
			sound_launch = "event:/SFX/WEAPONS/WeaponPlasmaBallStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponPlasmaBallLand",
			scale_start = 0.035f,
			scale_target = 0.2f,
			mass = 1f
		});
		add(new ProjectileAsset
		{
			id = "skull",
			speed = 17f,
			texture = "pr_skull",
			animation_speed = 10f,
			trigger_on_collision = true,
			look_at_target = true,
			end_effect = "fx_cast_ground_purple",
			end_effect_scale = 0.12f,
			texture_shadow = "shadows/projectiles/shadow_ball",
			scale_start = 0.035f,
			scale_target = 0.08f,
			sound_launch = "event:/SFX/WEAPONS/WeaponBoneProjectileStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponBoneProjectileLand"
		});
		add(new ProjectileAsset
		{
			id = "fireball",
			speed = 25f,
			texture = "fireball",
			trail_effect_enabled = true,
			trigger_on_collision = true,
			texture_shadow = "shadows/projectiles/shadow_ball",
			terraform_option = "demon_fireball",
			end_effect = "fx_fireball_explosion",
			terraform_range = 3,
			draw_light_area = true,
			draw_light_size = 0.1f,
			sound_launch = "event:/SFX/WEAPONS/WeaponFireballStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponFireballLand",
			scale_start = 0.035f,
			scale_target = 0.2f
		});
		add(new ProjectileAsset
		{
			id = "madness_ball",
			speed = 30f,
			trigger_on_collision = true,
			texture = "madness_ball",
			terraform_option = "madness_ball",
			texture_shadow = "shadows/projectiles/shadow_ball",
			terraform_range = 3,
			scale_start = 0.035f,
			scale_target = 0.2f,
			draw_light_area = true,
			sound_launch = "event:/SFX/WEAPONS/WeaponMadnessBallStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponMadnessBallLand"
		});
		add(new ProjectileAsset
		{
			id = "rock",
			texture = "rock",
			texture_shadow = "shadows/projectiles/shadow_snowball",
			sound_launch = "event:/SFX/WEAPONS/WeaponStartThrow",
			sound_impact = "event:/SFX/WEAPONS/WeaponRockLand",
			can_be_left_on_ground = true,
			scale_start = 0.05f,
			scale_target = 0.09f,
			can_be_blocked = true
		});
		add(new ProjectileAsset
		{
			id = "shotgun_bullet",
			speed = 30f,
			speed_random = 7f,
			texture = "shotgun_bullet",
			look_at_target = true,
			draw_light_area = true,
			terraform_option = "assimilator_bullet",
			sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
			scale_start = 0.035f,
			scale_target = 0.08f,
			can_be_blocked = true
		});
		add(new ProjectileAsset
		{
			id = "cannonball",
			speed = 25f,
			texture = "cannonball",
			texture_shadow = "shadows/projectiles/shadow_ball",
			terraform_option = "cannonball",
			terraform_range = 2,
			sound_launch = "event:/SFX/WEAPONS/WeaponShotgunStart",
			sound_impact = "event:/SFX/WEAPONS/WeaponShotgunLand",
			end_effect = "fx_firebomb_explosion",
			scale_start = 0.1f,
			scale_target = 0.1f
		});
		add(new ProjectileAsset
		{
			id = "acid_ball",
			speed = 25f,
			texture = "acid_ball",
			texture_shadow = "shadows/projectiles/shadow_ball",
			terraform_option = "acid_ball",
			terraform_range = 2,
			sound_impact = "event:/SFX/DROPS/DropAcid",
			end_effect = "fx_cast_top_green",
			scale_start = 0.1f,
			scale_target = 0.1f
		});
	}
}
