using System.Collections;
using UnityEngine;

public class SubspeciesStatsElement : SubspeciesElement, IStatsElement, IRefreshElement
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
		if (base.subspecies != null && base.subspecies.isAlive())
		{
			_stats_icons.showGeneralIcons<Subspecies, SubspeciesData>(base.subspecies);
			setIconValue("i_offspring", base.subspecies.base_stats["offspring"]);
			setIconValue("i_mutation_rate", base.subspecies.base_stats_meta["mutation"]);
			float num = base.subspecies.base_stats["lifespan"];
			float pMainVal = num + base.subspecies.base_stats_male["lifespan"];
			float pMainVal2 = num + base.subspecies.base_stats_female["lifespan"];
			int num2 = (int)((float)(int)base.subspecies.base_stats["intelligence"] * SimGlobals.m.MANA_PER_INTELLIGENCE);
			setIconValue("i_lifespan_male", pMainVal);
			setIconValue("i_lifespan_female", pMainVal2);
			setIconValue("i_maturation", base.subspecies.getMaturationTimeMonths());
			setIconValue("i_mana", num2);
			showIconSubspecies("i_birth_rate", "birth_rate");
			showIconSubspecies("i_health", "health");
			showIconSubspecies("i_armor", "armor");
			showIconSubspecies("i_speed", "speed");
			showIconSubspecies("i_damage", "damage");
			showIconSubspecies("i_critical_chance", "critical_chance");
			showIconSubspecies("i_attack_speed", "attack_speed");
			showIconSubspecies("i_diplomacy", "diplomacy");
			showIconSubspecies("i_warfare", "warfare");
			showIconSubspecies("i_stewardship", "stewardship");
			showIconSubspecies("i_intelligence", "intelligence");
			showIconSubspecies("i_stamina", "stamina");
			int num3 = base.subspecies.countMainKingdoms();
			int num4 = base.subspecies.countMainCities();
			setIconValue("i_kingdoms", num3);
			setIconValue("i_villages", num4);
		}
		yield break;
	}

	private void showIconSubspecies(string pFieldID, string pStatID)
	{
		ActorAsset actorAsset = base.subspecies.getActorAsset();
		int num = (int)base.subspecies.nucleus.getStats().get(pStatID);
		int num2 = (int)actorAsset.base_stats[pStatID];
		foreach (GenomePart genome_part in actorAsset.genome_parts)
		{
			if (genome_part.id == pStatID)
			{
				num2 += (int)genome_part.value;
				break;
			}
		}
		string text = ((num > num2) ? "#43FF43" : ((num >= num2) ? string.Empty : "#FB2C21"));
		float pMainVal = base.subspecies.base_stats[pStatID];
		string pColor = text;
		setIconValue(pFieldID, pMainVal, null, pColor);
	}
}
