using ai.behaviours;

public class BehClanChiefCheckMembersToBanish : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Clan clan = pActor.clan;
		for (int i = 0; i < clan.units.Count; i++)
		{
			Actor actor = clan.units[i];
			if (actor != pActor && pActor.areFoes(actor))
			{
				actor.setClan(null);
			}
		}
		return BehResult.Continue;
	}
}
