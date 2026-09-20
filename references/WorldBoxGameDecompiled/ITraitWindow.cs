using System.Collections.Generic;

public interface ITraitWindow<TTrait, TTraitButton> : IAugmentationsWindow<ITraitsEditor<TTrait>> where TTrait : BaseTrait<TTrait> where TTraitButton : TraitButton<TTrait>
{
	TraitsContainer<TTrait, TTraitButton> getContainer()
	{
		return GetComponentInChildren<TraitsContainer<TTrait, TTraitButton>>();
	}

	void reloadTraits(bool pAnimated = true)
	{
		getContainer().reloadTraits(pAnimated);
	}

	ITraitsOwner<TTrait> getTraitsOwner()
	{
		return getEditor().getTraitsOwner();
	}

	IReadOnlyCollection<TTrait> getTraits()
	{
		return getTraitsOwner().getTraits();
	}

	void sortTraits(IReadOnlyCollection<TTrait> pTraits)
	{
		getTraitsOwner().sortTraits(pTraits);
	}

	bool hasTraits()
	{
		return getTraitsOwner().hasTraits();
	}
}
