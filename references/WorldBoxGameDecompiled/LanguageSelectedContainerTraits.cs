using System.Collections.Generic;

public class LanguageSelectedContainerTraits : SelectedContainerTraits<LanguageTrait, LanguageTraitButton, LanguageTraitsContainer, LanguageTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Language;

	protected override IReadOnlyCollection<LanguageTrait> getTraits()
	{
		return SelectedMetas.selected_language.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
