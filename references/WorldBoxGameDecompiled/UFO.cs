using System.Collections.Generic;
using UnityEngine;

public class UFO : BaseActorComponent
{
	private SpriteRenderer beamRnd;

	internal SpriteAnimation beamAnim;

	internal HashSet<Actor> aggroTargets = new HashSet<Actor>();

	internal override void create(Actor pActor)
	{
		base.create(pActor);
		beamRnd = ((Component)((Component)this).transform.Find("Beam")).GetComponent<SpriteRenderer>();
		beamAnim = ((Component)((Component)this).transform.Find("Beam")).GetComponent<SpriteAnimation>();
		actor.position_height = actor.asset.default_height;
		actor.getSpriteAnimation().forceUpdateFrame();
		hideBeam();
	}

	public static bool click(BaseSimObject pTarget, WorldTile pTile = null)
	{
		Actor a = pTarget.a;
		if (a.ai.task?.id == "ufo_attack")
		{
			return false;
		}
		a.cancelAllBeh();
		a.setTask("ufo_attack");
		return true;
	}

	internal void startBeam()
	{
		beamAnim.stopAt(0, pNow: true);
		beamAnim.isOn = true;
		((Renderer)beamRnd).enabled = true;
		MusicBox.playSound(actor.asset.sound_attack, actor.current_tile);
	}

	public override void update(float pElapsed)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		beamAnim.update(pElapsed);
		if (beamAnim.isOn)
		{
			World.world.stack_effects.light_blobs.Add(new LightBlobData
			{
				position = new Vector2(actor.current_position.x, actor.current_position.y),
				radius = 1f
			});
		}
		if (actor.stats["speed"] < 50f && actor.ai.task?.id == "ufo_fly")
		{
			actor.stats["speed"] += pElapsed * 10f;
		}
		if (!World.world.isPaused() && actor.isAlive() && actor.position_height < actor.asset.default_height)
		{
			actor.position_height += actor.stats["speed"] * pElapsed * 0.1f;
		}
	}

	internal void hideBeam()
	{
		beamAnim.isOn = false;
		((Renderer)beamRnd).enabled = false;
	}

	internal static bool getHit(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
	{
		Actor a = pSelf.a;
		UFO actorComponent = a.getActorComponent<UFO>();
		actorComponent.aggroTargets.RemoveWhere((Actor tAttacker) => tAttacker == null || !tAttacker.isAlive());
		if (pAttackedBy != null && pAttackedBy.isActor())
		{
			actorComponent.aggroTargets.Add(pAttackedBy?.a);
		}
		string text = a.ai.task?.id;
		if (text == "ufo_fly" || text == "ufo_explore")
		{
			a.cancelAllBeh();
			if (pAttackedBy == null)
			{
				a.setTask("ufo_flee");
			}
			else
			{
				a.setTask("ufo_hit");
			}
		}
		return true;
	}

	public static bool ufoFall(BaseSimObject pTarget, WorldTile pTile, float pElapsed)
	{
		pTarget.a.updateFall();
		if (pTarget.a.position_height == 0f)
		{
			WorldTile tile = World.world.GetTile((int)pTarget.a.current_position.x, (int)pTarget.a.current_position.y);
			if (tile != null)
			{
				MapAction.damageWorld(tile, 5, AssetManager.terraform.get("ufo_explosion"), pTarget);
				EffectsLibrary.spawnAtTileRandomScale("fx_explosion_ufo", tile, 0.45f, 0.6f);
			}
			pTarget.a.dieAndDestroy(AttackType.Other);
		}
		return true;
	}
}
