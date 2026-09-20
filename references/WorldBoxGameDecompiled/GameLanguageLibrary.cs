using UnityEngine;

public class GameLanguageLibrary : AssetLibrary<GameLanguageAsset>
{
	public override void init()
	{
		base.init();
		add("en", "English");
		t.main = true;
		add("de", "Deutsch");
		add("pl", "Polski");
		add("cz", "中文(简体)");
		t.is_hanzi = true;
		t.force_style = new ForcedFontStyle((FontStyle)0, pShadow: true);
		t.font = () => LocalizedTextManager.instance.simplified_chinese_font;
		add("ch", "中文(繁體)");
		t.is_hanzi = true;
		t.force_style = new ForcedFontStyle((FontStyle)0, pShadow: true);
		add("ko", "한국어");
		t.font = () => LocalizedTextManager.instance.korean_font;
		add("ja", "日本語");
		t.is_hanzi = true;
		t.force_style = new ForcedFontStyle((FontStyle)0, pShadow: true);
		t.font = () => LocalizedTextManager.instance.japanese_font;
		add("th", "ภาษาไทย");
		t.font = () => LocalizedTextManager.instance.thai_font;
		add("vn", "Tiếng Việt");
		add("ru", "Русский");
		t.show_translators = false;
		add("ua", "Українська");
		add("by", "Беларуская");
		add("es", "Español");
		add("br", "Português do Brasil");
		add("pt", "Português de Portugal");
		add("fr", "Français");
		add("it", "Italiano");
		add("ro", "Română");
		add("hr", "Hrvatski");
		add("tr", "Türkçe");
		add("gr", "Ελληνικά");
		add("ka", "ქართული");
		t.font = () => LocalizedTextManager.instance.arabic_font;
		add("sk", "Slovenčina");
		add("cs", "Čeština");
		add("hu", "Magyar");
		add("nl", "Nederlands");
		add("id", "Bahasa Indonesia");
		add("sv", "Svenska");
		add("no", "Norsk");
		add("da", "Dansk");
		add("fa", "فارسی");
		t.is_rtl = true;
		t.font = () => LocalizedTextManager.instance.persian_font;
		add("ar", "العربية");
		t.is_rtl = true;
		t.font = () => LocalizedTextManager.instance.arabic_font;
		add("he", "ע\u05b4בר\u05b4ית");
		t.is_rtl = true;
		add("fn", "Suomi");
		add("ph", "Tagalog");
		add("hi", "ह\u093fन\u094dद\u0940");
		t.is_hindi = true;
		t.font = () => LocalizedTextManager.instance.hindi_font;
		t.force_style = new ForcedFontStyle((FontStyle)1);
		add("lb", "Lëtzebuergesch");
		add("lt", "Lithuanian");
		add(new GameLanguageAsset
		{
			id = "boat",
			name = "Boat",
			export = false,
			path_icon = "ui/Icons/iconSeaborn"
		});
		add(new GameLanguageAsset
		{
			id = "keys",
			name = "KEYS",
			export = false,
			debug_only = true,
			path_icon = "ui/Icons/iconDebug"
		});
	}

	public GameLanguageAsset add(string pID, string pName)
	{
		return add(new GameLanguageAsset
		{
			id = pID,
			name = pName
		});
	}
}
