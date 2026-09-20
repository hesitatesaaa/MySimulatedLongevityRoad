public class PersonalityLibrary : AssetLibrary<PersonalityAsset>
{
	public override void init()
	{
		base.init();
		add(new PersonalityAsset
		{
			id = "administrator"
		});
		t.base_stats["personality_diplomatic"] = 0.1f;
		t.base_stats["personality_administration"] = 0.5f;
		t.base_stats["personality_aggression"] = 0.1f;
		add(new PersonalityAsset
		{
			id = "militarist"
		});
		t.base_stats["personality_diplomatic"] = 0.05f;
		t.base_stats["personality_administration"] = 0.1f;
		t.base_stats["personality_aggression"] = 0.5f;
		add(new PersonalityAsset
		{
			id = "diplomat"
		});
		t.base_stats["personality_diplomatic"] = 0.5f;
		t.base_stats["personality_aggression"] = 0.05f;
		t.base_stats["personality_administration"] = 0.2f;
		add(new PersonalityAsset
		{
			id = "balanced"
		});
		t.base_stats["personality_administration"] = 0.1f;
		t.base_stats["personality_diplomatic"] = 0.1f;
		t.base_stats["personality_aggression"] = 0.1f;
		add(new PersonalityAsset
		{
			id = "wildcard"
		});
		t.base_stats["personality_rationality"] = -1f;
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (PersonalityAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
