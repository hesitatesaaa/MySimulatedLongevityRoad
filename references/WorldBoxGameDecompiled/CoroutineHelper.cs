using UnityEngine;

public static class CoroutineHelper
{
	public static WaitForSecondsRealtime wait_for_0_5_s
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new WaitForSecondsRealtime(0.5f);
		}
	}

	public static WaitForSecondsRealtime wait_for_0_01_s
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new WaitForSecondsRealtime(0.01f);
		}
	}

	public static WaitForSecondsRealtime wait_for_0_05_s
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new WaitForSecondsRealtime(0.05f);
		}
	}

	public static WaitForSecondsRealtime wait_for_0_025_s
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			return new WaitForSecondsRealtime(0.025f);
		}
	}

	public static YieldInstruction wait_for_end_of_frame
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			return (YieldInstruction)new WaitForEndOfFrame();
		}
	}

	public static YieldInstruction wait_for_next_frame => null;
}
