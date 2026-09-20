using ai.behaviours;

public class TesterBehSaveWorldIfEmpty : BehaviourActionTester
{
	private int slot;

	public TesterBehSaveWorldIfEmpty(int pSlot)
	{
		slot = pSlot;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		SaveManager.setCurrentSlot(slot);
		if (!SaveManager.currentSlotExists())
		{
			SaveManager.saveWorldToDirectory(SaveManager.currentSavePath);
		}
		return BehResult.Continue;
	}
}
