public class TraitGroupElement<TTrait, TTraitButton, TTraitEditorButton> : AugmentationCategory<TTrait, TTraitButton, TTraitEditorButton> where TTrait : BaseTrait<TTrait> where TTraitButton : TraitButton<TTrait> where TTraitEditorButton : TraitEditorButton<TTraitButton, TTrait>
{
	protected override bool isUnlocked(TTraitButton pButton)
	{
		if (pButton.getElementAsset().isAvailable())
		{
			return true;
		}
		return false;
	}
}
