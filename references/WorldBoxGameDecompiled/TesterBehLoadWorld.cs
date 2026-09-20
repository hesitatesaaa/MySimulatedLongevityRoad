using ai.behaviours;

public class TesterBehLoadWorld : BehaviourActionTester
{
	private int slot;

	private string fallback;

	public TesterBehLoadWorld(int pSlot, string pFallback = null)
	{
		slot = pSlot;
		fallback = pFallback;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		SaveManager.setCurrentSlot(slot);
		if (!SaveManager.currentSlotExists())
		{
			SaveManager.loadMapFromResources(fallback);
		}
		else
		{
			BehaviourActionBase<AutoTesterBot>.world.save_manager.startLoadSlot();
		}
		return BehResult.Continue;
	}
}
