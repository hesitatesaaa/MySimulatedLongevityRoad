using UnityEngine;
using db;

public class ClickQuitTheGame : MonoBehaviour
{
	public void clickQuit()
	{
		DBManager.clearAndClose();
		Application.Quit();
	}
}
