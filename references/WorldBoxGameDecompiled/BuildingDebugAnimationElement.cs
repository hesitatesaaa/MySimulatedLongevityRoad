using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingDebugAnimationElement : BaseDebugAnimationElement<BuildingAsset>
{
	public BuildingDebugAnimationVariation variation_prefab;

	public Sprite no_animation_sprite;

	public Transform variations_transform;

	private List<BuildingDebugAnimationVariation> _variations;

	private bool _has_baby;

	public override void update()
	{
		if (!is_playing)
		{
			return;
		}
		foreach (BuildingDebugAnimationVariation variation in _variations)
		{
			variation.update(Time.deltaTime);
		}
		frame_number_text.text = _variations[0].sprite_animation.currentFrameIndex.ToString();
	}

	public override void setData(BuildingAsset pAsset)
	{
		base.setData(pAsset);
		_variations = new List<BuildingDebugAnimationVariation>();
		for (int i = 0; i < pAsset.building_sprites.animation_data.Count; i++)
		{
			BuildingDebugAnimationVariation buildingDebugAnimationVariation = Object.Instantiate<BuildingDebugAnimationVariation>(variation_prefab, variations_transform);
			setAnimationSettings(buildingDebugAnimationVariation.sprite_animation, buildingDebugAnimationVariation.image);
			setAnimationSettings(buildingDebugAnimationVariation.shadow_animation, buildingDebugAnimationVariation.shadow);
			_variations.Add(buildingDebugAnimationVariation);
		}
	}

	private void setAnimationSettings(SpriteAnimation pAnimation, Image pImage)
	{
		pAnimation.create();
		pAnimation.useOnSpriteRenderer = false;
		pAnimation.image = pImage;
		pAnimation.timeBetweenFrames = 1f / asset.animation_speed;
	}

	public void setFrames(List<DebugAnimatedVariation> pVariations, bool pShouldHaveSprites)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		if (pVariations.Count != _variations.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		bool flag = false;
		for (int i = 0; i < pVariations.Count; i++)
		{
			BuildingDebugAnimationVariation buildingDebugAnimationVariation = _variations[i];
			if (!pShouldHaveSprites)
			{
				((Graphic)buildingDebugAnimationVariation.image).color = Color.clear;
				((Graphic)buildingDebugAnimationVariation.shadow).color = Color.clear;
				continue;
			}
			DebugAnimatedVariation debugAnimatedVariation = pVariations[i];
			Sprite[] frames = debugAnimatedVariation.frames;
			if (frames == null || frames.Length == 0)
			{
				buildingDebugAnimationVariation.image.sprite = no_animation_sprite;
				((Graphic)buildingDebugAnimationVariation.shadow).color = Color.clear;
				((Behaviour)buildingDebugAnimationVariation).enabled = false;
				Debug.LogError((object)("Missing sprites for Building asset " + asset.id));
				continue;
			}
			if (!debugAnimatedVariation.animated)
			{
				Sprite val = frames[0];
				buildingDebugAnimationVariation.image.sprite = val;
				if (asset.shadow)
				{
					DynamicSpriteCreator.createBuildingShadow(asset, val, pIsContructionSprite: false);
					buildingDebugAnimationVariation.shadow.sprite = DynamicSprites.getShadowBuilding(asset, val);
				}
				else
				{
					((Graphic)buildingDebugAnimationVariation.shadow).color = Color.clear;
				}
				((Behaviour)buildingDebugAnimationVariation).enabled = false;
				continue;
			}
			buildingDebugAnimationVariation.sprite_animation.setFrames(frames);
			Sprite[] array = (Sprite[])(object)new Sprite[frames.Length];
			for (int j = 0; j < frames.Length; j++)
			{
				Sprite pSprite = frames[j];
				if (asset.shadow)
				{
					DynamicSpriteCreator.createBuildingShadow(asset, pSprite, pIsContructionSprite: false);
					array[j] = DynamicSprites.getShadowBuilding(asset, pSprite);
				}
				else
				{
					((Graphic)buildingDebugAnimationVariation.shadow).color = Color.clear;
				}
			}
			buildingDebugAnimationVariation.shadow_animation.setFrames(array);
			flag = true;
		}
		if (flag)
		{
			startAnimations();
		}
	}

	protected override void clear()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in variations_transform)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	public override void stopAnimations()
	{
		base.stopAnimations();
		foreach (BuildingDebugAnimationVariation variation in _variations)
		{
			variation.toggleAnimation(pState: false);
		}
		frame_number_text.text = _variations[0].sprite_animation.currentFrameIndex.ToString();
	}

	public override void startAnimations()
	{
		base.startAnimations();
		foreach (BuildingDebugAnimationVariation variation in _variations)
		{
			variation.toggleAnimation(pState: true);
		}
	}

	protected override void clickNextFrame()
	{
		if (is_playing)
		{
			return;
		}
		SpriteAnimation sprite_animation = _variations[0].sprite_animation;
		int num = sprite_animation.frames.Length;
		int num2 = sprite_animation.currentFrameIndex++;
		if (num2 > num - 1)
		{
			num2 = 0;
		}
		frame_number_text.text = num2.ToString();
		foreach (BuildingDebugAnimationVariation variation in _variations)
		{
			variation.setFrame(num2);
		}
	}
}
