public class MetaTextReportHelper
{
	public static string color_text_main => ColorStyleLibrary.m.color_text_grey;

	public static string color_text_quote => ColorStyleLibrary.m.color_text_grey_dark;

	public static string addSingleUnitText(Actor pActor, bool pAddGap = true, bool pAddNameQuote = true)
	{
		if (!pActor.hasHappinessHistory())
		{
			return string.Empty;
		}
		using ListPool<HappinessHistory> list = new ListPool<HappinessHistory>(pActor.happiness_change_history);
		HappinessHistory random = list.GetRandom();
		string randomTextSingleReportLocalized = random.asset.getRandomTextSingleReportLocalized();
		string pString = "<i>\"" + randomTextSingleReportLocalized + "\"</i>";
		string pString2 = "\n— " + pActor.name;
		string text = pActor.getAge().ToString();
		text = ((!pActor.isSexFemale()) ? (text + " M") : (text + " F"));
		string text2 = random.getAgoString().ColorHex(ColorStyleLibrary.m.color_text_grey_dark);
		string text3 = "";
		if (pAddGap)
		{
			text3 = "\n\n";
		}
		text3 = text3 + pString.ColorHex(color_text_quote) + "  " + text2;
		if (pAddNameQuote)
		{
			text3 = text3 + pString2.ColorHex(pActor.kingdom.getColor().color_text) + "  " + text.ColorHex(ColorStyleLibrary.m.color_text_grey_dark);
		}
		return text3;
	}

	public static string addSingleUnitTextRandomUnit(IMetaObject pMetaObject, out Actor pActorResult)
	{
		pActorResult = null;
		int num = 10;
		while (num-- > 0)
		{
			Actor randomUnit = pMetaObject.getRandomUnit();
			if (randomUnit != null && randomUnit.isAlive() && randomUnit.hasHappinessHistory())
			{
				string text = addSingleUnitText(randomUnit);
				if (!string.IsNullOrEmpty(text))
				{
					pActorResult = randomUnit;
					return text;
				}
			}
		}
		return string.Empty;
	}

	public static string getText(IMetaObject pMetaObject, MetaTypeAsset pAsset, out Actor pActorResult)
	{
		pActorResult = null;
		string text = string.Empty;
		bool flag = false;
		string[] reports = pAsset.reports;
		foreach (string pID in reports)
		{
			MetaTextReportAsset metaTextReportAsset = AssetManager.meta_text_report_library.get(pID);
			if (metaTextReportAsset.report_action(pMetaObject))
			{
				if (flag)
				{
					text += " ";
				}
				flag = true;
				string text2 = metaTextReportAsset.get_random_text;
				if (metaTextReportAsset.color != null)
				{
					text2 = text2.ColorHex(metaTextReportAsset.color);
				}
				text += text2;
			}
		}
		if (flag)
		{
			text += addSingleUnitTextRandomUnit(pMetaObject, out var pActorResult2);
			pActorResult = pActorResult2;
			text = text.ColorHex(color_text_main);
		}
		return text;
	}
}
