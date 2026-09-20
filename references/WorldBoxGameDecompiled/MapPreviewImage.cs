using UnityEngine;
using UnityEngine.UI;

public class MapPreviewImage : MonoBehaviour
{
	public bool premiumOnly = true;

	public Image premiumIcon;

	public Button button;

	public SlotButtonCallback slotData;

	public Map map;

	public Sprite defaultSprite;

	private ButtonAnimation buttonAnimation;
}
