using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;

public class GodFinger : BaseActorComponent
{
	public const float FLYING_SPEED = 8f;

	public const int MAX_TARGET_TILES = 1200;

	internal GodPower god_power;

	internal string brush;

	internal float flying_target = 8f;

	private float _rotate_wiggle = 30f;

	internal static string[] power_over_water;

	internal static string[] power_over_ground;

	internal HashSet<WorldTile> target_tiles = new HashSet<WorldTile>(1800);

	internal FingerTarget finger_target;

	private SpriteAnimation fingerTip;

	internal Color debug_color;

	private static Color[] _random_colors;

	internal bool is_drawing
	{
		get
		{
			if (actor.ai.hasTask())
			{
				return (actor.ai.action as BehFinger)?.drawing_action ?? false;
			}
			return false;
		}
	}

	internal bool drawing_over_water => finger_target == FingerTarget.Water;

	internal bool drawing_over_ground => finger_target == FingerTarget.Ground;

	internal override void create(Actor pActor)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		base.create(pActor);
		debug_color = _random_colors.GetRandom();
		((Object)((Component)this).gameObject).name = "GF " + actor.getID();
		fingerTip = ((Component)((Component)this).transform.Find("Tip")).gameObject.GetComponent<SpriteAnimation>();
		((Component)fingerTip).gameObject.SetActive(false);
		actor.target_angle = Vector3.zero;
		actor.setFlying(pVal: true);
		actor.position_height = 8f;
	}

	internal void lightAction()
	{
		AchievementLibrary.god_finger_lightning.check();
	}

	public override void update(float pElapsed)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (!actor.isAlive() || World.world.isPaused())
		{
			return;
		}
		bool flag = is_drawing;
		bool flag2 = !flag && flying_target < 2f;
		if (flag)
		{
			actor.target_angle.z = Mathf.Clamp(actor.target_angle.z, 25f, 35f);
			if (actor.target_angle.z < _rotate_wiggle)
			{
				actor.target_angle.z += 100f * pElapsed;
			}
			else if (actor.target_angle.z > _rotate_wiggle)
			{
				actor.target_angle.z -= 100f * pElapsed;
			}
			else
			{
				_rotate_wiggle = Randy.randomInt(25, 35);
			}
			actor.rotation_cooldown = 300f;
		}
		else if (flag2)
		{
			if (actor.target_angle.z < 30f)
			{
				actor.target_angle.z += 100f * pElapsed;
			}
			actor.rotation_cooldown = 300f;
		}
		else
		{
			actor.rotation_cooldown = 0f;
		}
		if (flying_target != actor.position_height)
		{
			actor.position_height = Mathf.MoveTowards(actor.position_height, flying_target, pElapsed * 8f);
		}
		((Component)fingerTip).gameObject.SetActive(flag);
		if (flag)
		{
			fingerTip.update(pElapsed);
			if (isInMapBounds(Vector2.op_Implicit(actor.current_position)))
			{
				drawOnTile(actor.current_tile);
			}
		}
	}

	private bool isInMapBounds(Vector3 pPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (pPos.x > 0f && pPos.y > 0f && pPos.x < (float)MapBox.width)
		{
			return pPos.y < (float)MapBox.height;
		}
		return false;
	}

	public void drawOnTile(WorldTile pTile)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		World.world.conway_layer.checkKillRange(pTile.pos, 2);
		string current_brush = Config.current_brush;
		Config.current_brush = brush;
		if (god_power.click_power_action != null || god_power.click_power_brush_action != null)
		{
			if (god_power.click_power_brush_action != null)
			{
				god_power.click_power_brush_action(pTile, god_power);
			}
			else if (god_power.click_power_action != null)
			{
				god_power.click_power_action(pTile, god_power);
			}
		}
		if (god_power.click_action != null || god_power.click_brush_action != null)
		{
			if (god_power.click_brush_action != null)
			{
				god_power.click_brush_action(pTile, god_power.id);
			}
			else if (god_power.click_action != null)
			{
				god_power.click_action(pTile, god_power.id);
			}
		}
		World.world.loopWithBrush(pTile, Config.current_brush_data, clearTargets, "god_finger");
		World.world.loopWithBrush(pTile, Brush.get(2), fingerTile, "god_finger");
		Config.current_brush = current_brush;
	}

	public bool clearTargets(WorldTile pTile, string pPowerID)
	{
		target_tiles.Remove(pTile);
		return true;
	}

	public bool fingerTile(WorldTile pTile, string pPowerID)
	{
		pTile.doUnits(delegate(Actor pActor)
		{
			if (!pActor.asset.flag_finger && pActor.asset.can_be_killed_by_stuff)
			{
				pActor.getHitFullHealth(AttackType.Gravity);
			}
		});
		return true;
	}

	public override void Dispose()
	{
		target_tiles.Clear();
		finger_target = FingerTarget.None;
		base.Dispose();
	}

	internal static bool deathFlip(BaseSimObject pTarget, WorldTile pTile, float pElapsed)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Actor a = pTarget.a;
		if (a.isFalling())
		{
			a.updateFall();
			return true;
		}
		if (a.target_angle.z < 90f)
		{
			a.target_angle.z = Mathf.Lerp(a.target_angle.z, 90f, pElapsed * 4f);
			if (a.target_angle.z > 90f)
			{
				a.target_angle.z = 90f;
			}
			if (!a.is_visible)
			{
				a.updateRotation();
			}
			if (Mathf.Abs(a.current_rotation.z) >= 89f)
			{
				a.dieAndDestroy(AttackType.None);
				return true;
			}
		}
		a.updateDeadBlackAnimation(pElapsed);
		return true;
	}

	public static void debug_trail(GodFinger pFinger)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.ShowGodFingerTargetting) || !MapBox.isRenderGameplay() || !pFinger.actor.is_visible)
		{
			return;
		}
		AiSystemActor ai = pFinger.actor.ai;
		if (ai.hasTask())
		{
			Color color_white = Toolbox.color_white;
			switch (ai.task.id)
			{
			default:
				return;
			case "godfinger_move":
				color_white = Toolbox.color_blue;
				break;
			case "godfinger_find_target":
				color_white = Toolbox.color_red;
				break;
			case "godfinger_random_fun_move":
				color_white = Toolbox.color_green;
				break;
			case "godfinger_circle_move":
				color_white = Toolbox.color_purple;
				break;
			case "godfinger_circle_move_big":
				color_white = Toolbox.color_yellow;
				break;
			case "godfinger_circle_move_small":
				color_white = Color32.op_Implicit(Toolbox.color_fire);
				break;
			}
			BaseEffect baseEffect = EffectsLibrary.spawn("fx_weapon_particle", null, null, null, 0f, ((Component)pFinger.fingerTip).transform.position.x, ((Component)pFinger.fingerTip).transform.position.y);
			if ((Object)(object)baseEffect != (Object)null)
			{
				((StatusParticle)baseEffect).spawnParticle(((Component)baseEffect).transform.position, color_white);
			}
		}
	}

	static GodFinger()
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		power_over_water = Toolbox.splitStringIntoArray("tile_high_soil#10", "tile_soil#10", "tile_hills", "tile_mountains", "tile_summit", "shovel_plus");
		power_over_ground = Toolbox.splitStringIntoArray("seeds_candy", "seeds_corrupted", "seeds_crystal", "seeds_desert", "seeds_enchanted", "seeds_grass", "seeds_infernal", "seeds_jungle", "seeds_lemon", "seeds_mushroom", "seeds_permafrost", "seeds_savanna", "seeds_swamp", "seeds_birch", "seeds_maple", "seeds_flower", "seeds_garlic", "seeds_rocklands", "seeds_celestial", "seeds_singularity", "seeds_clover", "seeds_paradox", "fertilizer_plants#4", "fertilizer_trees#4");
		_random_colors = (Color[])(object)new Color[20]
		{
			Toolbox.color_green,
			Toolbox.color_red,
			Toolbox.color_blue,
			Toolbox.color_yellow,
			Toolbox.color_purple,
			Color32.op_Implicit(Toolbox.color_fire),
			Color32.op_Implicit(Toolbox.color_phenotype_green_0),
			Color32.op_Implicit(Toolbox.color_phenotype_green_1),
			Color32.op_Implicit(Toolbox.color_phenotype_green_2),
			Color32.op_Implicit(Toolbox.color_phenotype_green_3),
			Color32.op_Implicit(Toolbox.color_magenta_0),
			Color32.op_Implicit(Toolbox.color_magenta_1),
			Color32.op_Implicit(Toolbox.color_magenta_2),
			Color32.op_Implicit(Toolbox.color_magenta_3),
			Color32.op_Implicit(Toolbox.color_magenta_4),
			Color32.op_Implicit(Toolbox.color_teal_0),
			Color32.op_Implicit(Toolbox.color_teal_1),
			Color32.op_Implicit(Toolbox.color_teal_2),
			Color32.op_Implicit(Toolbox.color_teal_3),
			Color32.op_Implicit(Toolbox.color_teal_4)
		};
	}
}
