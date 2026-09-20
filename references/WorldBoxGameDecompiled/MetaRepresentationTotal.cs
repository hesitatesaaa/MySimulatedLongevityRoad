using System.Collections.Generic;

public class MetaRepresentationTotal : MetaRepresentationContainerBase
{
	protected override void fillDict(ref int pTotal, ref bool pAny, Dictionary<IMetaObject, int> pDict)
	{
		List<Actor> list = asset.world_units_getter();
		pTotal = list.Count;
		foreach (Actor item in list)
		{
			if (!asset.check_has_meta(item))
			{
				if (!asset.show_none_percent_for_total)
				{
					pTotal--;
				}
				continue;
			}
			pAny = true;
			using ListPool<IMetaObject> listPool = asset.meta_getter_total(item);
			foreach (ref IMetaObject item2 in listPool)
			{
				IMetaObject current2 = item2;
				if (!pDict.ContainsKey(current2))
				{
					pDict.Add(current2, 0);
				}
				pDict[current2]++;
			}
		}
	}

	protected override void checkShowNone(bool pAny, int pNone, int pTotal)
	{
		if ((asset.show_none_percent_for_total & pAny) && pNone > 0)
		{
			string text = "statistics_breakdown_none_list".Localize();
			text += Toolbox.coloredGreyPart(pNone, ColorStyleLibrary.m.color_text_grey);
			string pValue = amountWithPercent(pNone, pTotal);
			KeyValueField pField = showStatRow(text, pValue, ColorStyleLibrary.m.color_text_grey, MetaType.None, -1L, pColorText: true, asset.general_icon_path, null, null, pLocalize: false);
			showBar(pField, pNone, pTotal, ColorStyleLibrary.m.color_text_grey);
		}
	}
}
