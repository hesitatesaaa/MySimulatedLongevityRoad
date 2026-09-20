using System.Collections.Generic;
using UnityEngine;

public class MetaRepresentationContainer : MetaRepresentationContainerBase
{
	private IMetaWindow _meta_window;

	protected override void init()
	{
		base.init();
		_meta_window = ((Component)this).GetComponentInParent<IMetaWindow>();
	}

	protected override void fillDict(ref int pTotal, ref bool pAny, Dictionary<IMetaObject, int> pDict)
	{
		foreach (Actor unit in getMetaObject().getUnits())
		{
			pTotal++;
			if (asset.check_has_meta(unit))
			{
				pAny = true;
				IMetaObject key = asset.meta_getter(unit);
				if (!pDict.ContainsKey(key))
				{
					pDict.Add(key, 0);
				}
				pDict[key]++;
			}
		}
	}

	protected override void checkShowNone(bool pAny, int pNone, int pTotal)
	{
		if (pAny && asset.show_none_percent && pNone > 0)
		{
			string pValue = amountWithPercent(pNone, pTotal);
			KeyValueField pField = showStatRow("statistics_breakdown_none", pValue, ColorStyleLibrary.m.color_text_grey, MetaType.None, -1L, pColorText: true, asset.general_icon_path);
			showBar(pField, pNone, pTotal, ColorStyleLibrary.m.color_text_grey);
		}
	}

	private IMetaObject getMetaObject()
	{
		return _meta_window.getCoreObject() as IMetaObject;
	}
}
