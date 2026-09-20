public class WarTypeLibrary : AssetLibrary<WarTypeAsset>
{
	public static WarTypeAsset normal;

	public static WarTypeAsset spite;

	public static WarTypeAsset inspire;

	public static WarTypeAsset rebellion;

	public static WarTypeAsset whisper_of_war;

	public static WarTypeAsset clash;

	public override void init()
	{
		base.init();
		normal = add(new WarTypeAsset
		{
			id = "normal",
			name_template = "war_conquest",
			localized_type = "war_type_conquest",
			localized_war_name = "war_name_conquest",
			path_icon = "wars/war_conquest",
			kingdom_for_name_attacker = true,
			alliance_join = true,
			can_end_with_plot = true
		});
		spite = add(new WarTypeAsset
		{
			id = "spite",
			name_template = "war_spite",
			localized_type = "war_type_spite",
			localized_war_name = "war_name_spite",
			path_icon = "wars/war_spite",
			kingdom_for_name_attacker = true,
			forced_war = true,
			total_war = true,
			alliance_join = false
		});
		inspire = add(new WarTypeAsset
		{
			id = "inspire",
			name_template = "war_inspire",
			localized_type = "war_type_inspire",
			localized_war_name = "war_name_inspire",
			path_icon = "wars/war_rebellion",
			kingdom_for_name_attacker = false,
			alliance_join = false,
			rebellion = true,
			can_end_with_plot = true
		});
		rebellion = add(new WarTypeAsset
		{
			id = "rebellion",
			name_template = "war_rebellion",
			localized_type = "war_type_rebellion",
			localized_war_name = "war_name_rebellion",
			path_icon = "wars/war_rebellion",
			kingdom_for_name_attacker = false,
			alliance_join = false,
			rebellion = true,
			can_end_with_plot = true
		});
		whisper_of_war = add(new WarTypeAsset
		{
			id = "whisper_of_war",
			name_template = "war_whisper",
			localized_type = "war_type_whisper",
			localized_war_name = "war_name_whisper",
			path_icon = "wars/war_whisper",
			kingdom_for_name_attacker = true,
			alliance_join = true
		});
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (WarTypeAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
	}
}
