using System.IO;
using ai.behaviours;

public class TesterBehScreenshotFolder : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		string screenshotFolder = getScreenshotFolder(LocalizedTextManager.instance.language);
		if (!Directory.Exists(screenshotFolder))
		{
			Directory.CreateDirectory(screenshotFolder);
		}
		return BehResult.Continue;
	}

	internal static string getScreenshotFolder(string pLanguage)
	{
		return "GenAssets/Windows/" + pLanguage;
	}
}
