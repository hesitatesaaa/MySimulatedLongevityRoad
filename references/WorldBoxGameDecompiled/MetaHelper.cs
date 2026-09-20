public static class MetaHelper
{
	public static void addRandomTrait<TTrait>(ITraitsOwner<TTrait> pMetaObject, BaseTraitLibrary<TTrait> pLibrary) where TTrait : BaseTrait<TTrait>
	{
		int pMinInclusive = 1;
		int pMaxExclusive = 3;
		if (WorldLawLibrary.world_law_glitched_noosphere.isEnabled())
		{
			pMinInclusive = 3;
			pMaxExclusive = 6;
		}
		int num = Randy.randomInt(pMinInclusive, pMaxExclusive);
		for (int i = 0; i < num; i++)
		{
			TTrait randomSpawnTrait = pLibrary.getRandomSpawnTrait();
			if (randomSpawnTrait.isAvailable())
			{
				pMetaObject.addTrait(randomSpawnTrait, pRemoveOpposites: true);
			}
		}
	}
}
