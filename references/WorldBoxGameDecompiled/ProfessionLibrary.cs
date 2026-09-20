using System;
using System.Collections.Generic;

[Serializable]
public class ProfessionLibrary : AssetLibrary<ProfessionAsset>
{
	public static readonly UnitProfession[] list_enum_profession_ids = (UnitProfession[])Enum.GetValues(typeof(UnitProfession));

	private Dictionary<UnitProfession, ProfessionAsset> _dict_profession_id = new Dictionary<UnitProfession, ProfessionAsset>();

	public override void init()
	{
		base.init();
		add(new ProfessionAsset
		{
			id = "nothing",
			profession_id = UnitProfession.Nothing
		});
		add(new ProfessionAsset
		{
			id = "unit",
			profession_id = UnitProfession.Unit,
			is_civilian = true
		});
		add(new ProfessionAsset
		{
			id = "warrior",
			profession_id = UnitProfession.Warrior,
			can_capture = true,
			cancel_when_no_city = true
		});
		t.addDecision("warrior_try_join_army_group");
		t.addDecision("check_warrior_limit");
		t.addDecision("city_walking_to_danger_zone");
		t.addDecision("warrior_army_captain_idle_walking_city");
		t.addDecision("warrior_army_captain_waiting");
		t.addDecision("warrior_army_leader_move_random");
		t.addDecision("warrior_army_leader_move_to_attack_target");
		t.addDecision("warrior_army_follow_leader");
		t.addDecision("warrior_random_move");
		t.addDecision("check_warrior_transport");
		t.addDecision("warrior_train_with_dummy");
		add(new ProfessionAsset
		{
			id = "king",
			profession_id = UnitProfession.King,
			can_capture = true
		});
		t.addDecision("king_check_new_city_foundation");
		t.addDecision("king_change_kingdom_language");
		t.addDecision("king_change_kingdom_culture");
		t.addDecision("king_change_kingdom_religion");
		t.addDecision("claim_land");
		add(new ProfessionAsset
		{
			id = "leader",
			profession_id = UnitProfession.Leader,
			can_capture = true
		});
		t.addDecision("leader_change_city_language");
		t.addDecision("leader_change_city_culture");
		t.addDecision("leader_change_city_religion");
		t.addDecision("claim_land");
	}

	public override void linkAssets()
	{
		base.linkAssets();
		linkDecisions();
	}

	private void linkDecisions()
	{
		foreach (ProfessionAsset item in list)
		{
			item.linkDecisions();
		}
	}

	public override ProfessionAsset add(ProfessionAsset pAsset)
	{
		_dict_profession_id.Add(pAsset.profession_id, pAsset);
		return base.add(pAsset);
	}

	public virtual ProfessionAsset get(UnitProfession pID)
	{
		return _dict_profession_id[pID];
	}
}
