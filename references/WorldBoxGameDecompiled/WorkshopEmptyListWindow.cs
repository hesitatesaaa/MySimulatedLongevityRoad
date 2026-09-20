using UnityEngine;

public class WorkshopEmptyListWindow : MonoBehaviour
{
	private void OnEnable()
	{
		if (Config.game_loaded && WindowHistory.hasHistory())
		{
			WindowHistory.list.RemoveAt(WindowHistory.list.Count - 1);
		}
	}
}
