using ai;
using ai.behaviours;

public class BehFinishTalk : BehaviourActionActor
{
	public BehFinishTalk()
	{
		socialize = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Actor actor = pActor.beh_actor_target?.a;
		if (actor == null)
		{
			return BehResult.Stop;
		}
		if (!stillCanTalk(actor))
		{
			return BehResult.Stop;
		}
		finishTalk(pActor, actor);
		return BehResult.Continue;
	}

	private bool stillCanTalk(Actor pTarget)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isLying())
		{
			return false;
		}
		return true;
	}

	private void finishTalk(Actor pActor, Actor pTarget)
	{
		pActor.resetSocialize();
		pTarget.resetSocialize();
		bool num = Randy.randomChance(0.7f);
		int pValue = ((!num) ? (-15) : 10);
		pActor.changeHappiness("just_talked", pValue);
		pTarget.changeHappiness("just_talked", pValue);
		pActor.addStatusEffect("recovery_social");
		pTarget.addStatusEffect("recovery_social");
		if (num)
		{
			ActorTool.checkFallInLove(pActor, pTarget);
		}
		if (num)
		{
			ActorTool.checkBecomingBestFriends(pActor, pTarget);
		}
		checkMetaSpread(pActor, pTarget);
		if (pActor.hasCulture() && pActor.culture.hasTrait("youth_reverence") && throwDiceForGift(pActor, pTarget) && pActor.isAdult() && pTarget.getAge() < pActor.getAge())
		{
			makeGift(pActor, pTarget);
		}
		if (pActor.hasCulture() && pActor.culture.hasTrait("elder_reverence") && throwDiceForGift(pActor, pTarget) && pActor.isAdult() && pTarget.getAge() > pActor.getAge())
		{
			makeGift(pActor, pTarget);
		}
		checkPassLearningAttributes(pActor, pTarget);
		pTarget.timer_action = (pActor.timer_action = Randy.randomFloat(1.1f, 3.3f));
	}

	private void checkAttribue(Actor pActor, Actor pTarget, string pAttributeID)
	{
		if (Randy.randomChance(0.3f))
		{
			if (pActor.stats[pAttributeID] > pTarget.stats[pAttributeID])
			{
				pTarget.stats[pAttributeID]++;
			}
			else if (pActor.stats[pAttributeID] < pTarget.stats[pAttributeID])
			{
				pActor.stats[pAttributeID]++;
			}
		}
	}

	private void checkPassLearningAttributes(Actor pActor, Actor pTarget)
	{
		checkAttribue(pActor, pTarget, "intelligence");
		checkAttribue(pActor, pTarget, "warfare");
		checkAttribue(pActor, pTarget, "diplomacy");
		checkAttribue(pActor, pTarget, "stewardship");
	}

	private void checkMetaSpread(Actor pActor, Actor pTarget)
	{
		if (pActor.hasSubspecies() && pTarget.hasSubspecies())
		{
			tryToSpreadCulture(pActor, pTarget);
			tryToSpreadLanguage(pActor, pTarget);
			tryToSpreadReligion(pActor, pTarget);
		}
	}

	private void tryToSpreadCulture(Actor pActor, Actor pTarget)
	{
		if (!pActor.subspecies.has_advanced_memory || !pTarget.subspecies.has_advanced_memory)
		{
			return;
		}
		Culture culture = decideCulture(pActor, pTarget);
		if (culture != null)
		{
			pActor.tryToConvertToCulture(culture);
			pTarget.tryToConvertToCulture(culture);
			if (culture.hasTrait("pep_talks") && Randy.randomChance(0.5f))
			{
				pActor.addStatusEffect("inspired");
				pTarget.addStatusEffect("inspired");
			}
			if (culture.hasTrait("expertise_exchange"))
			{
				pActor.addExperience(CultureTraitLibrary.getValue("expertise_exchange"));
				pTarget.addExperience(CultureTraitLibrary.getValue("expertise_exchange"));
			}
			if (culture.hasTrait("gossip_lovers"))
			{
				pActor.changeHappiness("just_talked_gossip");
				pTarget.changeHappiness("just_talked_gossip");
			}
		}
	}

	private void tryToSpreadLanguage(Actor pActor, Actor pTarget)
	{
		if (pActor.subspecies.has_advanced_communication && pTarget.subspecies.has_advanced_communication)
		{
			Language language = decideLanguage(pActor, pTarget);
			if (language != null)
			{
				pActor.tryToConvertToLanguage(language);
				pTarget.tryToConvertToLanguage(language);
			}
		}
	}

	private void tryToSpreadReligion(Actor pActor, Actor pTarget)
	{
		if (pActor.subspecies.has_advanced_memory && pTarget.subspecies.has_advanced_memory)
		{
			Religion religion = decideReligion(pActor, pTarget);
			if (religion != null)
			{
				pActor.tryToConvertToReligion(religion);
				pTarget.tryToConvertToReligion(religion);
			}
		}
	}

	private Religion decideReligion(Actor pActor1, Actor pActor2)
	{
		Religion religion = pActor1.religion;
		Religion religion2 = pActor2.religion;
		if (religion == null && religion2 == null)
		{
			return null;
		}
		if (religion == null)
		{
			return religion2;
		}
		if (religion2 == null)
		{
			return religion;
		}
		using ListPool<Religion> listPool = new ListPool<Religion>();
		listPool.Add(religion);
		listPool.Add(religion2);
		if (pActor1.hasCity() && pActor1.religion == pActor1.city.getReligion())
		{
			listPool.Add(pActor1.religion);
		}
		if (pActor1.kingdom.hasReligion() && pActor1.religion == pActor1.kingdom.getReligion())
		{
			listPool.Add(pActor1.religion);
		}
		if (pActor2.hasCity() && pActor2.religion == pActor2.city.getReligion())
		{
			listPool.Add(pActor2.religion);
		}
		if (pActor2.kingdom.hasReligion() && pActor2.religion == pActor2.kingdom.getReligion())
		{
			listPool.Add(pActor2.religion);
		}
		return listPool.GetRandom();
	}

	private Language decideLanguage(Actor pActor1, Actor pActor2)
	{
		Language language = pActor1.language;
		Language language2 = pActor2.language;
		if (language == null && language2 == null)
		{
			return null;
		}
		if (language == null)
		{
			return language2;
		}
		if (language2 == null)
		{
			return language;
		}
		using ListPool<Language> listPool = new ListPool<Language>();
		int num = 3;
		int num2 = 3;
		if (pActor1.hasLanguage() && pActor1.language.hasTrait("melodic"))
		{
			num += LanguageTraitLibrary.getValue("melodic");
		}
		if (pActor2.hasLanguage() && pActor2.language.hasTrait("melodic"))
		{
			num2 += LanguageTraitLibrary.getValue("melodic");
		}
		if (pActor1.hasCity() && pActor1.language == pActor1.city.getLanguage())
		{
			num++;
		}
		if (pActor1.kingdom.hasLanguage() && pActor1.language == pActor1.kingdom.getLanguage())
		{
			num++;
		}
		if (pActor2.hasCity() && pActor2.language == pActor2.city.getLanguage())
		{
			num2++;
		}
		if (pActor2.kingdom.hasLanguage() && pActor2.language == pActor2.kingdom.getLanguage())
		{
			num2++;
		}
		listPool.AddTimes(num, language);
		listPool.AddTimes(num2, language2);
		return listPool.GetRandom();
	}

	private Culture decideCulture(Actor pActor1, Actor pActor2)
	{
		Culture culture = pActor1.culture;
		Culture culture2 = pActor2.culture;
		if (culture == null && culture2 == null)
		{
			return null;
		}
		if (culture == null)
		{
			return culture2;
		}
		if (culture2 == null)
		{
			return culture;
		}
		using ListPool<Culture> listPool = new ListPool<Culture>();
		int num = 3;
		int num2 = 3;
		if (pActor1.hasLanguage() && pActor1.language.hasTrait("melodic"))
		{
			num += LanguageTraitLibrary.getValue("melodic");
		}
		if (pActor2.hasLanguage() && pActor2.language.hasTrait("melodic"))
		{
			num2 += LanguageTraitLibrary.getValue("melodic");
		}
		if (pActor1.hasCity() && pActor1.culture == pActor1.city.getCulture())
		{
			num++;
		}
		if (pActor1.kingdom.hasCulture() && pActor1.culture == pActor1.kingdom.getCulture())
		{
			num++;
		}
		if (pActor2.hasCity() && pActor2.culture == pActor2.city.getCulture())
		{
			num2++;
		}
		if (pActor2.kingdom.hasCulture() && pActor2.culture == pActor2.kingdom.getCulture())
		{
			num2++;
		}
		listPool.AddTimes(num, culture);
		listPool.AddTimes(num2, culture2);
		return listPool.GetRandom();
	}

	private bool throwDiceForGift(Actor pActor, Actor pTarget)
	{
		bool num = pActor.isRelatedTo(pTarget) || pActor.isImportantTo(pTarget);
		float num2 = 0.2f;
		if (num)
		{
			num2 += 0.3f;
		}
		return Randy.randomChance(num2);
	}

	private void makeGift(Actor pActor, Actor pTarget)
	{
		bool flag = pTarget.tryToAcceptGift(pActor);
		int moneyForGift = pActor.getMoneyForGift();
		if (moneyForGift > 0)
		{
			pTarget.addMoney(moneyForGift);
		}
		if ((moneyForGift > 0) | flag)
		{
			pActor.changeHappiness("just_gave_gift");
			pTarget.changeHappiness("just_received_gift");
		}
	}
}
