public class DecisionHelper
{
	internal static UtilityBasedDecisionSystem decision_system = new UtilityBasedDecisionSystem();

	public static bool makeDecisionFor(Actor pActor, out string pLastDecisionID)
	{
		pLastDecisionID = string.Empty;
		if (pActor.isStatsDirty())
		{
			pActor.setTask("wait");
			return false;
		}
		DecisionAsset decisionAsset = decision_system.useOn(pActor);
		if (decisionAsset == null)
		{
			return false;
		}
		pLastDecisionID = decisionAsset.id;
		string pTaskId = decisionAsset.id;
		if (!string.IsNullOrEmpty(decisionAsset.task_id))
		{
			pTaskId = decisionAsset.task_id;
		}
		pActor.setTask(pTaskId);
		return true;
	}

	public static void runSimulation(Actor pActor)
	{
		decision_system.useOn(pActor, pGameplay: false);
	}

	public static void runSimulationForMindTab(Actor pActor)
	{
		decision_system.useOn(pActor, pGameplay: false);
	}
}
