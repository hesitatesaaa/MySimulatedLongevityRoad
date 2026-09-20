using UnityEngine;
using ai.behaviours;

public class TesterBehCheckWorld : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		string text = checkTestData();
		if (!string.IsNullOrEmpty(text))
		{
			Debug.Log((object)("Errors:\n" + text));
			pObject.ai.setTask("shutdown", pClean: true, pCleanJob: true);
			return BehResult.Skip;
		}
		return BehResult.Continue;
	}

	private static string checkTestData()
	{
		string text = string.Empty;
		if (BehaviourActionBase<AutoTesterBot>.world.cities.Count == 0)
		{
			text += "cities list is empty - load a map with cities present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.clans.Count == 0)
		{
			text += "clans list is empty - load a map with clans present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.plots.Count == 0)
		{
			text += "plots list is empty - load a map with plots present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.alliances.Count == 0)
		{
			text += "alliances list is empty - load a map with alliances present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.wars.Count == 0)
		{
			text += "wars list is empty - load a map with wars present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.kingdoms.Count == 0)
		{
			text += "kingdoms list is empty - load a map with cultures present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.cultures.Count == 0)
		{
			text += "cultures list is empty - load a map with cultures present\n";
		}
		if (BehaviourActionBase<AutoTesterBot>.world.units.Count == 0)
		{
			text += "units list is empty - load a map with world present\n";
		}
		return text + "You can only test this in the editor\n";
	}
}
