using UnityEngine;

public static class InputHelpers
{
	public static bool touchSupported => Input.touchSupported;

	public static int touchCount => Input.touchCount;

	public static bool mouseSupported
	{
		get
		{
			if (Input.mousePresent)
			{
				if (Input.touchSupported)
				{
					return Input.touchCount == 0;
				}
				return true;
			}
			return false;
		}
	}

	public static bool GetMouseButtonDown(int pButton)
	{
		if (pButton == -1)
		{
			return false;
		}
		return Input.GetMouseButtonDown(pButton);
	}

	public static bool GetAnyMouseButtonDown()
	{
		if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
		{
			return Input.GetMouseButtonDown(2);
		}
		return true;
	}

	public static int GetAnyMouseButtonDownIndex()
	{
		if (Input.GetMouseButtonDown(0))
		{
			return 0;
		}
		if (Input.GetMouseButtonDown(1))
		{
			return 1;
		}
		if (Input.GetMouseButtonDown(2))
		{
			return 2;
		}
		return -1;
	}

	public static bool GetMouseButton(int pButton)
	{
		if (pButton == -1)
		{
			return false;
		}
		return Input.GetMouseButton(pButton);
	}

	public static bool GetAnyMouseButton()
	{
		if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
		{
			return Input.GetMouseButton(2);
		}
		return true;
	}

	public static int GetAnyMouseButtonIndex()
	{
		if (Input.GetMouseButton(0))
		{
			return 0;
		}
		if (Input.GetMouseButton(1))
		{
			return 1;
		}
		if (Input.GetMouseButton(2))
		{
			return 2;
		}
		return -1;
	}

	public static bool GetMouseButtonUp(int pButton)
	{
		if (pButton == -1)
		{
			return false;
		}
		return Input.GetMouseButtonUp(pButton);
	}

	public static bool GetAnyMouseButtonUp()
	{
		if (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1))
		{
			return Input.GetMouseButtonUp(2);
		}
		return true;
	}

	public static int GetAnyMouseButtonUpIndex()
	{
		if (Input.GetMouseButtonUp(0))
		{
			return 0;
		}
		if (Input.GetMouseButtonUp(1))
		{
			return 1;
		}
		if (Input.GetMouseButtonUp(2))
		{
			return 2;
		}
		return -1;
	}
}
