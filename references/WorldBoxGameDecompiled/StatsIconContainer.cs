using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsIconContainer : MonoBehaviour
{
	private Dictionary<string, StatsIcon> _stats_icons = new Dictionary<string, StatsIcon>();

	protected void Awake()
	{
		StatsIcon[] componentsInChildren = ((Component)this).GetComponentsInChildren<StatsIcon>(true);
		foreach (StatsIcon statsIcon in componentsInChildren)
		{
			if (!_stats_icons.TryAdd(((Object)statsIcon).name, statsIcon))
			{
				Debug.LogError((object)("Duplicate icon name! " + ((Object)statsIcon).name));
			}
		}
		clear();
	}

	protected void OnDisable()
	{
		clear();
	}

	public bool TryGetValue(string pName, out StatsIcon pIcon)
	{
		return _stats_icons.TryGetValue(pName, out pIcon);
	}

	public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		StatsIcon iconViaId = getIconViaId(pName);
		if (!((Object)(object)iconViaId == (Object)null) && !iconViaId.areValuesTooClose(pMainVal))
		{
			iconViaId.setValue(pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
			iconViaId.textScaleAnimation();
		}
	}

	public void setText(string pName, string pText, string pColor)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		StatsIcon iconViaId = getIconViaId(pName);
		if (!((Object)(object)iconViaId == (Object)null))
		{
			iconViaId.enable_animation = false;
			iconViaId.checkDestroyTween();
			iconViaId.text.text = pText;
			((Graphic)iconViaId.text).color = Toolbox.makeColor(pColor);
		}
	}

	public StatsIcon getIconViaId(string pName)
	{
		_stats_icons.TryGetValue(pName, out var value);
		if ((Object)(object)value == (Object)null)
		{
			return null;
		}
		((Component)value).gameObject.SetActive(true);
		return value;
	}

	protected void clear()
	{
		foreach (StatsIcon value in _stats_icons.Values)
		{
			((Component)value).gameObject.SetActive(false);
		}
	}

	public void showGeneralIcons<TMetaObject, TData>(TMetaObject pMetaObject) where TMetaObject : MetaObject<TData>, IMetaObject where TData : MetaObjectData
	{
		int num = pMetaObject.countHoused();
		int num2 = pMetaObject.countHomeless();
		setIconValue("i_renown", pMetaObject.getRenown());
		setIconValue("i_age", pMetaObject.getAge());
		setIconValue("i_births", pMetaObject.getTotalBirths());
		setIconValue("i_kings", pMetaObject.countKings());
		setIconValue("i_leaders", pMetaObject.countLeaders());
		setIconValue("i_population", pMetaObject.countUnits());
		setIconValue("i_members", pMetaObject.countUnits());
		setIconValue("i_total_money", pMetaObject.countTotalMoney());
		setIconValue("i_adults", pMetaObject.countAdults());
		float pMainVal = pMetaObject.countChildren();
		string pColor = ((pMetaObject.getRatioChildren() > 0.5f) ? "#43FF43" : string.Empty);
		setIconValue("i_children", pMainVal, null, pColor);
		float pMainVal2 = pMetaObject.countMales();
		pColor = ((pMetaObject.getRatioMales() > 0.7f) ? "#43FF43" : string.Empty);
		setIconValue("i_males", pMainVal2, null, pColor);
		float pMainVal3 = pMetaObject.countFemales();
		pColor = ((pMetaObject.getRatioFemales() > 0.7f) ? "#43FF43" : string.Empty);
		setIconValue("i_females", pMainVal3, null, pColor);
		setIconValue("i_single_females", pMetaObject.countSingleFemales());
		setIconValue("i_single_males", pMetaObject.countSingleMales());
		setIconValue("i_families", pMetaObject.countFamilies());
		setIconValue("i_couples", pMetaObject.countCouples());
		setIconValue("i_deaths", pMetaObject.getTotalDeaths());
		float pMainVal4 = pMetaObject.countHappyUnits();
		pColor = ((pMetaObject.getRatioHappy() > 0.7f) ? "#43FF43" : string.Empty);
		setIconValue("i_happy_units", pMainVal4, null, pColor);
		float pMainVal5 = pMetaObject.countUnhappyUnits();
		pColor = ((pMetaObject.getRatioUnhappy() > 0.4f) ? "#FB2C21" : string.Empty);
		setIconValue("i_unhappy_units", pMainVal5, null, pColor);
		float pMainVal6 = pMetaObject.countSick();
		pColor = ((pMetaObject.getRatioSick() > 0.3f) ? "#FB2C21" : string.Empty);
		setIconValue("i_sick", pMainVal6, null, pColor);
		float pMainVal7 = pMetaObject.countHungry();
		pColor = ((pMetaObject.getRatioHungry() > 0.5f) ? "#FB2C21" : string.Empty);
		setIconValue("i_hungry", pMainVal7, null, pColor);
		float pMainVal8 = pMetaObject.countStarving();
		pColor = ((pMetaObject.getRatioStarving() > 0.3f) ? "#FB2C21" : string.Empty);
		setIconValue("i_starving", pMainVal8, null, pColor);
		float pMainVal9 = num;
		pColor = ((pMetaObject.getRatioHoused() > 0.8f) ? "#43FF43" : string.Empty);
		setIconValue("i_housed", pMainVal9, null, pColor);
		float pMainVal10 = num2;
		pColor = ((pMetaObject.getRatioHomeless() > 0.3f) ? "#FB2C21" : string.Empty);
		setIconValue("i_homeless", pMainVal10, null, pColor);
		setIconValue("i_kills", pMetaObject.getTotalKills());
	}
}
