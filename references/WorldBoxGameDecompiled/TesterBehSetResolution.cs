using ai.behaviours;

public class TesterBehSetResolution : BehaviourActionTester
{
	private int width;

	private int height;

	private string name;

	public TesterBehSetResolution(int pWidth, int pHeight, string pName = null)
	{
		width = pWidth;
		height = pHeight;
		name = pName;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		return BehResult.Continue;
	}
}
