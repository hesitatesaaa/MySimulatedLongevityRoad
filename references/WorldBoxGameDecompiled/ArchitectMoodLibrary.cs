using UnityEngine;

public class ArchitectMoodLibrary : AssetLibrary<ArchitectMood>
{
	public const string DEFAULT_MOOD = "serene";

	public override void init()
	{
		base.init();
		add(new ArchitectMood
		{
			id = "trickster",
			color_main = "#1cf713",
			color_text = "#1cf713"
		});
		add(new ArchitectMood
		{
			id = "benevolent",
			color_main = "#ffe90b",
			color_text = "#ffe90b"
		});
		add(new ArchitectMood
		{
			id = "malevolent",
			color_main = "#a00cfc",
			color_text = "#a00cfc"
		});
		add(new ArchitectMood
		{
			id = "serene",
			color_main = "#68FFFF",
			color_text = "#68FFFF"
		});
		add(new ArchitectMood
		{
			id = "chaotic",
			color_main = "#ff0e0e",
			color_text = "#ff0e0e"
		});
		add(new ArchitectMood
		{
			id = "orderly",
			color_main = "#ff870e",
			color_text = "#ff870e"
		});
		add(new ArchitectMood
		{
			id = "mysterious",
			color_main = "#f01fb4",
			color_text = "#f01fb4"
		});
		add(new ArchitectMood
		{
			id = "apathetic",
			color_main = "#000000",
			color_text = "#AAAAAA"
		});
		add(new ArchitectMood
		{
			id = "ethereal",
			color_main = "#73a18e",
			color_text = "#73a18e"
		});
	}

	public override void editorDiagnosticLocales()
	{
		foreach (ArchitectMood item in list)
		{
			string localeID = item.getLocaleID();
			checkLocale(item, localeID);
		}
		base.editorDiagnosticLocales();
	}

	public override void post_init()
	{
		base.post_init();
		foreach (ArchitectMood item in list)
		{
			item.path_icon = "ui/Icons/architect_moods/architect_mood_" + item.id;
		}
	}

	public override void editorDiagnostic()
	{
		foreach (ArchitectMood item in list)
		{
			if ((Object)(object)SpriteTextureLoader.getSprite(item.path_icon) == (Object)null)
			{
				BaseAssetLibrary.logAssetError("Missing icon file", item.path_icon);
			}
		}
		base.editorDiagnostic();
	}
}
