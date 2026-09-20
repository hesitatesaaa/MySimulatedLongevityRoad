using UnityEngine;

public class DebugDropdown : MonoBehaviour
{
	[SerializeField]
	private DebugTool _debug_tool;

	private void OnEnable()
	{
		_debug_tool.active_dropdown = this;
	}
}
