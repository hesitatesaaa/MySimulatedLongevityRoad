using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct ButtonTrigger(Button pButton, Entry pEntry, int pIndex)
{
	public Button button { get; } = pButton;

	public Entry entry { get; } = pEntry;

	public int index { get; } = pIndex;
}
