using System;
using System.Collections.Generic;

[Serializable]
public class SpellAsset : Asset
{
	public float health_ratio;

	public int cost_mana;

	public float chance = 1f;

	public float min_distance;

	public CastTarget cast_target;

	public CastEntity cast_entity = CastEntity.Both;

	public AttackAction action;

	public List<string> decision_ids;

	[NonSerialized]
	public DecisionAsset[] decisions_assets;

	public bool can_be_used_in_combat;

	public bool hasDecisions()
	{
		return decisions_assets != null;
	}

	public void addDecision(string pID)
	{
		if (decision_ids == null)
		{
			decision_ids = new List<string>();
		}
		decision_ids.Add(pID);
	}
}
