using UnityEngine;

public class TagButton : MonoBehaviour
{
	public MapTagType tagType;

	private void Awake()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void showWorldNetTagListWindow()
	{
	}

	public bool inListWindow()
	{
		if (ScrollWindow.isCurrentWindow("worldnet_list_your_worlds"))
		{
			return true;
		}
		if (ScrollWindow.isCurrentWindow("worldnet_list_more_worlds"))
		{
			return true;
		}
		return false;
	}
}
