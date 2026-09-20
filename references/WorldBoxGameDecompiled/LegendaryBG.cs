using UnityEngine;
using UnityEngine.UI;

public class LegendaryBG : MonoBehaviour
{
	public Sprite[] spriteArray;

	private Image img;

	private int max_frames = 9;

	private int currentFrame;

	private float timer = 0.07f;

	private void Awake()
	{
		img = ((Component)this).GetComponent<Image>();
		max_frames = spriteArray.Length;
	}

	private void OnEnable()
	{
		timer = 0.2f;
		currentFrame = max_frames - 1;
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			return;
		}
		timer = 0.07f;
		currentFrame++;
		if (currentFrame >= max_frames)
		{
			currentFrame = 0;
		}
		else if (currentFrame == max_frames - 1)
		{
			timer = 2.4f;
		}
		img.sprite = spriteArray[currentFrame];
	}
}
