using ai.behaviours;

public class BehTryToRead : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.city.hasBooksToRead(pActor))
		{
			return BehResult.Stop;
		}
		Book book;
		if (pActor.hasTag("can_read_any_book"))
		{
			book = pActor.city.getRandomBook();
		}
		else
		{
			if (!pActor.hasLanguage())
			{
				return BehResult.Stop;
			}
			if (!pActor.city.hasBooksOfLanguage(pActor.language))
			{
				return BehResult.Stop;
			}
			book = pActor.city.getRandomBookOfLanguage(pActor.language);
		}
		if (book == null)
		{
			return BehResult.Stop;
		}
		book.readIt();
		pActor.beh_book_target = book;
		return BehResult.Continue;
	}
}
