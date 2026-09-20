using System.Collections;

public class WarAttackersContainer : WarBannersContainer
{
	protected override IEnumerator showContent()
	{
		bool tHasWon = false;
		bool tHasLost = false;
		switch (base.war.data.winner)
		{
		case WarWinner.Attackers:
			tHasWon = true;
			break;
		case WarWinner.Defenders:
			tHasLost = true;
			break;
		}
		foreach (Kingdom attacker in base.war.getAttackers())
		{
			yield return showBanner(attacker, pLeft: false, tHasWon, tHasLost);
		}
		foreach (Kingdom diedAttacker in base.war.getDiedAttackers())
		{
			yield return showBanner(diedAttacker);
		}
		foreach (Kingdom pastAttacker in base.war.getPastAttackers())
		{
			yield return showBanner(pastAttacker, pLeft: true);
		}
	}
}
