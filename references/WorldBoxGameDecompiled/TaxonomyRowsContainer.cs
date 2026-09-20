using UnityEngine.Events;

public class TaxonomyRowsContainer : StatsRowsContainer
{
	protected override void showStats()
	{
		showTaxonomicRank("taxonomy_kingdom");
		showTaxonomicRank("taxonomy_phylum");
		showTaxonomicRank("taxonomy_class");
		showTaxonomicRank("taxonomy_order");
		showTaxonomicRank("taxonomy_family");
		showTaxonomicRank("taxonomy_genus");
		Subspecies selected_subspecies = SelectedMetas.selected_subspecies;
		StatsWindow.tryToShowMetaSpecies("species", selected_subspecies.data.species_id, this);
	}

	private void showTaxonomicRank(string pTaxonomyRank)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		Subspecies tSubspecies = SelectedMetas.selected_subspecies;
		string colorForTaxonomy = ColorStyleLibrary.m.getColorForTaxonomy(pTaxonomyRank);
		string taxonomyRank = AssetManager.actor_library.get(tSubspecies.data.species_id).getTaxonomyRank(pTaxonomyRank);
		if (!string.IsNullOrEmpty(taxonomyRank))
		{
			taxonomyRank = Toolbox.firstLetterToUpper(taxonomyRank);
			KeyValueField tField = showStatRow(pTaxonomyRank, taxonomyRank, colorForTaxonomy, MetaType.None, -1L, pColorText: true);
			tField.on_hover_value = (UnityAction)delegate
			{
				showTooltipTaxonomy(pTaxonomyRank, tSubspecies, tField);
			};
			tField.on_hover_value_out = new UnityAction(Tooltip.hideTooltip);
		}
	}

	private void showTooltipTaxonomy(string pRankType, Subspecies pSpecies, KeyValueField pField)
	{
		TooltipData pData = new TooltipData
		{
			subspecies = pSpecies,
			tip_name = pRankType
		};
		Tooltip.show(pField, "taxonomy", pData);
	}
}
