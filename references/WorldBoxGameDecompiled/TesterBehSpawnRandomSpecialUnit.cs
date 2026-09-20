public class TesterBehSpawnRandomSpecialUnit : TesterBehSpawnRandomUnit
{
	public TesterBehSpawnRandomSpecialUnit(int pAmount = 1)
		: base(pAmount)
	{
		filter_delegate = delegate(ActorAsset pActorAsset)
		{
			if (pActorAsset.isTemplateAsset())
			{
				return false;
			}
			if (pActorAsset.unit_other)
			{
				return true;
			}
			return pActorAsset.special ? true : false;
		};
	}
}
