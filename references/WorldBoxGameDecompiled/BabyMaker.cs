using UnityEngine;

public class BabyMaker
{
	public static void startMiracleBirth(Actor pActor)
	{
		BabyHelper.babyMakingStart(pActor);
		if (pActor.hasSubspeciesTrait("reproduction_strategy_viviparity") && pActor.isSexFemale())
		{
			pActor.addStatusEffect("pregnant", pActor.getMaturationTimeSeconds());
		}
		else
		{
			pActor.birthEvent("miracle_bearer");
			makeBabyFromMiracle(pActor, ActorSex.Male, pAddToFamily: true);
			makeBabyFromMiracle(pActor, ActorSex.Female, pAddToFamily: true);
			if (Randy.randomBool())
			{
				makeBabyFromMiracle(pActor, ActorSex.None, pAddToFamily: true);
			}
		}
		pActor.subspecies.counterReproduction();
	}

	public static void startSoulborneBirth(Actor pActor)
	{
		BabyHelper.babyMakingStart(pActor);
		if (pActor.subspecies.hasTrait("reproduction_strategy_viviparity") && pActor.isSexFemale())
		{
			pActor.addStatusEffect("pregnant", pActor.getMaturationTimeSeconds());
		}
		else
		{
			pActor.birthEvent();
			makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: false, pJoinFamily: true);
		}
		pActor.subspecies.counterReproduction();
	}

	public static void spawnSporesFor(Actor pActor)
	{
		pActor.birthEvent();
		BabyHelper.babyMakingStart(pActor);
		int num = Randy.randomInt(3, 10);
		for (int i = 0; i < num; i++)
		{
			Spores spores = (Spores)EffectsLibrary.spawn("fx_spores", pActor.current_tile);
			if ((Object)(object)spores == (Object)null)
			{
				return;
			}
			spores.prepare();
			spores.setActorParent(pActor);
		}
		pActor.subspecies.counterReproduction();
	}

	public static void spawnBabyFromSpore(Actor pActor, Vector3 pPosition)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		WorldTile tile = World.world.GetTile((int)pPosition.x, (int)pPosition.y);
		if (tile != null)
		{
			makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, tile, pAddToFamily: false, pJoinFamily: true);
		}
	}

	public static void makeBabyFromMiracle(Actor pActor, ActorSex pSex = ActorSex.None, bool pAddToFamily = false)
	{
		makeBaby(pActor, null, pSex, pCloneTraits: false, 0, null, pAddToFamily).addTrait("miracle_born");
	}

	public static Actor makeBabyViaFission(Actor pActor)
	{
		pActor.birthEvent();
		BabyHelper.babyMakingStart(pActor);
		Actor actor = makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: false, pJoinFamily: true);
		int pValue = pActor.getHealth() / 2;
		int pValue2 = pActor.getHappiness() / 2;
		int pVal = pActor.getNutrition() / 2;
		pActor.setHealth(pValue);
		pActor.setStamina(0);
		pActor.setHappiness(pValue2);
		pActor.setNutrition(pVal);
		actor.setHealth(pValue);
		actor.setStamina(0);
		actor.setHappiness(pValue2);
		actor.setNutrition(pVal);
		pActor.subspecies.counterReproduction();
		return actor;
	}

	public static Actor makeBabyViaBudding(Actor pActor)
	{
		pActor.birthEvent();
		BabyHelper.babyMakingStart(pActor);
		return makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: false, pJoinFamily: true);
	}

	public static Actor makeBabyViaVegetative(Actor pActor)
	{
		pActor.birthEvent();
		BabyHelper.babyMakingStart(pActor);
		Actor actor = makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: false, pJoinFamily: true);
		actor.addStatusEffect("uprooting", actor.getMaturationTimeSeconds());
		return actor;
	}

	public static void makeBabyViaParthenogenesis(Actor pActor)
	{
		pActor.birthEvent();
		BabyHelper.babyMakingStart(pActor);
		makeBaby(pActor, null, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: false, pJoinFamily: true);
		pActor.subspecies.counterReproduction();
	}

	public static void makeBabiesViaSexual(Actor pMotherTarget, Actor pParentA, Actor pParentB)
	{
		pParentA.birthEvent();
		pParentB.birthEvent();
		BabyHelper.babyMakingStart(pParentA);
		BabyHelper.babyMakingStart(pParentB);
		newImmediateBabySpawn(pParentA, pParentB);
		int num = (int)pMotherTarget.stats["birth_rate"];
		float num2 = 0.5f;
		for (int i = 0; i < num; i++)
		{
			if (!Randy.randomChance(num2))
			{
				break;
			}
			newImmediateBabySpawn(pParentA, pParentB);
			num2 *= 0.85f;
		}
	}

	public static void makeBabyFromPregnancy(Actor pActor)
	{
		pActor.hasLover();
		Actor lover = pActor.lover;
		pActor.birthEvent();
		makeBaby(pActor, lover, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: true);
		float num = 0.5f;
		int num2 = (int)pActor.stats["birth_rate"];
		for (int i = 0; i < num2; i++)
		{
			if (!Randy.randomChance(num))
			{
				break;
			}
			makeBaby(pActor, lover, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: true);
			num *= 0.85f;
		}
	}

	private static void newImmediateBabySpawn(Actor pParent1, Actor pParent2)
	{
		makeBaby(pParent1, pParent2, ActorSex.None, pCloneTraits: false, 0, null, pAddToFamily: true).justBorn();
	}

	public static Actor makeBaby(Actor pParent1, Actor pParent2, ActorSex pForcedSexType = ActorSex.None, bool pCloneTraits = false, int pMutationRate = 0, WorldTile pTile = null, bool pAddToFamily = false, bool pJoinFamily = false)
	{
		City city = pParent1.city ?? pParent2?.city;
		if (city != null)
		{
			city.status.housing_free--;
		}
		ActorAsset asset = pParent1.asset;
		ActorData actorData = new ActorData();
		actorData.created_time = World.world.getCurWorldTime();
		actorData.id = World.world.map_stats.getNextId("unit");
		actorData.asset_id = asset.id;
		int generation = pParent1.data.generation;
		if (pParent2 != null && pParent2.data.generation > generation)
		{
			generation = pParent2.data.generation;
		}
		actorData.generation = generation + 1;
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>(4);
		WorldTile[] neighboursAll = pParent1.current_tile.neighboursAll;
		foreach (WorldTile worldTile in neighboursAll)
		{
			if (worldTile != pParent1.current_tile && (pParent2 == null || worldTile != pParent2.current_tile) && worldTile.Type.ground)
			{
				listPool.Add(worldTile);
			}
		}
		WorldTile pTile2 = ((pTile != null) ? pTile : ((listPool.Count != 0) ? listPool.GetRandom() : pParent1.current_tile));
		Actor actor = World.world.units.createBabyActorFromData(actorData, pTile2, city);
		actor.setParent1(pParent1);
		if (pParent2 != null)
		{
			actor.setParent2(pParent2);
		}
		if (pAddToFamily && !pParent1.hasFamily())
		{
			World.world.families.newFamily(pParent1, pParent1.current_tile, pParent2);
		}
		else if (pJoinFamily)
		{
			Family family = null;
			family = (pParent1.hasFamily() ? pParent1.family : World.world.families.newFamily(pParent1, pParent1.current_tile, pParent2));
			if (family != null)
			{
				actor.setFamily(family);
			}
		}
		BabyHelper.applyParentsMeta(pParent1, pParent2, actor);
		if (pCloneTraits || pParent1.hasSubspeciesTrait("genetic_mirror"))
		{
			BabyHelper.traitsClone(actor, pParent1);
		}
		else
		{
			foreach (ActorTrait trait in actor.subspecies.getActorBirthTraits().getTraits())
			{
				actor.addTrait(trait);
			}
			BabyHelper.traitsInherit(actor, pParent1, pParent2);
		}
		actor.checkTraitMutationOnBirth();
		actor.setNutrition(SimGlobals.m.nutrition_start_level_baby);
		if (pForcedSexType != ActorSex.None)
		{
			actor.data.sex = pForcedSexType;
		}
		else
		{
			ActorSex actorSex = ActorSex.None;
			if (Randy.randomBool())
			{
				actorSex = (pParent1.hasCity() ? ((pParent1.city.status.females <= pParent1.city.status.males) ? ActorSex.Female : ActorSex.Male) : ((pParent1.subspecies.cached_females <= pParent1.subspecies.cached_males) ? ActorSex.Female : ActorSex.Male));
			}
			if (actorSex != ActorSex.None)
			{
				actor.data.sex = actorSex;
			}
			else
			{
				actor.generateSex();
			}
		}
		actor.checkShouldBeEgg();
		actor.makeStunned(10f);
		actor.applyRandomForce();
		BabyHelper.countBirth(actor);
		BabyHelper.countMakeChild(pParent1, pParent2);
		actor.setStatsDirty();
		actor.event_full_stats = true;
		return actor;
	}
}
