namespace ai.behaviours;

public class BehShakeBuilding : BehaviourActionActor
{
	private float _shake_time;

	private float _shake_intensity_x;

	private float _shake_intensity_y;

	public BehShakeBuilding(float pShakeParam = 0.7f, float pIntensityX = 0.04f, float pIntensityY = 0.04f)
	{
		_shake_time = pShakeParam;
		_shake_intensity_x = pIntensityX;
		_shake_intensity_y = pIntensityY;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.beh_building_target.startShake(_shake_time, _shake_intensity_x, _shake_intensity_y);
		return BehResult.Continue;
	}
}
