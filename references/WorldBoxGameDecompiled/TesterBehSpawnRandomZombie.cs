public class TesterBehSpawnRandomZombie : TesterBehSpawnRandomUnit
{
	public TesterBehSpawnRandomZombie(int pAmount = 1)
		: base(pAmount)
	{
		filter_delegate = delegate(ActorAsset pActorAsset)
		{
			if (pActorAsset.isTemplateAsset())
			{
				return false;
			}
			return pActorAsset.id.Contains("zombie") ? true : false;
		};
	}
}
