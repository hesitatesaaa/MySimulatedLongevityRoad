using ai.behaviours;

public class TesterBehClearWindowTest : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		ScrollWindow.clear();
		Config.selected_trait_editor = null;
		SaveManager.currentWorkshopMapData = null;
		return BehResult.Continue;
	}
}
