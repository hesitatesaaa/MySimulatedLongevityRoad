using System.Collections.Generic;

public interface ITraitsContainer<TTrait, TTraitButton> where TTrait : BaseTrait<TTrait> where TTraitButton : TraitButton<TTrait>
{
	IReadOnlyCollection<TTraitButton> getTraitButtons();
}
