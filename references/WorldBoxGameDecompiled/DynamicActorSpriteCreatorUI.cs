using UnityEngine;

public static class DynamicActorSpriteCreatorUI
{
	private static int[] _boat_angles = new int[8] { 0, -45, -90, -135, 180, 135, 90, 45 };

	public static AnimationContainerUnit getContainerForUI(ActorAsset pAsset, bool pAdult, ActorTextureSubAsset pTextureAsset, SubspeciesTrait pMutationAsset = null, bool pIsEgg = false, SubspeciesTrait pEggAsset = null, string pTexturePath = null)
	{
		string pTexturePath2 = ((!string.IsNullOrEmpty(pTexturePath)) ? pTexturePath : (pIsEgg ? pEggAsset.sprite_path : ((pAdult || !pAsset.has_baby_form) ? pTextureAsset.getUnitTexturePathForUI(pAsset) : pTextureAsset.texture_path_baby)));
		return ActorAnimationLoader.getAnimationContainer(pTexturePath2, pAsset, pEggAsset, pMutationAsset);
	}

	public static AnimationContainerUnit getContainerForUI(Actor pActor)
	{
		Subspecies subspecies = pActor.subspecies;
		ActorAsset asset = pActor.asset;
		SubspeciesTrait subspeciesTrait = null;
		ActorTextureSubAsset texture_asset;
		if (pActor.hasSubspecies() && subspecies.has_mutation_reskin)
		{
			subspeciesTrait = subspecies.mutation_skin_asset;
			texture_asset = subspeciesTrait.texture_asset;
		}
		else
		{
			texture_asset = asset.texture_asset;
		}
		return getContainerForUI(asset, pActor.isAdult(), texture_asset, subspeciesTrait);
	}

	public static Sprite getUnitSpriteForUI(ActorAsset pAsset, Sprite pMainSprite, AnimationContainerUnit pContainer, bool pAdult, ActorSex pSex, int pPhenotypeIndex, int pPhenotypeShade, ColorAsset pKingdomColor, long pActorId, int pHeadId, bool pEgg = false, bool pKing = false, bool pWarrior = false, bool pWise = false)
	{
		long num = 0L;
		long num2 = pPhenotypeIndex;
		long num3 = 0L;
		long num4 = 0L;
		long num5 = DynamicSpriteCreator.getBodySpriteSmallID(pMainSprite);
		if (num2 != 0L)
		{
			num3 = pPhenotypeShade + 1;
		}
		Sprite spriteHeadForUI = getSpriteHeadForUI(pAsset, pSex, pContainer, pActorId, pHeadId, pAdult, pEgg, pKing, pWarrior, pWise);
		int value = 0;
		if ((Object)(object)spriteHeadForUI != (Object)null)
		{
			ActorAnimationLoader.int_ids_heads.TryGetValue(spriteHeadForUI, out value);
			if (value == 0)
			{
				int num6 = ActorAnimationLoader.int_ids_heads.Count + 1;
				ActorAnimationLoader.int_ids_heads.Add(spriteHeadForUI, num6);
				value = num6;
			}
			num4 = value;
		}
		if (pKingdomColor != null)
		{
			num = pKingdomColor.index_id + 1;
		}
		long num7 = num * 10000000 + num4 * 1000000 + num5 * 1000 + num2 * 10 + num3;
		AnimationFrameData value2 = null;
		pContainer?.dict_frame_data.TryGetValue(((Object)pMainSprite).name, out value2);
		DynamicSpritesAsset units = DynamicSpritesLibrary.units;
		Sprite val = units.getSprite(num7);
		if (val == null)
		{
			val = DynamicSpriteCreator.createNewSpriteUnit(value2, pMainSprite, spriteHeadForUI, pKingdomColor, pAsset, pPhenotypeIndex, pPhenotypeShade, pAsset.texture_atlas);
			units.addSprite(num7, val);
		}
		return val;
	}

	public static Sprite getUnitSpriteForUI(Actor pActor, Sprite pSprite)
	{
		ActorAsset asset = pActor.asset;
		AnimationContainerUnit animation_container = pActor.animation_container;
		if (asset.has_override_avatar_frames)
		{
			return asset.get_override_avatar_frames(pActor)[0];
		}
		int phenotype_shade = pActor.data.phenotype_shade;
		int phenotype_index = pActor.data.phenotype_index;
		return getUnitSpriteForUI(pActor.asset, pSprite, animation_container, pActor.isAdult(), pActor.data.sex, phenotype_index, phenotype_shade, pActor.kingdom.getColor(), pActor.data.id, pActor.data.head, pActor.isEgg());
	}

	public static Sprite getSpriteHeadForUI(ActorAsset pAsset, ActorSex pSex, AnimationContainerUnit pContainer, long pActorId, int pHeadId, bool pAdult = true, bool pEgg = false, bool pKing = false, bool pWarrior = false, bool pWise = false, bool pRandom = false)
	{
		if (pEgg)
		{
			return null;
		}
		if (pAsset.is_boat)
		{
			return null;
		}
		if (!pAdult && !pContainer.render_heads_for_children)
		{
			return null;
		}
		string pPath = "";
		int num = 0;
		bool flag = false;
		ActorTextureSubAsset texture_asset = pAsset.texture_asset;
		if (!texture_asset.has_advanced_textures)
		{
			Sprite[] heads = pContainer.heads;
			if (heads != null && heads.Length != 0)
			{
				if (pRandom)
				{
					return pContainer.heads.GetRandom();
				}
				num = AnimationHelper.getSpriteIndex(pActorId, pContainer.heads.Length);
				return getSprite(num, pContainer.heads);
			}
			return null;
		}
		if (pKing)
		{
			pPath = texture_asset.texture_head_king;
			flag = true;
		}
		else if (pWarrior)
		{
			pPath = texture_asset.texture_head_warrior;
			flag = true;
		}
		else if (pWise && texture_asset.has_old_heads)
		{
			flag = true;
			pPath = ((pSex != ActorSex.Male) ? texture_asset.texture_heads_old_female : texture_asset.texture_heads_old_male);
		}
		if (flag)
		{
			return ActorAnimationLoader.getHeadSpecial(pPath);
		}
		if (pSex == ActorSex.Male)
		{
			if (pRandom)
			{
				return pContainer.heads_male.GetRandom();
			}
			num = ((pHeadId != -1) ? pHeadId : AnimationHelper.getSpriteIndex(pActorId, pContainer.heads_male.Length));
			return getSprite(num, pContainer.heads_male);
		}
		if (pRandom)
		{
			return pContainer.heads_female.GetRandom();
		}
		num = ((pHeadId != -1) ? pHeadId : AnimationHelper.getSpriteIndex(pActorId, pContainer.heads_female.Length));
		return getSprite(num, pContainer.heads_female);
	}

	private static Sprite getSprite(int pIndex, Sprite[] pSprites)
	{
		return pSprites[pIndex];
	}

	public static ActorAnimation getBoatAnimation(AnimationDataBoat pBoatAnimation)
	{
		ActorAnimation actorAnimation = new ActorAnimation();
		Sprite[] array = (Sprite[])(object)new Sprite[_boat_angles.Length * 2];
		for (int i = 0; i < _boat_angles.Length; i++)
		{
			int key = _boat_angles[i];
			ActorAnimation actorAnimation2 = pBoatAnimation.dict[key];
			int num = i * 2;
			array[num] = actorAnimation2.frames[0];
			array[num + 1] = actorAnimation2.frames[1];
		}
		actorAnimation.frames = array;
		return actorAnimation;
	}
}
