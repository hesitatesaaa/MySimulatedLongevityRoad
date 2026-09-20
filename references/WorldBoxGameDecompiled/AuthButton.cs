using System.Collections.Generic;
using UnityEngine;

public class AuthButton : MonoBehaviour
{
	private static string windowId;

	private static List<string> worldnetNoSub = new List<string> { "worldnet_main" };

	private void Awake()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void showWorldNetOwnWorldsWindow()
	{
	}

	public void showWorldNetWorldsListWindow()
	{
	}

	public void showWorldNetMainWindow()
	{
	}

	public void showWorldNetUploadWindow()
	{
	}

	public void showBrowseByTagWindow()
	{
	}

	public void wbbConfirm()
	{
	}

	public void uploadWorldButton()
	{
	}

	public void checkAuthAndOpenWindow()
	{
	}
}
