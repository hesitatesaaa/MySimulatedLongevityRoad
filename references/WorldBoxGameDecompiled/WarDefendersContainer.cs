using System.Collections;

public class WarDefendersContainer : WarBannersContainer
{
	protected override IEnumerator showContent()
	{
		bool tHasWon = false;
		bool tHasLost = false;
		switch (base.war.data.winner)
		{
		case WarWinner.Defenders:
			tHasWon = true;
			break;
		case WarWinner.Attackers:
			tHasLost = true;
			break;
		}
		foreach (Kingdom defender in base.war.getDefenders())
		{
			yield return showBanner(defender, pLeft: false, tHasWon, tHasLost);
		}
		foreach (Kingdom diedDefender in base.war.getDiedDefenders())
		{
			yield return showBanner(diedDefender);
		}
		foreach (Kingdom pastDefender in base.war.getPastDefenders())
		{
			yield return showBanner(pastDefender, pLeft: true);
		}
	}
}
