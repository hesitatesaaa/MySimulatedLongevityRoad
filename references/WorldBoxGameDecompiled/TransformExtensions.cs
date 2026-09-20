using System;
using UnityEngine;

public static class TransformExtensions
{
	public static Transform FindRecursive(this Transform pTransform, string pName)
	{
		return pTransform.FindRecursive((Transform tChild) => ((Object)tChild).name == pName);
	}

	public static Transform FindRecursive(this Transform pTransform, Func<Transform, bool> pSelector)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		foreach (Transform item in pTransform)
		{
			Transform val = item;
			if (pSelector(val))
			{
				return val;
			}
			Transform val2 = val.FindRecursive(pSelector);
			if ((Object)(object)val2 != (Object)null)
			{
				return val2;
			}
		}
		return null;
	}

	public static T[] FindAllRecursive<T>(this Transform pTransform)
	{
		return pTransform.FindAllRecursive<T>((Transform p) => ((Component)(object)p).HasComponent<T>());
	}

	public static T[] FindAllRecursive<T>(this Transform pTransform, Func<Transform, bool> pSelector)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		using ListPool<T> listPool = new ListPool<T>();
		foreach (Transform item in pTransform)
		{
			Transform val = item;
			if (pSelector(val) && ((Component)(object)val).HasComponent<T>())
			{
				listPool.Add(((Component)val).GetComponent<T>());
			}
			T[] array = val.FindAllRecursive<T>(pSelector);
			if (array != null)
			{
				listPool.AddRange(array);
			}
		}
		return listPool.ToArray();
	}

	public static Transform FindParentWithName(this Transform pChildObject, params string[] pNames)
	{
		Transform val = null;
		foreach (string pName in pNames)
		{
			val = pChildObject.FindParentWithName(pName);
			if ((Object)(object)val != (Object)null)
			{
				break;
			}
		}
		return val;
	}

	public static Transform FindParentWithName(this Transform pChildObject, string pName)
	{
		Transform val = pChildObject;
		while ((Object)(object)val.parent != (Object)null)
		{
			if (((Object)((Component)val.parent).gameObject).name == pName)
			{
				return val.parent;
			}
			val = ((Component)val.parent).transform;
		}
		return null;
	}

	public static int GetActiveSiblingIndex(this Transform pTransform)
	{
		int num = 0;
		Transform parent = pTransform.parent;
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (((Component)child).gameObject.activeSelf)
			{
				if ((Object)(object)child == (Object)(object)pTransform)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public static int CountActiveChildren(this Transform pTransform)
	{
		int num = 0;
		int i = 0;
		for (int childCount = pTransform.childCount; i < childCount; i++)
		{
			if (((Component)pTransform.GetChild(i)).gameObject.activeSelf)
			{
				num++;
			}
		}
		return num;
	}

	public static int CountChildren(this Transform pTransform, Func<Transform, bool> pSelector)
	{
		int num = 0;
		int i = 0;
		for (int childCount = pTransform.childCount; i < childCount; i++)
		{
			if (pSelector(pTransform.GetChild(i)))
			{
				num++;
			}
		}
		return num;
	}
}
