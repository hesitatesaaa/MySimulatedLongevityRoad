using System.Collections.Generic;
using UnityEngine;

public class EffectInfinityCoin : BaseEffect
{
	private static List<Actor> _temp_list = new List<Actor>();

	private bool used;

	internal override void create()
	{
		base.create();
	}

	internal override void spawnOnTile(WorldTile pTile)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		prepare(Vector2.op_Implicit(new Vector3(pTile.posV3.x, pTile.posV3.y - 1f)), 0.25f);
	}

	internal override void prepare(Vector2 pVector, float pScale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.prepare(pVector, pScale);
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.z = -2f;
		current_position = Vector2.op_Implicit(localPosition);
		((Component)this).transform.localPosition = localPosition;
		used = false;
		World.world.startShake(0.1f, 0.02f, 3f);
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (sprite_animation.currentFrameIndex >= 32 && !used)
		{
			World.world.startShake(0.2f, 0.01f, 3f);
			used = true;
			Vector3 localPosition = ((Component)this).transform.localPosition;
			localPosition.y += 2f;
			BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_boulder_impact", localPosition, ((Component)this).transform.localScale.x);
			if ((Object)(object)baseEffect != (Object)null)
			{
				localPosition = ((Component)baseEffect).transform.localPosition;
				localPosition.z = -1f;
				((Component)baseEffect).transform.localPosition = localPosition;
			}
			EffectsLibrary.spawnExplosionWave(localPosition, 5f);
			doAction();
		}
	}

	private void doAction()
	{
		int num = 0;
		int num2 = 0;
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.isAlive() && !actor.isFavorite() && !actor.asset.ignored_by_infinity_coin)
			{
				num++;
			}
		}
		num2 = ((num % 2 != 0) ? (num / 2 + 1) : (num / 2));
		int num3 = 0;
		_temp_list.AddRange(World.world.units);
		for (int j = 0; j < _temp_list.Count; j++)
		{
			_temp_list.ShuffleOne(j);
			Actor actor2 = _temp_list[j];
			if (num2 == 0)
			{
				break;
			}
			if (actor2.isAlive() && !actor2.isFavorite() && !actor2.asset.ignored_by_infinity_coin && !actor2.is_invincible)
			{
				num3++;
				num2--;
				actor2.getHitFullHealth(AttackType.Divine);
			}
		}
		WorldTip.addWordReplacement("$removed$", num3.ToString());
		WorldTip.showNow("infinity_coin_used", pTranslate: true, "top");
		_temp_list.Clear();
	}
}
