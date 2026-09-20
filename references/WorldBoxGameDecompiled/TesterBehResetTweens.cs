using ai.behaviours;

public class TesterBehResetTweens : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		Tooltip.tweenTime = 0f;
		PremiumUnlockAnimation.scaleTime = 0f;
		PremiumUnlockAnimation.delayTime = 0f;
		PowersTab.scale_time = 0f;
		PowersTab.buttonScaleTime = 0f;
		ButtonAnimation.scaleTime = 0f;
		ButtonResource.scaleTime = 0f;
		UiButtonHoverAnimation.scaleTime = 0f;
		return BehResult.Continue;
	}
}
