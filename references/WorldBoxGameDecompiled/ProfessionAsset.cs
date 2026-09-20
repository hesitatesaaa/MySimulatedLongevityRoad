using System;
using System.Collections.Generic;

[Serializable]
public class ProfessionAsset : Asset
{
	public UnitProfession profession_id;

	public bool can_capture;

	public bool is_civilian;

	public bool cancel_when_no_city;

	public List<string> decision_ids;

	[NonSerialized]
	public DecisionAsset[] decisions_assets;

	public bool hasDecisions()
	{
		return decision_ids != null;
	}

	public bool hasDecision(string pID)
	{
		return decision_ids.Contains(pID);
	}

	public void linkDecisions()
	{
		if (decision_ids != null)
		{
			decisions_assets = new DecisionAsset[decision_ids.Count];
			for (int i = 0; i < decision_ids.Count; i++)
			{
				string pID = decision_ids[i];
				DecisionAsset decisionAsset = AssetManager.decisions_library.get(pID);
				decisions_assets[i] = decisionAsset;
			}
		}
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
