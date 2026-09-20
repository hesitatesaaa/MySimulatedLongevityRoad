using System.Collections.Generic;
using UnityEngine;

public class ActorAvatarData
{
	public ActorAsset asset;

	public SubspeciesTrait mutation_skin_asset;

	public ActorSex sex;

	public Sprite sprite_head;

	public int head_id;

	public long actor_id;

	public int phenotype_index;

	public int phenotype_skin_shade;

	public ColorAsset kingdom_color;

	public bool is_egg;

	public bool is_king;

	public bool is_warrior;

	public bool is_wise;

	public SubspeciesTrait egg_asset;

	public bool is_adult;

	public bool is_lying;

	public bool is_touching_liquid;

	public bool is_inside_boat;

	public bool is_hovering;

	public bool is_immovable;

	public bool is_unconscious;

	public bool is_stop_idle_animation;

	public IHandRenderer item_renderer;

	public int actor_hash;

	public IEnumerable<string> statuses;

	public IReadOnlyDictionary<string, Status> statuses_gameplay;

	public void setData(Actor pActor)
	{
		ActorAsset actorAsset = pActor.getActorAsset();
		ActorData data = pActor.data;
		setData(actorAsset, pActor.subspecies?.mutation_skin_asset, data.sex, data.id, data.head, pActor.cached_sprite_head, data.phenotype_index, data.phenotype_shade, pActor.kingdom.getColor(), pActor.isEgg(), pActor.isKing(), pActor.isWarrior() && !pActor.equipment.helmet.isEmpty(), pActor.hasTrait("wise"), pActor.subspecies?.egg_asset, pActor.isAdult(), !actorAsset.prevent_unconscious_rotation && pActor.isLying(), pActor.isTouchingLiquid(), pActor.is_inside_boat, pActor.isHovering(), pActor.isImmovable(), !actorAsset.prevent_unconscious_rotation && pActor.is_unconscious, pActor.hasStopIdleAnimation(), pActor.getHandRendererAsset(), pActor.GetHashCode(), pActor.getStatusesIds(), pActor.getStatusesDict());
	}

	public void setData(ActorAsset pAsset, SubspeciesTrait pMutation, ActorSex pSex, long pActorId, int pHeadId, Sprite pSpriteHead, int pPhenotypeIndex, int pPhenotypeSkinShade, ColorAsset pKingdomColor, bool pIsEgg, bool pIsKing, bool pIsWarrior, bool pIsWise, SubspeciesTrait pEggAsset, bool pIsAdult, bool pIsLying, bool pIsTouchingLiquid, bool pIsInsideBoat, bool pIsHovering, bool pIsImmovable, bool pIsUnconscious, bool pIsStopIdleAnimation, IHandRenderer pItemPath, int pActorHash, IEnumerable<string> pStatuses, IReadOnlyDictionary<string, Status> pGameplayStatuses)
	{
		asset = pAsset;
		mutation_skin_asset = pMutation;
		sex = pSex;
		sprite_head = pSpriteHead;
		actor_id = pActorId;
		head_id = pHeadId;
		phenotype_index = pPhenotypeIndex;
		phenotype_skin_shade = pPhenotypeSkinShade;
		kingdom_color = pKingdomColor;
		is_egg = pIsEgg;
		is_king = pIsKing;
		is_warrior = pIsWarrior;
		is_wise = pIsWise;
		egg_asset = pEggAsset;
		is_adult = pIsAdult;
		is_lying = pIsLying;
		is_touching_liquid = pIsTouchingLiquid;
		is_inside_boat = pIsInsideBoat;
		is_hovering = pIsHovering;
		is_immovable = pIsImmovable;
		is_unconscious = pIsUnconscious;
		is_stop_idle_animation = pIsStopIdleAnimation;
		item_renderer = pItemPath;
		actor_hash = pActorHash;
		statuses = pStatuses;
		statuses_gameplay = pGameplayStatuses;
	}

	public ActorTextureSubAsset getTextureAsset()
	{
		if (mutation_skin_asset != null)
		{
			return mutation_skin_asset.texture_asset;
		}
		return asset.texture_asset;
	}

	public Sprite getColoredSprite(Sprite pSprite, AnimationContainerUnit pContainer)
	{
		return DynamicActorSpriteCreatorUI.getUnitSpriteForUI(asset, pSprite, pContainer, is_adult, sex, phenotype_index, phenotype_skin_shade, kingdom_color, actor_id, head_id, is_egg, is_king, is_warrior, is_wise);
	}

	public bool hasRenderedSpriteHead()
	{
		return (Object)(object)sprite_head != (Object)null;
	}
}
