using System.Collections.Generic;

public class TooltipLanguageTraitsRow : TooltipTraitsRow<LanguageTrait>
{
	protected override IReadOnlyCollection<LanguageTrait> traits_hashset => tooltip_data.language.getTraits();
}
