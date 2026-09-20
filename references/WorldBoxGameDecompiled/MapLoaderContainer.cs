internal class MapLoaderContainer
{
	public MapLoaderAction action;

	public string id;

	public bool debug_log = true;

	public float new_timer_value = 0.001f;

	public MapLoaderContainer(MapLoaderAction pAction, string pID, bool pDebugLog = true, float pNewWaitTimerValue = 0.001f)
	{
		action = pAction;
		id = pID;
		debug_log = pDebugLog;
		new_timer_value = pNewWaitTimerValue;
	}
}
