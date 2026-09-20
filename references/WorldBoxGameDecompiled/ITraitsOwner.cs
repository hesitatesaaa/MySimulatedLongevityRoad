using System.Collections.Generic;

public interface ITraitsOwner<TTrait> where TTrait : BaseTrait<TTrait>
{
	bool hasTrait(TTrait pTraitId);

	bool addTrait(TTrait pTraitId, bool pRemoveOpposites = false);

	bool removeTrait(TTrait pTrait);

	IReadOnlyCollection<TTrait> getTraits();

	bool hasTraits();

	void sortTraits(IReadOnlyCollection<TTrait> pTraits);

	void traitModifiedEvent();

	ActorAsset getActorAsset();
}
