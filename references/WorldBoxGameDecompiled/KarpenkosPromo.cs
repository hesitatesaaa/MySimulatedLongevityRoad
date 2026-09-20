using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KarpenkosPromo : MonoBehaviour
{
	public List<Sprite> sprites = new List<Sprite>();

	public Image image1;

	public Image image2;

	private float intervalChange = 1f;

	private float intervalMainImage = 1.5f;

	private int maxElements;

	private int curImageIndex;

	private float timerChange;

	private Image imageTransition;

	private Image imageCurrent;

	private void Awake()
	{
		maxElements = sprites.Count;
	}

	private void OnEnable()
	{
		curImageIndex = 0;
		timerChange = intervalMainImage / 2f;
		setImage(image1, curImageIndex);
		curImageIndex++;
		setImage(image2, curImageIndex);
		imageCurrent = image1;
		imageTransition = image2;
		((Component)imageCurrent).GetComponent<CanvasGroup>().alpha = 1f;
		((Component)imageTransition).GetComponent<CanvasGroup>().alpha = 0f;
	}

	private void setImage(Image pImage, int pIndex)
	{
		pImage.sprite = sprites[pIndex];
	}

	private void Update()
	{
		if (timerChange > 0f)
		{
			timerChange -= Time.deltaTime;
		}
		else
		{
			if (!(((Component)imageTransition).GetComponent<CanvasGroup>().alpha < 1f))
			{
				return;
			}
			CanvasGroup component = ((Component)imageTransition).GetComponent<CanvasGroup>();
			component.alpha += Time.deltaTime * 2f;
			if (((Component)imageTransition).GetComponent<CanvasGroup>().alpha >= 1f)
			{
				((Component)imageTransition).GetComponent<CanvasGroup>().alpha = 0f;
				imageCurrent.sprite = imageTransition.sprite;
				timerChange = intervalChange;
				if (curImageIndex == 0)
				{
					timerChange = intervalMainImage;
				}
				curImageIndex++;
				if (curImageIndex >= maxElements)
				{
					curImageIndex = 0;
				}
				setImage(imageTransition, curImageIndex);
			}
		}
	}
}
