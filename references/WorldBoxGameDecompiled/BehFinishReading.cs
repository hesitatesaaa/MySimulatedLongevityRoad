using ai.behaviours;

public class BehFinishReading : BehCitizenActionCity
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_books = true;
		uses_religions = true;
		uses_languages = true;
		uses_cultures = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Book beh_book_target = pActor.beh_book_target;
		if (beh_book_target == null || !beh_book_target.isAlive())
		{
			return BehResult.Stop;
		}
		checkBookTrait(pActor, beh_book_target);
		checkBookValueBonuses(pActor, beh_book_target);
		checkBookAttributes(pActor, beh_book_target);
		checkSpecialBookRewards(pActor, beh_book_target);
		tryToConvertActorToMetaFromBook(pActor, beh_book_target);
		checkBookAssetAction(pActor, beh_book_target);
		tryToGetMetaTraitsFromBook(pActor, beh_book_target);
		beh_book_target.increaseReadTimes();
		return BehResult.Continue;
	}

	private void checkBookAttributes(Actor pActor, Book pBook)
	{
		foreach (BaseStatsContainer item in pBook.getBaseStats().getList())
		{
			if (item.asset.actor_data_attribute)
			{
				pActor.data[item.id] += item.value;
			}
		}
	}

	private void checkBookAssetAction(Actor pActor, Book pBook)
	{
		BookTypeAsset asset = pBook.getAsset();
		asset.read_action?.Invoke(pActor, asset);
	}

	private void checkSpecialBookRewards(Actor pActor, Book pBook)
	{
		foreach (LanguageTrait trait in pBook.getLanguage().getTraits())
		{
			trait.read_book_trait_action?.Invoke(pActor, trait, pBook);
		}
	}

	private void checkBookValueBonuses(Actor pActor, Book pBook)
	{
		int num = pBook.getHappiness();
		int num2 = pBook.getExperience();
		int mana = pBook.getMana();
		if (pActor.hasCulture())
		{
			if (pActor.culture.hasTrait("reading_lovers") && num < 0)
			{
				num *= -1;
			}
			if (pActor.culture.hasTrait("attentive_readers"))
			{
				num2 *= (int)((float)num2 * CultureTraitLibrary.getValueFloat("attentive_readers"));
			}
		}
		pActor.changeHappiness("just_read_book", num);
		pActor.addExperience(num2);
		pActor.addMana(mana);
	}

	private void checkBookTrait(Actor pActor, Book pBook)
	{
		if (Randy.randomBool())
		{
			ActorTrait bookTraitActor = pBook.getBookTraitActor();
			if (bookTraitActor != null)
			{
				pActor.addTrait(bookTraitActor);
			}
		}
	}

	private void tryToConvertActorToMetaFromBook(Actor pActor, Book pBook)
	{
		tryToConvertActorToBookCulture(pActor, pBook);
		tryToConvertActorToBookLanguage(pActor, pBook);
		tryToConvertActorToBookReligion(pActor, pBook);
	}

	private void tryToGetMetaTraitsFromBook(Actor pActor, Book pBook)
	{
		if (pActor.isKing() || pActor.isCityLeader())
		{
			tryToGetMetaTraitFromBookCulture(pActor, pBook);
			tryToGetMetaTraitFromBookLanguage(pActor, pBook);
			tryToGetMetaTraitFromBookReligion(pActor, pBook);
		}
	}

	private void tryToGetMetaTraitFromBookCulture(Actor pActor, Book pBook)
	{
		if (pActor.hasCulture())
		{
			CultureTrait bookTraitCulture = pBook.getBookTraitCulture();
			if (bookTraitCulture != null && Randy.randomBool())
			{
				pActor.culture.addTrait(bookTraitCulture);
			}
		}
	}

	private void tryToGetMetaTraitFromBookLanguage(Actor pActor, Book pBook)
	{
		if (pActor.hasLanguage())
		{
			LanguageTrait bookTraitLanguage = pBook.getBookTraitLanguage();
			if (bookTraitLanguage != null && Randy.randomBool())
			{
				pActor.language.addTrait(bookTraitLanguage);
			}
		}
	}

	private void tryToGetMetaTraitFromBookReligion(Actor pActor, Book pBook)
	{
		if (pActor.hasReligion())
		{
			ReligionTrait bookTraitReligion = pBook.getBookTraitReligion();
			if (bookTraitReligion != null && Randy.randomBool())
			{
				pActor.religion.addTrait(bookTraitReligion);
			}
		}
	}

	private void tryToConvertActorToBookReligion(Actor pActor, Book pBook)
	{
		Religion religion = pBook.getReligion();
		if (religion == null || pActor.religion == religion)
		{
			return;
		}
		using ListPool<Religion> listPool = new ListPool<Religion>(6);
		if (pActor.hasReligion())
		{
			listPool.AddTimes(3, pActor.religion);
			if (hasStylishWritingActor(pActor))
			{
				listPool.AddTimes(getStylishWritingValue(), pActor.religion);
			}
		}
		listPool.AddTimes(3, religion);
		if (hasStylishWritingBook(pBook))
		{
			listPool.AddTimes(getStylishWritingValue(), religion);
		}
		Religion random = listPool.GetRandom();
		if (random != pActor.religion)
		{
			pActor.tryToConvertToReligion(random);
		}
	}

	private void tryToConvertActorToBookLanguage(Actor pActor, Book pBook)
	{
		Language language = pBook.getLanguage();
		if (language == null || pActor.language == language)
		{
			return;
		}
		using ListPool<Language> listPool = new ListPool<Language>();
		if (pActor.hasLanguage())
		{
			listPool.AddTimes(3, pActor.language);
			if (hasStylishWritingActor(pActor))
			{
				listPool.AddTimes(getStylishWritingValue(), pActor.language);
			}
		}
		listPool.AddTimes(3, language);
		if (hasStylishWritingBook(pBook))
		{
			listPool.AddTimes(getStylishWritingValue(), language);
		}
		Language random = listPool.GetRandom();
		if (random != pActor.language)
		{
			pActor.tryToConvertToLanguage(random);
		}
	}

	private void tryToConvertActorToBookCulture(Actor pActor, Book pBook)
	{
		Culture culture = pBook.getCulture();
		if (culture == null)
		{
			return;
		}
		Culture culture2 = pActor.culture;
		if (culture2 == culture)
		{
			return;
		}
		using ListPool<Culture> listPool = new ListPool<Culture>();
		if (pActor.hasCulture())
		{
			listPool.AddTimes(3, culture2);
			if (hasStylishWritingActor(pActor))
			{
				listPool.AddTimes(getStylishWritingValue(), culture2);
			}
		}
		listPool.AddTimes(3, culture);
		if (hasStylishWritingBook(pBook))
		{
			listPool.AddTimes(getStylishWritingValue(), culture);
		}
		Culture random = listPool.GetRandom();
		if (random != culture2)
		{
			pActor.tryToConvertToCulture(random);
		}
	}

	private bool hasStylishWritingActor(Actor pActor)
	{
		if (pActor.hasLanguage() && pActor.language.hasTrait("stylish_writing"))
		{
			return true;
		}
		return false;
	}

	private bool hasStylishWritingBook(Book pBook)
	{
		if (pBook.getLanguage().hasTrait("stylish_writing"))
		{
			return true;
		}
		return false;
	}

	private int getStylishWritingValue()
	{
		return LanguageTraitLibrary.getValue("stylish_writing");
	}
}
