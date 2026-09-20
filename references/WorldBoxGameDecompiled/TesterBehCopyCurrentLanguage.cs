using System;
using System.IO;
using UnityEngine;
using ai.behaviours;

public class TesterBehCopyCurrentLanguage : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		string language = LocalizedTextManager.instance.language;
		string screenshotFolder = TesterBehScreenshotFolder.getScreenshotFolder(language);
		string text = "locales/" + language;
		Object obj = Resources.Load(text);
		string text2 = ((TextAsset)((obj is TextAsset) ? obj : null)).text;
		Console.WriteLine("[" + Date.TimeNow() + "] Copying language: " + text + " to " + screenshotFolder + "/" + language + ".json");
		File.WriteAllText(screenshotFolder + "/" + language + ".json", text2);
		return BehResult.Continue;
	}
}
