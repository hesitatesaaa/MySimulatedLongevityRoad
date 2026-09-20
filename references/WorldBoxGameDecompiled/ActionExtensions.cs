using System;

public static class ActionExtensions
{
	public static bool[] Run(this WorldAction pAction, BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		Delegate[] invocationList = pAction.GetInvocationList();
		bool[] array = new bool[invocationList.Length];
		int num = 0;
		Delegate[] array2 = invocationList;
		for (int i = 0; i < array2.Length; i++)
		{
			WorldAction worldAction = (WorldAction)array2[i];
			array[num++] = worldAction(pTarget, pTile);
		}
		return array;
	}

	public static bool RunAnyTrue(this WorldAction pAction, BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		Delegate[] invocationList = pAction.GetInvocationList();
		bool result = false;
		Delegate[] array = invocationList;
		for (int i = 0; i < array.Length; i++)
		{
			if (((WorldAction)array[i])(pTarget, pTile))
			{
				result = true;
			}
		}
		return result;
	}

	public static bool[] Run(this AttackAction pAction, BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		Delegate[] invocationList = pAction.GetInvocationList();
		bool[] array = new bool[invocationList.Length];
		int num = 0;
		Delegate[] array2 = invocationList;
		for (int i = 0; i < array2.Length; i++)
		{
			AttackAction attackAction = (AttackAction)array2[i];
			array[num++] = attackAction(pSelf, pTarget, pTile);
		}
		return array;
	}

	public static bool RunAnyTrue(this AttackAction pAction, BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null)
	{
		Delegate[] invocationList = pAction.GetInvocationList();
		bool result = false;
		Delegate[] array = invocationList;
		for (int i = 0; i < array.Length; i++)
		{
			if (((AttackAction)array[i])(pSelf, pTarget, pTile))
			{
				result = true;
			}
		}
		return result;
	}
}
