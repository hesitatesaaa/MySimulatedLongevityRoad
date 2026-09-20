using System;
using UnityEngine;
using UnityEngine.UI;

public class ActorDebugAnimationElement : BaseDebugAnimationElement<ActorAsset>
{
	public SpriteAnimation adult;

	public SpriteAnimation baby;

	protected override void Start()
	{
		base.Start();
		adult.create();
		baby.create();
	}

	public override void update()
	{
		if (is_playing)
		{
			adult.update(Time.deltaTime);
			if (asset.has_baby_form)
			{
				baby.update(Time.deltaTime);
			}
			frame_number_text.text = adult.currentFrameIndex.ToString();
		}
	}

	public override void setData(ActorAsset pAsset)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		base.setData(pAsset);
		if (!asset.has_baby_form)
		{
			((Behaviour)baby).enabled = false;
			baby.image.sprite = null;
			((Graphic)baby.image).color = Color.clear;
		}
	}

	protected override void clear()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)adult).enabled = true;
		((Graphic)adult.image).color = Color.white;
		adult.frames = Array.Empty<Sprite>();
		adult.resetAnim();
		((Behaviour)baby).enabled = true;
		((Graphic)baby.image).color = Color.white;
		baby.frames = Array.Empty<Sprite>();
		baby.resetAnim();
	}

	public override void stopAnimations()
	{
		base.stopAnimations();
		adult.isOn = false;
		baby.isOn = false;
		frame_number_text.text = adult.currentFrameIndex.ToString();
	}

	public override void startAnimations()
	{
		base.startAnimations();
		adult.isOn = true;
		baby.isOn = true;
	}

	protected override void clickNextFrame()
	{
		if (!is_playing)
		{
			int num = adult.frames.Length;
			adult.currentFrameIndex++;
			baby.currentFrameIndex++;
			if (adult.currentFrameIndex > num - 1)
			{
				adult.currentFrameIndex = 0;
				baby.currentFrameIndex = 0;
			}
			frame_number_text.text = adult.currentFrameIndex.ToString();
			adult.updateFrame();
			baby.updateFrame();
		}
	}
}
