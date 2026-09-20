using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorDebugAssetElement : BaseDebugAssetElement<ActorAsset>
{
	public Image icon_left;

	public Image icon_right;

	public ActorDebugAnimationElement animation_idle;

	public ActorDebugAnimationElement animation_walk;

	public ActorDebugAnimationElement animation_swim;

	public Image egg;

	public Sprite no_animation_baby;

	private AnimationContainerUnit _animation_container_adult;

	private AnimationContainerUnit _animation_container_baby;

	private int _phenotype_index;

	private int _phenotype_shade_id;

	public override void setData(ActorAsset pAsset)
	{
		asset = pAsset;
		Sprite spriteIcon = asset.getSpriteIcon();
		icon_left.sprite = spriteIcon;
		icon_right.sprite = spriteIcon;
		title.text = asset.id;
		((Component)egg).gameObject.SetActive(true);
		initAnimations();
		initStats();
	}

	protected override void initAnimations()
	{
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		if (asset.hasDefaultEggForm())
		{
			((Component)egg).gameObject.SetActive(true);
			SubspeciesTrait subspeciesTrait = AssetManager.subspecies_traits.get(asset.getDefaultEggID());
			egg.sprite = SpriteTextureLoader.getSprite(subspeciesTrait.sprite_path);
		}
		else
		{
			((Component)egg).gameObject.SetActive(false);
		}
		animation_idle.setData(asset);
		animation_walk.setData(asset);
		animation_swim.setData(asset);
		if (asset.special)
		{
			Sprite sprite = icon_left.sprite;
			animation_idle.adult.image.sprite = sprite;
			animation_walk.adult.image.sprite = sprite;
			animation_swim.adult.image.sprite = sprite;
			((Component)egg).gameObject.SetActive(false);
			stopAnimations();
			return;
		}
		if (asset.use_phenotypes)
		{
			_phenotype_index = AssetManager.phenotype_library.get(asset.debug_phenotype_colors).phenotype_index;
			_phenotype_shade_id = Actor.getRandomPhenotypeShade();
		}
		else
		{
			_phenotype_index = 0;
			_phenotype_shade_id = 0;
		}
		if (asset.is_boat)
		{
			AnimationDataBoat animationDataBoat = ActorAnimationLoader.loadAnimationBoat(asset.id);
			setAnimation(DynamicActorSpriteCreatorUI.getBoatAnimation(animationDataBoat), animation_idle, asset.animation_idle_speed, pIsAdult: true, pHasAnimation: true, pShouldHaveAnimation: true);
			setAnimation(animationDataBoat.normal, animation_walk, asset.animation_walk_speed, pIsAdult: true, pHasAnimation: true, pShouldHaveAnimation: true);
			setAnimation(animationDataBoat.broken, animation_swim, asset.animation_swim_speed, pIsAdult: true, pHasAnimation: true, pShouldHaveAnimation: true);
			return;
		}
		string[] array = asset.animation_idle;
		bool pShouldHaveAnimation = array != null && array.Length != 0;
		string[] array2 = asset.animation_walk;
		bool pShouldHaveAnimation2 = array2 != null && array2.Length != 0;
		string[] array3 = asset.animation_swim;
		bool pShouldHaveAnimation3 = array3 != null && array3.Length != 0;
		_animation_container_adult = DynamicActorSpriteCreatorUI.getContainerForUI(asset, pAdult: true, asset.texture_asset);
		setAnimation(_animation_container_adult.idle, animation_idle, asset.animation_idle_speed, pIsAdult: true, _animation_container_adult.has_idle, pShouldHaveAnimation);
		setAnimation(_animation_container_adult.walking, animation_walk, asset.animation_walk_speed, pIsAdult: true, _animation_container_adult.has_walking, pShouldHaveAnimation2);
		List<string> default_subspecies_traits = asset.default_subspecies_traits;
		if (default_subspecies_traits != null && !default_subspecies_traits.Contains("hovering") && !asset.flying)
		{
			setAnimation(_animation_container_adult.swimming, animation_swim, asset.animation_swim_speed, pIsAdult: true, _animation_container_adult.has_swimming, pShouldHaveAnimation3);
		}
		else
		{
			((Graphic)animation_swim.adult.image).color = Color.clear;
			((Behaviour)animation_swim.adult).enabled = false;
		}
		if (asset.has_baby_form)
		{
			_animation_container_baby = DynamicActorSpriteCreatorUI.getContainerForUI(asset, pAdult: false, asset.texture_asset);
			setAnimation(_animation_container_baby.idle, animation_idle, asset.animation_idle_speed, pIsAdult: false, _animation_container_baby.has_idle, pShouldHaveAnimation);
			setAnimation(_animation_container_baby.walking, animation_walk, asset.animation_walk_speed, pIsAdult: false, _animation_container_baby.has_walking, pShouldHaveAnimation2);
			if (!asset.default_subspecies_traits.Contains("hovering") && !asset.flying)
			{
				setAnimation(_animation_container_baby.swimming, animation_swim, asset.animation_swim_speed, pIsAdult: false, _animation_container_baby.has_swimming, pShouldHaveAnimation3);
				return;
			}
			((Graphic)animation_swim.baby.image).color = Color.clear;
			((Behaviour)animation_swim.baby).enabled = false;
		}
	}

	public override void update()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			animation_idle.update();
			animation_walk.update();
			animation_swim.update();
		}
	}

	public override void stopAnimations()
	{
		animation_idle.stopAnimations();
		animation_walk.stopAnimations();
		animation_swim.stopAnimations();
	}

	public override void startAnimations()
	{
		animation_idle.startAnimations();
		animation_walk.startAnimations();
		animation_swim.startAnimations();
	}

	private void setAnimation(ActorAnimation pAnimation, ActorDebugAnimationElement pElement, float pAnimationSpeed, bool pIsAdult, bool pHasAnimation, bool pShouldHaveAnimation)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		SpriteAnimation spriteAnimation = (pIsAdult ? pElement.adult : pElement.baby);
		if (!pShouldHaveAnimation)
		{
			((Graphic)spriteAnimation.image).color = Color.clear;
			spriteAnimation.image.sprite = null;
			((Behaviour)spriteAnimation).enabled = false;
			return;
		}
		if (!pHasAnimation)
		{
			((Graphic)spriteAnimation.image).color = Color.white;
			spriteAnimation.image.sprite = (pIsAdult ? no_animation : no_animation_baby);
			((Behaviour)spriteAnimation).enabled = false;
			return;
		}
		AnimationContainerUnit pContainer = (pIsAdult ? _animation_container_adult : _animation_container_baby);
		Sprite[] array = (Sprite[])(object)new Sprite[pAnimation.frames.Length];
		ColorAsset debug_color_asset = AssetManager.kingdoms.get(asset.kingdom_id_wild).debug_color_asset;
		for (int i = 0; i < pAnimation.frames.Length; i++)
		{
			array[i] = DynamicActorSpriteCreatorUI.getUnitSpriteForUI(asset, pAnimation.frames[i], pContainer, pIsAdult, AssetsDebugManager.actors_sex, _phenotype_index, _phenotype_shade_id, debug_color_asset, 0L, 0);
		}
		((Behaviour)spriteAnimation).enabled = true;
		spriteAnimation.setFrames(array);
		spriteAnimation.timeBetweenFrames = 1f / pAnimationSpeed;
		pElement.startAnimations();
	}

	protected override void initStats()
	{
		base.initStats();
		BaseStats statsForOverview = asset.getStatsForOverview();
		showStat("health", statsForOverview["health"]);
		showStat("damage", statsForOverview["damage"]);
		showStat("speed", statsForOverview["speed"]);
		showStat("lifespan", statsForOverview["lifespan"]);
	}

	protected override void showAssetWindow()
	{
		base.showAssetWindow();
		ScrollWindow.showWindow("actor_asset");
	}
}
