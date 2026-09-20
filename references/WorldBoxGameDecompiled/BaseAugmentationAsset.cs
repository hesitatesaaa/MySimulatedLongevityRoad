using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class BaseAugmentationAsset : BaseUnlockableAsset
{
	[DefaultValue(true)]
	public bool can_be_given = true;

	[DefaultValue(true)]
	public bool can_be_removed = true;

	[DefaultValue(true)]
	public bool show_in_meta_editor = true;

	public WorldActionTrait action_on_object_remove;

	public WorldAction action_special_effect;

	public WorldActionTrait action_on_augmentation_add;

	public WorldActionTrait action_on_augmentation_remove;

	public WorldActionTrait action_on_augmentation_load;

	[DefaultValue(1f)]
	public float special_effect_interval = 1f;

	public AttackAction action_attack_target;

	public string group_id;

	public int priority;

	public string special_locale_id;

	[NonSerialized]
	public CombatActionHolder combat_actions;

	[NonSerialized]
	public List<SpellAsset> spells;

	public List<string> combat_actions_ids;

	[NonSerialized]
	public DecisionAsset[] decisions_assets;

	public List<string> decision_ids;

	public List<string> spells_ids { get; set; }

	public bool hasDecisions()
	{
		return decisions_assets != null;
	}

	public bool hasCombatActions()
	{
		return combat_actions != null;
	}

	public bool hasSpells()
	{
		return spells != null;
	}

	public void addDecision(string pID)
	{
		if (decision_ids == null)
		{
			decision_ids = new List<string>();
		}
		decision_ids.Add(pID);
	}

	public void addSpell(string pSpell)
	{
		if (spells_ids == null)
		{
			spells_ids = new List<string>();
		}
		spells_ids.Add(pSpell);
	}

	public void addCombatAction(string pCombatActionID)
	{
		if (combat_actions_ids == null)
		{
			combat_actions_ids = new List<string>();
		}
		combat_actions_ids.Add(pCombatActionID);
	}

	public void linkCombatActions()
	{
		if (combat_actions_ids != null && combat_actions_ids.Count != 0)
		{
			combat_actions = new CombatActionHolder();
			combat_actions.fillFromIDS(combat_actions_ids);
		}
	}

	public void linkSpells()
	{
		if (spells_ids == null || spells_ids.Count == 0)
		{
			return;
		}
		spells = new List<SpellAsset>();
		foreach (string spells_id in spells_ids)
		{
			SpellAsset spellAsset = AssetManager.spells.get(spells_id);
			if (spellAsset != null)
			{
				spells.Add(spellAsset);
			}
		}
	}

	public virtual BaseCategoryAsset getGroup()
	{
		throw new NotImplementedException();
	}
}
