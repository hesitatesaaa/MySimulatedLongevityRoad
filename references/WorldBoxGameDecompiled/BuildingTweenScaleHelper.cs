using System;

public class BuildingTweenScaleHelper
{
	internal bool active;

	internal float scale_start;

	internal float scale_target = 1f;

	internal double scale_time;

	internal float scale_duration = 1f;

	internal float scale_last_priority;

	internal bool scale_use_x;

	internal Action scale_final_action;

	internal EasingFunction scale_ease;

	public float angle_target;

	public float angle_duration;

	public float angle_time;

	internal Action angle_final_action;

	public void doRotateTween(float pTargetAngle, float pDuration, Action pAction)
	{
		angle_target = pTargetAngle;
		angle_duration = pDuration;
		angle_final_action = pAction;
		angle_time = 0f;
	}

	public void reset()
	{
		active = false;
		scale_start = 0f;
		scale_target = 1f;
		scale_time = 0.0;
		scale_duration = 1f;
		scale_last_priority = 0f;
		scale_use_x = false;
		scale_final_action = null;
		scale_ease = null;
		angle_target = 0f;
		angle_duration = 0f;
		angle_time = 0f;
		angle_final_action = null;
	}
}
