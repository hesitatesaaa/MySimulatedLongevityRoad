public class TesterBehSpawnRandomCivUnit : TesterBehSpawnRandomUnit
{
	public TesterBehSpawnRandomCivUnit(int pAmount = 1, string pLocation = "random")
		: base(pAmount, pLocation)
	{
		filter_delegate = delegate(ActorAsset pActorAsset)
		{
			if (pActorAsset.isTemplateAsset())
			{
				return false;
			}
			if (!pActorAsset.has_ai_system)
			{
				return false;
			}
			if (pActorAsset.is_boat)
			{
				return false;
			}
			if (pActorAsset.unit_other)
			{
				return false;
			}
			if (pActorAsset.special)
			{
				return false;
			}
			if (pActorAsset.id.Contains("zombie"))
			{
				return false;
			}
			return pActorAsset.civ ? true : false;
		};
	}
}
