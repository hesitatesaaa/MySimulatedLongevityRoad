public class TesterBehSpawnRandomBoat : TesterBehSpawnRandomUnit
{
	public TesterBehSpawnRandomBoat(int pAmount = 1)
		: base(pAmount)
	{
		filter_delegate = delegate(ActorAsset pActorAsset)
		{
			if (pActorAsset.isTemplateAsset())
			{
				return false;
			}
			return pActorAsset.is_boat ? true : false;
		};
	}
}
