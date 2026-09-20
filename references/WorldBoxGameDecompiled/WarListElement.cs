using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarListElement : WindowListElementBase<War, WarData>
{
	public Text text_name;

	public LocalizedText war_type;

	public CountUpOnClick age;

	public CountUpOnClick duration;

	public CountUpOnClick kingdoms;

	public CountUpOnClick cities;

	public CountUpOnClick renown;

	public CountUpOnClick dead;

	public KingdomBanner prefabMiniKingdomBanner;

	public GameObject gridAttackers;

	public GameObject gridDefenders;

	protected ObjectPoolGenericMono<KingdomBanner> pool_mini_banners_attackers;

	protected ObjectPoolGenericMono<KingdomBanner> pool_mini_banners_defenders;

	public Image total_war_icon;

	internal override void show(War pWar)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.show(pWar);
		text_name.text = pWar.data.name;
		((Graphic)text_name).color = pWar.getColor().getColorText();
		war_type.setKeyAndUpdate(pWar.getAsset().localized_war_name);
		kingdoms.setValue(pWar.countKingdoms());
		cities.setValue(pWar.countCities());
		renown.setValue(pWar.getRenown());
		dead.setValue((int)pWar.getTotalDeaths());
		age.setValue(pWar.getAge());
		duration.setValue(pWar.getDuration());
		((Component)total_war_icon).gameObject.SetActive(false);
		clearBanners();
		WarWinner winner = pWar.data.winner;
		showKingdomBanners(pWar.getAttackers(), pool_mini_banners_attackers, pLeft: false, winner == WarWinner.Attackers, winner == WarWinner.Defenders);
		showKingdomBanners(pWar.getDiedAttackers(), pool_mini_banners_attackers);
		showKingdomBanners(pWar.getPastAttackers(), pool_mini_banners_attackers, pLeft: true);
		showKingdomBanners(pWar.getDefenders(), pool_mini_banners_defenders, pLeft: false, winner == WarWinner.Defenders, winner == WarWinner.Attackers);
		showKingdomBanners(pWar.getDiedDefenders(), pool_mini_banners_defenders);
		showKingdomBanners(pWar.getPastDefenders(), pool_mini_banners_defenders, pLeft: true);
		bool flag = pWar.countAttackersPopulation() > pWar.countDefendersPopulation();
		bool flag2 = pWar.getDeadDefenders() > pWar.getDeadAttackers();
		bool flag3 = pWar.countAttackersWarriors() > pWar.countDefendersWarriors();
		pWar.countAttackersCities();
		pWar.countDefendersCities();
		setIconValue("i_attackers_population", pWar.countAttackersPopulation(), flag ? "#43FF43" : "#FB2C21");
		setIconValue("i_attackers_army", pWar.countAttackersWarriors(), flag3 ? "#43FF43" : "#FB2C21");
		setIconValue("i_attackers_dead", pWar.getDeadAttackers(), flag2 ? "#43FF43" : "#FB2C21");
		setIconValue("i_defenders_population", pWar.countDefendersPopulation(), flag ? "#FB2C21" : "#43FF43");
		setIconValue("i_defenders_army", pWar.countDefendersWarriors(), flag3 ? "#FB2C21" : "#43FF43");
		setIconValue("i_defenders_dead", pWar.getDeadDefenders(), flag2 ? "#FB2C21" : "#43FF43");
	}

	private void checkCreation()
	{
		if (pool_mini_banners_attackers == null)
		{
			pool_mini_banners_attackers = new ObjectPoolGenericMono<KingdomBanner>(prefabMiniKingdomBanner, gridAttackers.transform);
			pool_mini_banners_defenders = new ObjectPoolGenericMono<KingdomBanner>(prefabMiniKingdomBanner, gridDefenders.transform);
		}
	}

	public void clearBanners()
	{
		checkCreation();
		pool_mini_banners_attackers.clear();
		pool_mini_banners_defenders.clear();
	}

	public void showKingdomBanners(IEnumerable<Kingdom> pList, ObjectPoolGenericMono<KingdomBanner> pPool, bool pLeft = false, bool pWinner = false, bool pLoser = false)
	{
		checkCreation();
		int num = 6 - pPool.countActive();
		if (num <= 0)
		{
			return;
		}
		foreach (Kingdom p in pList)
		{
			if (p != null && p.isAlive())
			{
				KingdomBanner next = pPool.getNext();
				next.load(p);
				if (pLeft)
				{
					next.hasLeftWar();
				}
				if (pWinner)
				{
					next.hasWon();
				}
				if (pLoser)
				{
					next.hasLost();
				}
				((Behaviour)((Component)next).GetComponentInChildren<RotateOnHover>()).enabled = true;
				if (num-- <= 0)
				{
					break;
				}
			}
		}
	}

	public void setIconValue(string pName, int pMainVal, string pColor)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Component)this).transform.FindRecursive(pName);
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)("No icon with this name! " + pName));
			return;
		}
		Transform val2 = val.Find("Container/Text");
		if ((Object)(object)val2 == (Object)null)
		{
			Debug.LogError((object)(pName + " doesn't have Container/Text"));
			return;
		}
		((Component)val2).gameObject.SetActive(true);
		Text component = ((Component)val2).GetComponent<Text>();
		CountUpOnClick component2 = ((Component)val).GetComponent<CountUpOnClick>();
		((Graphic)component).color = Toolbox.makeColor(pColor);
		component2.setValue(pMainVal);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		pool_mini_banners_attackers?.clear();
		pool_mini_banners_defenders?.clear();
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "war", new TooltipData
		{
			war = meta_object
		});
	}
}
