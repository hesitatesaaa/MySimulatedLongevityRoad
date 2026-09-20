public class ChromosomeTypeLibrary : AssetLibrary<ChromosomeTypeAsset>
{
	public const int LOCI_PER_ROW = 6;

	public override void init()
	{
		base.init();
		add(new ChromosomeTypeAsset
		{
			id = "chromosome_big",
			amount_loci_min_amplifier = 1,
			amount_loci_max_amplifier = 4,
			amount_loci_min_empty = 4,
			amount_loci_max_empty = 8,
			amount_loci = 30,
			name = "chromosome_big",
			description = "some chromosome"
		});
		add(new ChromosomeTypeAsset
		{
			id = "chromosome_medium",
			amount_loci_min_amplifier = 1,
			amount_loci_max_amplifier = 3,
			amount_loci_min_empty = 3,
			amount_loci_max_empty = 5,
			amount_loci = 24,
			name = "chromosome_medium",
			description = "some chromosome"
		});
		add(new ChromosomeTypeAsset
		{
			id = "chromosome_small",
			amount_loci_min_amplifier = 1,
			amount_loci_max_amplifier = 3,
			amount_loci_min_empty = 2,
			amount_loci_max_empty = 4,
			amount_loci = 18,
			name = "chromosome_small",
			description = "some chromosome"
		});
		add(new ChromosomeTypeAsset
		{
			id = "chromosome_tiny",
			amount_loci_min_amplifier = 0,
			amount_loci_max_amplifier = 2,
			amount_loci_min_empty = 1,
			amount_loci_max_empty = 3,
			amount_loci = 12,
			name = "chromosome_tiny",
			description = "some chromosome"
		});
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (ChromosomeTypeAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}
}
