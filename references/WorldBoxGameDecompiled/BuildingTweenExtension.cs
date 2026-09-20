using System;
using UnityEngine;

public static class BuildingTweenExtension
{
	internal static void checkTweens(this Building pBuilding)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		switch (pBuilding.animation_state)
		{
		case BuildingAnimationState.OnRuin:
			pBuilding.setScaleTween(1f, 0.1f, 0f, pBuilding.completeMakingRuin, new EasingFunction(iTween.easeInCubic));
			break;
		case BuildingAnimationState.OnRemove:
		{
			EasingFunction pEase = new EasingFunction(iTween.easeInBack);
			if (pBuilding.chopped)
			{
				pEase = new EasingFunction(iTween.easeInCubic);
				pBuilding.scale_helper.scale_use_x = true;
			}
			pBuilding.setScaleTween(1f, 0.5f, 0f, pBuilding.removeBuildingFinal, pEase, 1);
			if (pBuilding.asset.city_building)
			{
				pBuilding.startShake(0.5f);
			}
			break;
		}
		}
	}

	internal static void setScaleTween(this Building pBuilding, float pFrom = 0f, float pDuration = 0.2f, float pTarget = 1f, Action pActionOnComplete = null, EasingFunction pEase = null, int pPriority = 0)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		BuildingTweenScaleHelper scale_helper = pBuilding.scale_helper;
		if (!scale_helper.active || scale_helper.scale_final_action == null || !(scale_helper.scale_last_priority >= (float)pPriority))
		{
			if (pEase == null)
			{
				pEase = new EasingFunction(iTween.easeOutBack);
			}
			scale_helper.active = true;
			scale_helper.scale_start = pFrom;
			scale_helper.scale_target = pTarget;
			scale_helper.scale_time = World.world.getCurSessionTime() + (double)pDuration;
			scale_helper.scale_duration = pDuration;
			scale_helper.scale_final_action = pActionOnComplete;
			scale_helper.scale_ease = pEase;
			if (scale_helper.scale_use_x)
			{
				pBuilding.current_scale.x = pBuilding.asset.scale_base.x * pFrom;
			}
			else
			{
				pBuilding.current_scale.y = pBuilding.asset.scale_base.y * pFrom;
			}
			pBuilding.batch.c_scale.Add(pBuilding);
		}
	}

	public static void checkFinalAction(this Building pBuilding)
	{
		pBuilding.scale_helper.scale_final_action?.Invoke();
		pBuilding.scale_helper.scale_final_action = null;
		pBuilding.scale_helper.angle_final_action?.Invoke();
		pBuilding.scale_helper.angle_final_action = null;
	}

	internal static void finishScaleTween(this Building pBuilding)
	{
		pBuilding.setAnimationState(BuildingAnimationState.Normal);
		BuildingTweenScaleHelper scale_helper = pBuilding.scale_helper;
		scale_helper.scale_time = World.world.getCurSessionTime() + (double)scale_helper.scale_duration;
	}

	internal static void updateAngle(this Building pBuilding, float pElapsed)
	{
		if (pBuilding.current_rotation.z != pBuilding.scale_helper.angle_target)
		{
			BuildingTweenScaleHelper scale_helper = pBuilding.scale_helper;
			scale_helper.angle_time += pElapsed;
			if (scale_helper.angle_time >= 1f)
			{
				scale_helper.angle_time = 1f;
				pBuilding.batch.c_angle.Remove(pBuilding);
				pBuilding.batch.actions_to_run.Add(pBuilding.checkFinalAction);
			}
			float num = iTween.easeInExpo(0f, 1f, scale_helper.angle_time);
			((Vector3)(ref pBuilding.current_rotation)).Set(0f, 0f, num * pBuilding.scale_helper.angle_target);
		}
	}

	internal static void updateScale(this Building pBuilding)
	{
		if (pBuilding.scale_helper.active)
		{
			BuildingTweenScaleHelper scale_helper = pBuilding.scale_helper;
			double num = scale_helper.scale_time - World.world.getCurSessionTime();
			float num2 = 1f;
			if (num <= 0.0)
			{
				scale_helper.scale_time = World.world.getCurSessionTime() + (double)scale_helper.scale_duration;
				scale_helper.active = false;
				pBuilding.batch.actions_to_run.Add(pBuilding.checkFinalAction);
				pBuilding.batch.c_scale.Remove(pBuilding);
				num2 = scale_helper.scale_target;
			}
			else
			{
				float num3 = (float)(((double)scale_helper.scale_duration - num) / (double)scale_helper.scale_duration);
				num2 = scale_helper.scale_ease.Invoke(scale_helper.scale_start, scale_helper.scale_target, num3);
			}
			if (scale_helper.scale_use_x)
			{
				pBuilding.current_scale.x = pBuilding.asset.scale_base.x * num2;
			}
			else
			{
				pBuilding.current_scale.y = pBuilding.asset.scale_base.y * num2;
			}
		}
	}
}
