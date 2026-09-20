using System.Collections;
using UnityEngine;

public class WarStatsElement : WarElement, IStatsElement, IRefreshElement
{
	private StatsIconContainer _stats_icons;

	public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		_stats_icons.setIconValue(pName, pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
	}

	protected override void Awake()
	{
		_stats_icons = ((Component)this).gameObject.AddOrGetComponent<StatsIconContainer>();
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (base.war != null && base.war.isAlive())
		{
			setIconValue("i_age", base.war.getAge());
			setIconValue("i_population", base.war.countTotalPopulation());
			setIconValue("i_total_army", base.war.countTotalArmy());
			setIconValue("i_kingdoms", base.war.countKingdoms());
			setIconValue("i_cities", base.war.countCities());
			setIconValue("i_deaths", base.war.getTotalDeaths());
			bool flag = base.war.countAttackersPopulation() > base.war.countDefendersPopulation();
			bool flag2 = base.war.getDeadDefenders() > base.war.getDeadAttackers();
			bool flag3 = base.war.countAttackersWarriors() > base.war.countDefendersWarriors();
			bool flag4 = base.war.countAttackersCities() > base.war.countDefendersCities();
			WarStatsElement warStatsElement = this;
			float pMainVal = base.war.countAttackersPopulation();
			string pColor = (flag ? "#43FF43" : "#FB2C21");
			warStatsElement.setIconValue("i_attackers_population", pMainVal, null, pColor);
			WarStatsElement warStatsElement2 = this;
			float pMainVal2 = base.war.countAttackersWarriors();
			pColor = (flag3 ? "#43FF43" : "#FB2C21");
			warStatsElement2.setIconValue("i_attackers_army", pMainVal2, null, pColor);
			WarStatsElement warStatsElement3 = this;
			float pMainVal3 = base.war.getDeadAttackers();
			pColor = (flag2 ? "#43FF43" : "#FB2C21");
			warStatsElement3.setIconValue("i_attackers_dead", pMainVal3, null, pColor);
			WarStatsElement warStatsElement4 = this;
			float pMainVal4 = base.war.countAttackersCities();
			pColor = (flag4 ? "#43FF43" : "#FB2C21");
			warStatsElement4.setIconValue("i_attackers_cities", pMainVal4, null, pColor);
			WarStatsElement warStatsElement5 = this;
			float pMainVal5 = base.war.countDefendersPopulation();
			pColor = (flag ? "#FB2C21" : "#43FF43");
			warStatsElement5.setIconValue("i_defenders_population", pMainVal5, null, pColor);
			WarStatsElement warStatsElement6 = this;
			float pMainVal6 = base.war.countDefendersWarriors();
			pColor = (flag3 ? "#FB2C21" : "#43FF43");
			warStatsElement6.setIconValue("i_defenders_army", pMainVal6, null, pColor);
			WarStatsElement warStatsElement7 = this;
			float pMainVal7 = base.war.getDeadDefenders();
			pColor = (flag2 ? "#FB2C21" : "#43FF43");
			warStatsElement7.setIconValue("i_defenders_dead", pMainVal7, null, pColor);
			WarStatsElement warStatsElement8 = this;
			float pMainVal8 = base.war.countDefendersCities();
			pColor = (flag4 ? "#FB2C21" : "#43FF43");
			warStatsElement8.setIconValue("i_defenders_cities", pMainVal8, null, pColor);
		}
		yield break;
	}
}
