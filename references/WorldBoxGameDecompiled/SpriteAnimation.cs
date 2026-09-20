using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimation : MonoBehaviour
{
	public bool ignorePause = true;

	public bool isOn = true;

	public float timeBetweenFrames = 0.1f;

	public float nextFrameTime;

	public bool useNormalDeltaTime;

	public AnimPlayType playType;

	public int currentFrameIndex;

	public bool looped = true;

	public bool returnToPool;

	public Sprite[] frames;

	public bool dirty;

	internal SpriteRenderer spriteRenderer;

	internal Image image;

	internal bool useOnSpriteRenderer = true;

	internal PhenotypeAsset phenotype;

	private bool stopFrameTrigger;

	internal Sprite currentSpriteGraphic;

	public void Awake()
	{
		spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		if ((Object)(object)spriteRenderer == (Object)null)
		{
			image = ((Component)this).GetComponent<Image>();
			useOnSpriteRenderer = false;
		}
		else
		{
			currentSpriteGraphic = spriteRenderer.sprite;
		}
	}

	public virtual void create()
	{
		spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		nextFrameTime = timeBetweenFrames;
	}

	public void stopAnimations()
	{
		isOn = false;
	}

	internal void setFrames(Sprite[] pFrames)
	{
		if (frames != pFrames)
		{
			frames = pFrames;
			if (currentFrameIndex >= frames.Length)
			{
				currentFrameIndex = 0;
			}
			updateFrame();
		}
	}

	public bool isLastFrame()
	{
		return currentFrameIndex >= frames.Length - 1;
	}

	public bool isFirstFrame()
	{
		return currentFrameIndex == 0;
	}

	internal virtual void update(float pElapsed)
	{
		if (useNormalDeltaTime)
		{
			pElapsed = Time.deltaTime;
		}
		if (dirty)
		{
			dirty = false;
			forceUpdateFrame();
		}
		else if (!isOn)
		{
			if (stopFrameTrigger)
			{
				stopFrameTrigger = false;
				updateFrame();
			}
		}
		else
		{
			if (World.world.isPaused() && !ignorePause)
			{
				return;
			}
			if (nextFrameTime > 0f)
			{
				nextFrameTime -= pElapsed;
				return;
			}
			nextFrameTime = timeBetweenFrames;
			if (playType == AnimPlayType.Forward)
			{
				if (currentFrameIndex + 1 >= frames.Length)
				{
					if (returnToPool)
					{
						((Component)this).GetComponent<BaseEffect>().kill();
						return;
					}
					if (!looped)
					{
						return;
					}
					currentFrameIndex = 0;
				}
				else
				{
					currentFrameIndex++;
				}
			}
			else if (currentFrameIndex - 1 < 0)
			{
				if (returnToPool)
				{
					((Component)this).GetComponent<BaseEffect>().kill();
					return;
				}
				if (!looped)
				{
					return;
				}
				currentFrameIndex = frames.Length - 1;
			}
			else
			{
				currentFrameIndex--;
			}
			updateFrame();
		}
	}

	public void stopAt(int pFrameId = 0, bool pNow = false)
	{
		isOn = false;
		currentFrameIndex = pFrameId;
		if (pNow)
		{
			updateFrame();
		}
		else
		{
			stopFrameTrigger = true;
		}
	}

	public void forceUpdateFrame()
	{
		if (frames.Length != 0)
		{
			currentSpriteGraphic = frames[currentFrameIndex];
			applyCurrentSpriteGraphics(currentSpriteGraphic);
		}
	}

	public void setRandomFrame()
	{
		currentFrameIndex = Randy.randomInt(0, frames.Length);
		updateFrame();
	}

	public void setFrameIndex(int pFrame)
	{
		currentFrameIndex = pFrame;
		updateFrame();
	}

	public void updateFrame()
	{
		if (frames.Length != 0 && currentFrameIndex < frames.Length && !((Object)(object)currentSpriteGraphic == (Object)(object)frames[currentFrameIndex]))
		{
			currentSpriteGraphic = frames[currentFrameIndex];
			applyCurrentSpriteGraphics(currentSpriteGraphic);
		}
	}

	internal void applyCurrentSpriteGraphics(Sprite pSprite)
	{
		if (useOnSpriteRenderer)
		{
			Sprite sprite = pSprite;
			if (phenotype != null)
			{
				sprite = DynamicSprites.getIconWithColors(pSprite, phenotype, null);
			}
			spriteRenderer.sprite = sprite;
		}
		else
		{
			image.sprite = pSprite;
		}
	}

	public Sprite getCurrentGraphics()
	{
		return currentSpriteGraphic;
	}

	public void resetAnim(int pFrameIndex = 0)
	{
		nextFrameTime = timeBetweenFrames;
		currentFrameIndex = pFrameIndex;
		updateFrame();
	}
}
