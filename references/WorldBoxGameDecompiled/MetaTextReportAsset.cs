using System;
using System.Collections.Generic;

[Serializable]
public class MetaTextReportAsset : Asset, IMultiLocalesAsset
{
	public MetaTextReportAction report_action;

	public string color;

	public int amount = 5;

	internal string get_locale_id => "meta_report_" + id + "_";

	internal string get_random_text
	{
		get
		{
			int num = Randy.randomInt(0, amount);
			return LocalizedTextManager.getText($"{get_locale_id}{num}");
		}
	}

	public IEnumerable<string> getLocaleIDs()
	{
		for (int i = 0; i < amount; i++)
		{
			yield return $"{get_locale_id}{i}";
		}
	}
}
