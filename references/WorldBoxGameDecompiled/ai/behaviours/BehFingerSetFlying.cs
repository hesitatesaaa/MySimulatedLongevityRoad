namespace ai.behaviours;

public class BehFingerSetFlying : BehFinger
{
	private bool _flying;

	private float _height_target = -1f;

	public BehFingerSetFlying(bool pFlying, float pHeightTarget = -1f)
	{
		_flying = pFlying;
		if (pHeightTarget > -1f)
		{
			_height_target = pHeightTarget;
		}
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.setFlying(_flying);
		if (_flying)
		{
			if (_height_target > -1f)
			{
				finger.flying_target = _height_target;
			}
			else
			{
				finger.flying_target = Randy.randomFloat(5f, 13f);
			}
		}
		else
		{
			finger.flying_target = 0.3f;
		}
		return BehResult.Continue;
	}
}
