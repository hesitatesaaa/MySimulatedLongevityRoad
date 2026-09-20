using UnityEngine;

public class MapTag : MonoBehaviour
{
	public bool tagEnabled;

	public MapTagType tagType;

	public Sprite buttonOn;

	public Sprite buttonOff;

	public string icon;

	public CanvasGroup tagGroup;

	private void Start()
	{
		updateSprite();
	}

	public void clickButton()
	{
	}

	public void clickListWorldsButton()
	{
	}

	private void updateSprite()
	{
	}
}
