using UnityEngine;

public class WorldStatus : MonoBehaviour
{
	public static int currentSlot;

	public void setCurrentSlot(int pSlotId)
	{
		currentSlot = pSlotId;
	}
}
