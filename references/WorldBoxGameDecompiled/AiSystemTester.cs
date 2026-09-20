public class AiSystemTester : AiSystem<AutoTesterBot, JobTesterAsset, BehaviourTaskTester, BehaviourActionTester, BehaviourTesterCondition>
{
	public AiSystemTester(AutoTesterBot pObject)
		: base(pObject)
	{
		jobs_library = AssetManager.tester_jobs;
		task_library = AssetManager.tester_tasks;
	}
}
